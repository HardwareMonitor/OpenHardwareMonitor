using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using OpenHardwareMonitor.Interop;

namespace OpenHardwareMonitor.Hardware.Network;

internal sealed class Network : Hardware
{
    private readonly Sensor _dataDownloaded;
    private readonly Sensor _dataUploaded;
    private readonly Sensor _downloadSpeed;
    private readonly Sensor _connectionSpeed;
    private readonly Sensor _dhcp;
    private readonly Sensor _dns;
    private readonly Sensor _gateway;
    private readonly Sensor _incomingDiscards;
    private readonly Sensor _incomingErrors;
    private readonly Sensor _interfaceType;
    private readonly Sensor _ip;
    private readonly Sensor _mac;
    private readonly Sensor _networkUtilization;
    private readonly Sensor _outgoingDiscards;
    private readonly Sensor _outgoingErrors;
    private readonly Sensor _receiveLinkRate;
    private readonly Sensor _signalStrength;
    private readonly Sensor _ssid;
    private readonly Sensor _status;
    private readonly Sensor _subnetMask;
    private readonly Sensor _transmitLinkRate;
    private readonly Sensor _uploadSpeed;
    private IntPtr _wlanHandle;
    private long _lastDataUpdate;

    // How long IP, MAC, Subnet Mask, Gateway, DNS, DHCP, Status, Type and Connection Speed stay valid.
    private const double PropertiesRefreshIntervalSeconds = 10;

    private long _bytesDownloaded;
    private long _bytesUploaded;
    private long _lastTick;

