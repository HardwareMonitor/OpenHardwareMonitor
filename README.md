# Open Hardware Monitor

[![Release](https://img.shields.io/github/v/release/HardwareMonitor/openhardwaremonitor)](https://github.com/HardwareMonitor/openhardwaremonitor/releases/latest)
[![Downloads](https://img.shields.io/github/downloads/HardwareMonitor/openhardwaremonitor/total?color=ff4f42)](https://github.com/HardwareMonitor/openhardwaremonitor/releases)
[![Last commit](https://img.shields.io/github/last-commit/HardwareMonitor/openhardwaremonitor?color=00AD00)](https://github.com/HardwareMonitor/openhardwaremonitor/commits/master)
[![](https://img.shields.io/badge/WINDOWS-7%20%E2%80%93%2011-blue)](https://endoflife.date/windows)
[![](https://img.shields.io/badge/SERVER-2012%20%E2%80%93%202025-blue)](https://endoflife.date/windows-server)

[![Nuget](https://img.shields.io/nuget/v/OpenHardwareMonitorLib)](https://www.nuget.org/packages/OpenHardwareMonitorLib/)
[![Nuget](https://img.shields.io/nuget/dt/OpenHardwareMonitorLib?label=nuget-downloads)](https://www.nuget.org/packages/OpenHardwareMonitorLib/)


Open Hardware Monitor is a free, open source Windows application that reads hardware sensors of a computer (temperatures, fan speeds, voltages, power, clock speeds, load, data throughput and more) and shows them in a single tree view. The same data is available through tray icons, a desktop gadget, a built-in web server, WMI, CSV logs and a .NET library.

This application is based on the original [OpenHardwareMonitor](https://github.com/openhardwaremonitor/openhardwaremonitor) project. It adds support for new hardware, a modern UI (themes, tray icons, gadget), remote access and logging.

> [!NOTE]
> Some sensors are only available when the application runs as administrator, because it needs a kernel driver to read hardware registers.

## Features

### Supported hardware

Each group can be switched on/off in `File > Hardware`. Only enabled groups are queried, so you can disable what you don't need to reduce load and startup time.

1. **Motherboards** - temperatures, voltages, fan speeds and fan controls from Super I/O chips and embedded controllers of many ASUS, MSI, Gigabyte, ASRock and other boards.
2. **CPU** - Intel and AMD processors: per-core temperature, clocks, load, power and voltages. Intel hybrid CPUs show Performance and Efficient cores separately.
3. **RAM** - physical and virtual memory usage, DIMM temperature and memory timings.
4. **GPU** - NVIDIA, AMD and Intel (integrated and discrete) graphics cards: temperature (including hot spot and memory junction), clocks, load, fan, power, VRAM usage and bandwidth.
5. **Storage devices** - HDD, SSD and NVMe drives: temperature, S.M.A.R.T. attributes, wear level, data written and drive load.
6. **Network** - network adapters: upload/download speed, transferred data and bandwidth utilization.
7. **Fan controllers and liquid cooling** - USB devices from Aqua Computer, NZXT (Kraken, Grid), MSI, Razer, AeroCool, Arctic, Heatmaster and T-Balancer.
8. **Power supplies** - Corsair and MSI PSUs with a USB interface: power, voltages, currents, temperatures and fan.
9. **Batteries** - laptop battery charge level, voltage, current, power and capacity.

Motherboards, CPU, RAM and batteries are enabled by default. Other groups are disabled until you turn them on.

### Main window

- **Sensor tree** - hardware devices are grouped by type; every sensor shows its current `Value`, `Min` and `Max`. Columns can be toggled in `View > Columns`.
- **Reset Min/Max** (`View`) - restarts min/max tracking for all sensors.
- **Expand/Collapse All Nodes** and **Show Hidden Sensors** (`View`).
- **Hide Menu** (`View`) - hides the menu bar; press `Alt` to show it again.
- **Update Interval** (`Options`) - how often sensors are polled, from 250 ms to 10 s.
- **Throttle ATA Storage** (`Options`) - polls S.M.A.R.T. data of ATA drives less often to avoid slowing down disk access.
- **Sensor Values Time Window** (`Options`) - how much sensor history (30 s to 24 h) is kept in memory.
- **Save Report** (`File`) - saves a text report with full hardware and sensor information; attach it to bug reports and compatibility requests.

### Sensor actions

Right-click a sensor (or use a hotkey) to:

1. `Rename` (`F2`) a sensor or hardware node.
2. `Hide` (`Ctrl+H`) a sensor. Hidden sensors disappear from the tree and the web server until `Show Hidden Sensors` is enabled.
3. Change the `Pen Color` and reset it (`Ctrl+R`).
4. `Show in Tray` (`Ctrl+T`) - add a separate tray icon for the sensor.
5. `Show in Gadget` (`Ctrl+G`) - add the sensor to the desktop gadget.
6. Open `Parameters` (`Ctrl+P`) - available for sensors with adjustable parameters.

Other hotkeys: `F5` - reset, `Ctrl+W` - exit, `F1` - about, `Ctrl+F1` - project site, `Ctrl+U` - check for updates.

### Tray icons and gadget

- **Tray icons** - one icon per selected sensor with the value drawn in the icon and details in the tooltip.
- **Icon kind** - for percent sensors choose `Value`, `Bar` or `Pie` from the icon's context menu; other sensors always show the value.
- **Gadget** - a small window on the desktop with selected sensors, enabled in `View > Show Gadget`.
- **Window behavior** - `Start Minimized`, `Minimize To Tray` and `Minimize On Close` (`Options`).

### Themes and units

- `Light` and `Dark` themes, with an auto mode that follows the Windows theme (`Options > Theme`).
- Custom color themes from external files (see below).
- Temperature unit: `Celsius` or `Fahrenheit` (`Options > Temperature Unit`).
- `Increase Font Size` / `Decrease Font Size` for the tree view.

#### Custom themes

To add a custom theme, create a `themes` folder next to the executable file and place any `{themeName}.json` files there. Example:
```json
{
  "DisplayName": "Custom Theme",
  "DarkMode": true,
  "BackgroundColor": "#1E1E1E",
  "ForegroundColor": "#E9E9E9",
  "HyperlinkColor": "#00D980",
  "SelectedBackgroundColor": "#4CBB17",
  "SelectedForegroundColor": "#000000",
  "LineColor": "#262626",
  "StrongLineColor": "#454545",
  "WarnColor": "#FF4500"
}
```
Restart the app so it scans for new theme files.

### Remote web server

Enable it in `Options > Remote Web Server > Run`; `Open` opens the page in your browser. The default port is `8085`; change it with `Port`. `Authentication` enables HTTP basic authentication with a user name and password.

The server provides:

1. A web page with the live sensor tree, available from any device in the network.
2. `/data.json` - the whole sensor tree as JSON, with display values and raw values (`RawValue`, `RawMin`, `RawMax`).
3. `/metrics` - the same data in OpenMetrics (Prometheus) format for Grafana and other monitoring systems.
4. `/Sensor?action=Get&id={SensorId}` - current value, min and max of one sensor.
5. `/Sensor?action=Set&id={SensorId}&value={number}` - sets a control sensor (fan/pump speed); `value=null` returns it to the default mode.
6. `/Sensor?action=ResetMinMax&id={SensorId}` and `/ResetAllMinMax` - reset min/max for one sensor or for all of them.

Hidden sensors are not exposed. Use authentication if the port is reachable outside your local network. A usage example is in `OpenHardwareMonitor/TestScripts/basicrest.py`.

### WMI

While the app is running, hardware and sensors are published to WMI in the `root\OpenHardwareMonitor` namespace (`Hardware` and `Sensor` classes), so PowerShell and other tools can read values without HTTP. An example is in `OpenHardwareMonitor/TestScripts/basicwmi.py`.

### Logging

`Options > Log Sensors` writes sensor values to CSV files.

1. `Log Folder...` - destination folder; it is created automatically if it doesn't exist.
2. `Logging Interval` - from 1 s to 6 h.
3. `File rotation method` - `Per session` starts a new file when logging starts or after a gap in logging (for example, after sleep); `Daily` starts a new file every day.

Files are named `OpenHardwareMonitorLog-yyyy-MM-dd[-N].csv`. The first row contains sensor identifiers, the second row contains sensor names, and every following row is one sample with a timestamp.

### System integration

- `Run On Windows Startup` - starts the app after login with administrator rights through a scheduled task (with a 5-second delay), so no UAC prompt appears.
- `Portable mode` (`File`) - stores the settings and the temporary driver file next to the executable file.
- `Auto-Update Application` (`Options`) and `Check for updates` (`Help`) - get new versions from GitHub releases.
- Single instance: launching the app again shows the already running window.
- x64 and x86 builds for Windows 7 - 11 and Windows Server 2012 - 2025.

### What does it look like?

Here's a preview of the app's UI with `Light`/`Dark` themes running on Windows 10:

[<img src="https://github.com/HardwareMonitor/openhardwaremonitor/raw/master/themes.png" alt="Themes" width="300"/>](https://github.com/HardwareMonitor/openhardwaremonitor/raw/master/themes.png)

Here's a preview of the tray icons and gadget (in Windows 10):

[<img src="https://github.com/HardwareMonitor/openhardwaremonitor/raw/master/preview_tray.png" alt="Tray icons and gadget" width="300"/>](https://github.com/HardwareMonitor/openhardwaremonitor/raw/master/preview_tray.png)

## Download

Get the latest version from [releases](https://github.com/HardwareMonitor/openhardwaremonitor/releases).

### Antivirus warnings

Antivirus software (including Microsoft Defender) may flag OpenHardwareMonitor as malware. This is a **false positive**. It happens because of how the app works: it creates a scheduled task with administrator privileges to auto-start after login, and it extracts an internal driver to a temporary folder to access hardware resources. Detections can appear a few days after a release, and a Microsoft signal usually spreads to other vendors.

> [!IMPORTANT]
> If the driver file is blocked or deleted by an antivirus, the application will not start or will not work properly. We strongly recommend adding the application folder to your antivirus exclusion list.

For Defender, run in PowerShell as administrator:
`Add-MpPreference -ExclusionPath "folder_with_app_binaries"`

> [!CAUTION]
> If your antivirus deletes the downloaded file, temporarily disable real-time protection or save the file to an excluded folder. If you are not comfortable with this or your antivirus is managed by your company, we do not recommend using OpenHardwareMonitor.

## Developer information
**Integrate the library in own application**
1. Add the [OpenHardwareMonitorLib](https://www.nuget.org/packages/OpenHardwareMonitorLib/) NuGet package to your application.
2. Use the sample code below or the test console application from [here](https://github.com/HardwareMonitor/openhardwaremonitor/tree/master/LibTest)


**Sample code**
```c#
class HardwareInfoProvider : IVisitor, IDisposable {

  private readonly Computer computer;

  public HardwareInfoProvider() {
    computer = new Computer {
      IsCpuEnabled = true,
      IsMemoryEnabled = true,
    };
    computer.Open(false);
  }

  internal float Cpu { get; private set; }
  internal float Memory { get; private set; }

  public void VisitComputer(IComputer computer) => computer.Traverse(this);
  public void VisitHardware(IHardware hardware) => hardware.Update();
  public void VisitSensor(ISensor sensor) { }
  public void VisitParameter(IParameter parameter) { }
  public void Dispose() => computer.Close();

  internal void Refresh() {
    computer.Accept(this);
    var cpuTotal = computer.Hardware.FirstOrDefault(h => h.HardwareType == HardwareType.Cpu)?.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Load && s.Name == "CPU Total");
    Cpu = cpuTotal?.Value ?? -1;

    var memorySensorName = "Physical Memory Available"; //"Virtual Memory Available";
    var memUsed = computer.Hardware.FirstOrDefault(h => h.HardwareType == HardwareType.Memory)?.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Data && s.Name == memorySensorName);
    Memory = (memUsed == null || !memUsed.Value.HasValue) ? -1 : memUsed.Value.Value * 1024; //GB -> MB
  }
}
```

**Administrator rights**

Some sensors require administrator privileges to access the data. Restart your IDE with admin privileges, or add an [app.manifest](https://learn.microsoft.com/en-us/windows/win32/sbscs/application-manifests) file to your project with requestedExecutionLevel on requireAdministrator.


## How can I help improve it?
Your feedback and contributions are welcome! Please feel free to submit Pull Requests or report issues.
<br/>
Please check if it works properly on your hardware. For many manufacturers, the way of reading data differs a bit, so if you notice any inaccuracies, please send us a pull request. If you have any suggestions or improvements, don't hesitate to create an issue.

Also, don't forget to ★ star ★ the repository to help other people find it.

<!-- [![Star History Chart](https://api.star-history.com/svg?repos=HardwareMonitor/openhardwaremonitor&type=Date)](https://star-history.com/#HardwareMonitor/openhardwaremonitor&Date) -->

<!-- [![Stargazers](https://reporoster.com/stars/HardwareMonitor/openhardwaremonitor)](https://star-history.com/#HardwareMonitor/openhardwaremonitor&Date) -->

<!-- [![Forkers](https://reporoster.com/forks/HardwareMonitor/OpenHardwareMonitor)](https://github.com/HardwareMonitor/OpenHardwareMonitor/network/members) -->

## Donate
If you find this project useful, consider [supporting the author](https://patreon.com/SergiyE).

## License

This program is free to use for **personal, home and other non-commercial purposes only**.

Any commercial use is not allowed without prior written permission from the author. This includes use inside a company or organization, bundling it with or into commercial products or services, and selling it or charging for access to it. For commercial licensing, please contact the author through [GitHub issues](https://github.com/HardwareMonitor/OpenHardwareMonitor/issues).

You may share unmodified copies of the program free of charge, as long as they are used under the same terms.

This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.

