# Open hardware monitor
[![Release](https://img.shields.io/github/v/release/HardwareMonitor/openhardwaremonitor)](https://github.com/HardwareMonitor/openhardwaremonitor/releases/latest)
[![Downloads](https://img.shields.io/github/downloads/HardwareMonitor/openhardwaremonitor/total?color=ff4f42)](https://github.com/HardwareMonitor/openhardwaremonitor/releases)
[![Last commit](https://img.shields.io/github/last-commit/HardwareMonitor/openhardwaremonitor?color=00AD00)](https://github.com/HardwareMonitor/openhardwaremonitor/commits/master)
[![](https://img.shields.io/badge/WINDOWS-7%20%E2%80%93%2011-blue)](https://endoflife.date/windows)
[![](https://img.shields.io/badge/SERVER-2012%20%E2%80%93%202025-blue)](https://endoflife.date/windows-server)

[![Nuget](https://img.shields.io/nuget/v/OpenHardwareMonitorLib)](https://www.nuget.org/packages/OpenHardwareMonitorLib/)
[![Nuget](https://img.shields.io/nuget/dt/OpenHardwareMonitorLib?label=nuget-downloads)](https://www.nuget.org/packages/OpenHardwareMonitorLib/)

Open hardware monitor is free software that can monitor the temperature sensors, fan speeds, voltages, load and clock speeds of your computer.

This application is based on the "original" [openhardwaremonitor](https://github.com/openhardwaremonitor/openhardwaremonitor) project.

## Features

### What can it do?

You can see information about devices such as:
 - Motherboards
 - Intel and AMD processors
 - RAM
 - NVIDIA, AMD and Intel graphics cards
 - HDD, SSD and NVMe hard drives
 - Network cards
 - Power suppliers
 - Laptop batteries

### Additional features

 - `Remote web-server` mode for browsing data from remote machine with custom port and authentication.
 - `Hide/Unhide` sensors to remove some data from UI and web server.
 - Multiple `Tray icons` and `Gadget` for selected sensor values.
 - `Light`/`Dark` themes with auto switching mode.
 - Custom `color-themes` from external files.
 - `Portable` mode for storing temporary driver file and settings configuration next to the executable file.
 - `Updated versions check` - manually from main menu.

 Note: Some sensors are only available when running the application as administrator.

To add custom theme to the app, just create a `themes` folder next to the executable file and place any {themeName}.json files there.
Custom theme.json file content example:
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
Don't forget to restart the app to scan for new theme files!


### What does it look like?

Here's a preview of the app's UI with `Light`/`Dark` themes running on Windows 10:

[<img src="https://github.com/HardwareMonitor/openhardwaremonitor/raw/master/themes.png" alt="Themes" width="300"/>](https://github.com/HardwareMonitor/openhardwaremonitor/raw/master/themes.png)

Here's a preview of the tray icons and gadget (in Windows 10):

[<img src="https://github.com/HardwareMonitor/openhardwaremonitor/raw/master/preview_tray.png" alt="Themes" width="300"/>](https://github.com/HardwareMonitor/openhardwaremonitor/raw/master/preview_tray.png)

## Download

The published version can be obtained from [releases](https://github.com/HardwareMonitor/openhardwaremonitor/releases).

> [!WARNING]
>Microsoft and other major antivirus vendors may have flagged OpenHardwareMonitor as malware. This is a false positive and is not related to a virus or anything similar. Signals from Microsoft usually extend to other antivirus vendors as well.
>OpenHardwareMonitor has a history of being falsely flagged as malware by antivirus vendors (including Defender). This is likely due to its behavior, such as creating a task with administrator privileges to auto-start the application after login, or storing an internal driver in a temporary folder to gain access to hardware resources.

Currently, Defender does not flag this release, but it is likely that future updates may be flagged by Defender's machine learning-based detection systems within a few days of release.

> [!IMPORTANT]
>If Defender or another antivirus detects any part of OpenHardwareMonitor as malware, it may prevent proper work or cause application to fail to start.
> OpenHardwareMonitor will not start if this file exists but is blocked from being loaded.
>We strongly recommend excluding OpenHardwareMonitor's binaries from antivirus scans

> [!TIP]
> You can include the app folder in your antivirus' exclusion list to prevent issues due to antivirus detections

For Defender, you can run the following script in PowerShell as an administrator:
`Add-MpPreference -ExclusionPath "folder_with_app_binaries"`


> [!CAUTION]
> If your antivirus deletes the downloaded app file, you may need to temporarily disable real-time protection or save the file in an excluded folder.
> If you are not comfortable with this process or your antivirus is managed by your company, we do not recommend using OpenHardwareMonitor. Please consider alternative solutions instead.

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
The OpenHardwareMonitor team welcomes feedback and contributions!<br/>
You can check if it works properly on your motherboard. For many manufacturers, the way of reading data differs a bit, so if you notice any inaccuracies, please send us a pull request. If you have any suggestions or improvements, don't hesitate to create an issue.

Also, don't forget to ★ star ★ the repository to help other people find it.

<!-- [![Star History Chart](https://api.star-history.com/svg?repos=HardwareMonitor/openhardwaremonitor&type=Date)](https://star-history.com/#HardwareMonitor/openhardwaremonitor&Date) -->

[![Stargazers](https://reporoster.com/stars/HardwareMonitor/openhardwaremonitor)](https://star-history.com/#HardwareMonitor/openhardwaremonitor&Date)

[![Forkers](https://reporoster.com/forks/HardwareMonitor/OpenHardwareMonitor)](https://github.com/HardwareMonitor/OpenHardwareMonitor/network/members)

## Donate!
Every [cup of coffee](https://patreon.com/SergiyE) you donate will help this app become better and let me know that this project is in demand.

## License

This program is free software: you can redistribute it.

This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.