    public Network(NetworkInterface networkInterface, ISettings settings)
        : base(networkInterface.Name, new Identifier("nic", networkInterface.Id), settings)
    {
        NetworkInterface = networkInterface;
        _dataUploaded = new Sensor("Data Uploaded", 2, SensorType.Data, this, settings);
        ActivateSensor(_dataUploaded);
        _dataDownloaded = new Sensor("Data Downloaded", 3, SensorType.Data, this, settings);
        ActivateSensor(_dataDownloaded);
        _uploadSpeed = new Sensor("Upload Speed", 7, SensorType.Throughput, this, settings);
        ActivateSensor(_uploadSpeed);
        _downloadSpeed = new Sensor("Download Speed", 8, SensorType.Throughput, this, settings);
        ActivateSensor(_downloadSpeed);
        _networkUtilization = new Sensor("Network Utilization", 1, SensorType.Load, this, settings);
        ActivateSensor(_networkUtilization);
        _ip = new Sensor("IP", 9, SensorType.Text, this, settings);
        ActivateSensor(_ip);
        _mac = new Sensor("MAC", 11, SensorType.Text, this, settings);
        ActivateSensor(_mac);
        _subnetMask = CreateTextSensor("Subnet Mask", 15, settings);
        _gateway = CreateTextSensor("Gateway", 13, settings);
        _dns = CreateTextSensor("DNS", 14, settings);
        _dhcp = CreateTextSensor("DHCP", 25, settings);
        _status = CreateTextSensor("Status", 16, settings);
        _interfaceType = CreateTextSensor("Type", 17, settings);
        _connectionSpeed = new Sensor("Connection Speed", 12, SensorType.Throughput, this, settings);
        ActivateSensor(_connectionSpeed);
        _incomingErrors = CreateCounterSensor("Incoming Errors", 21, settings);
        _incomingDiscards = CreateCounterSensor("Incoming Discards", 22, settings);
        _outgoingErrors = CreateCounterSensor("Outgoing Errors", 23, settings);
        _outgoingDiscards = CreateCounterSensor("Outgoing Discards", 24, settings);

        if (networkInterface.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
        {
            _wlanHandle = WlanApi.Open();
            if (_wlanHandle != IntPtr.Zero)
            {
                _signalStrength = new Sensor("Signal Strength", 10, SensorType.Level, this, settings);
                ActivateSensor(_signalStrength);
                _ssid = CreateTextSensor("SSID", 18, settings);
                _receiveLinkRate = new Sensor("Receive Link Rate", 19, SensorType.Throughput, this, settings);
                ActivateSensor(_receiveLinkRate);
                _transmitLinkRate = new Sensor("Transmit Link Rate", 20, SensorType.Throughput, this, settings);
                ActivateSensor(_transmitLinkRate);
            }
        }

        _bytesUploaded = NetworkInterface.GetIPStatistics().BytesSent;
        _bytesDownloaded = NetworkInterface.GetIPStatistics().BytesReceived;
        _lastTick = Stopwatch.GetTimestamp();
    }

    public override HardwareType HardwareType
    {
        get { return HardwareType.Network; }
    }

    internal NetworkInterface NetworkInterface { get; private set; }

    public override void Close()
    {
        WlanApi.Close(_wlanHandle);
        _wlanHandle = IntPtr.Zero;
        base.Close();
    }

    private Sensor CreateTextSensor(string name, int index, ISettings settings)
    {
        Sensor sensor = new(name, index, SensorType.Text, this, settings);
        ActivateSensor(sensor);
        return sensor;
    }

    private Sensor CreateCounterSensor(string name, int index, ISettings settings)
    {
        Sensor sensor = new(name, index, SensorType.IntFactor, this, settings);
        ActivateSensor(sensor);
        return sensor;
    }

    private static string Join(IEnumerable<IPAddress> addresses)
    {
        string result = string.Join(", ", addresses.Select(static x => x.ToString()));
        return result.Length == 0 ? null : result;
    }

    private string GetMac()
    {
        byte[] bytes = NetworkInterface.GetPhysicalAddress().GetAddressBytes();
        return bytes.Length == 0 ? null : string.Join(":", bytes.Select(static x => x.ToString("X2")));
    }

    private string GetIp()
    {
        var addresses = NetworkInterface.GetIPProperties().UnicastAddresses.Select(static x => x.Address).ToList();

        // Prefer IPv4; fall back to a non-link-local IPv6 address.
        return (addresses.FirstOrDefault(static x => x.AddressFamily == AddressFamily.InterNetwork)
             ?? addresses.FirstOrDefault(static x => x.AddressFamily == AddressFamily.InterNetworkV6 && !x.IsIPv6LinkLocal))
            ?.ToString();
    }

    private static string GetDhcp(IPInterfaceProperties ipProperties)
    {
        string servers = Join(ipProperties.DhcpServerAddresses);
        if (servers != null)
            return servers;

        try
        {
            return ipProperties.GetIPv4Properties().IsDhcpEnabled ? "Enabled" : "Disabled";
        }
        catch (NetworkInformationException)
        {
            // IPv4 is not available on this interface.
            return null;
        }
    }

    private void UpdateWireless()
    {
        WlanApi.Connection? connection = _wlanHandle != IntPtr.Zero && Guid.TryParse(NetworkInterface.Id, out Guid interfaceGuid)
            ? WlanApi.GetConnection(_wlanHandle, interfaceGuid)
            : null;

        _signalStrength.Value = connection?.SignalQuality;
        _ssid.Text = connection?.Ssid;

        // The link rates are reported in Kbps; throughput sensors use bytes per second.
        _receiveLinkRate.Value = connection?.RxRateKbps * 1000f / 8;
        _transmitLinkRate.Value = connection?.TxRateKbps * 1000f / 8;
    }

    private void UpdateProperties()
    {
        _ip.Text = GetIp();
        _mac.Text = GetMac();

        IPInterfaceProperties ipProperties = NetworkInterface.GetIPProperties();
        UnicastIPAddressInformation ipv4 = ipProperties.UnicastAddresses.FirstOrDefault(static x => x.Address.AddressFamily == AddressFamily.InterNetwork);
        _subnetMask.Text = ipv4?.IPv4Mask?.ToString();
        _gateway.Text = Join(ipProperties.GatewayAddresses.Select(static x => x.Address));
        _dns.Text = Join(ipProperties.DnsAddresses);
        _dhcp.Text = GetDhcp(ipProperties);
        _status.Text = NetworkInterface.OperationalStatus.ToString();
        _interfaceType.Text = NetworkInterface.NetworkInterfaceType.ToString();
        _connectionSpeed.Value = NetworkInterface.Speed > 0 ? NetworkInterface.Speed : null;
    }

    public override void Update()
    {
        try
        {
            if (NetworkInterface == null)
                return;

            long newTick = Stopwatch.GetTimestamp();

            // NetworkInterface is a snapshot: status, addresses and speed stay stale until it is re-enumerated.
            if (_lastDataUpdate == 0 || (newTick - _lastDataUpdate) / (double)Stopwatch.Frequency >= PropertiesRefreshIntervalSeconds)
            {
                string id = NetworkInterface.Id;
                NetworkInterface = NetworkInterface.GetAllNetworkInterfaces().FirstOrDefault(x => x.Id == id) ?? NetworkInterface;
                UpdateProperties();
                _lastDataUpdate = newTick;
            }

            double dt = new TimeSpan(newTick - _lastTick).TotalSeconds;

            IPv4InterfaceStatistics interfaceStats = NetworkInterface.GetIPv4Statistics();

            // Report out the number of GB (2^30 Bytes) that this interface has up/downloaded. Note
            // that these values can reset back at zero (eg: after waking from sleep).
            _dataUploaded.Value = (float)(interfaceStats.BytesSent / (double)0x40000000);
            _dataDownloaded.Value = (float)(interfaceStats.BytesReceived / (double)0x40000000);

            // Detect a reset in interface stats if the new total is less than what was previously
            // seen. While setting the previous values to zero doesn't encapsulate the value the
            // instant before the reset, it is the best approximation we have.
            if (interfaceStats.BytesSent < _bytesUploaded || interfaceStats.BytesReceived < _bytesDownloaded)
            {
                _bytesUploaded = 0;
                _bytesDownloaded = 0;
            }

            long dBytesUploaded = interfaceStats.BytesSent - _bytesUploaded;
            long dBytesDownloaded = interfaceStats.BytesReceived - _bytesDownloaded;

            // Upload and download speeds are reported as the number of bytes transfered over the
            // time difference since the last report. In this way, the values represent the average
            // number of bytes up/downloaded in a second.
            _uploadSpeed.Value = (float)(dBytesUploaded / dt);
            _downloadSpeed.Value = (float)(dBytesDownloaded / dt);

            // Network speed is in bits per second, so when calculating the load on the NIC we first
            // grab the total number of bits up/downloaded
            long dbits = (dBytesUploaded + dBytesDownloaded) * 8;

            // Converts the ratio of total bits transferred over time over theoretical max bits
            // transfer rate into a percentage load
            double load = (dbits / dt / NetworkInterface.Speed) * 100;

            // Finally clamp the value between 0% and 100% to avoid reporting nonsensical numbers
            _networkUtilization.Value = (float)Math.Min(Math.Max(load, 0), 100);

            _incomingErrors.Value = interfaceStats.IncomingPacketsWithErrors;
            _incomingDiscards.Value = interfaceStats.IncomingPacketsDiscarded;
            _outgoingErrors.Value = interfaceStats.OutgoingPacketsWithErrors;
            _outgoingDiscards.Value = interfaceStats.OutgoingPacketsDiscarded;

            if (_signalStrength != null)
                UpdateWireless();

            // Store the recorded values and time, so they can be used in the next update
            _bytesUploaded = interfaceStats.BytesSent;
            _bytesDownloaded = interfaceStats.BytesReceived;
            _lastTick = newTick;
        }
        catch (NetworkInformationException networkInformationException) when (unchecked(networkInformationException.HResult == (int)0x80004005))
        {
            foreach (NetworkInterface networkInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (networkInterface.Id.Equals(NetworkInterface?.Id))
                {
                    NetworkInterface = networkInterface;
                    break;
                }
            }
        }
    }
}
