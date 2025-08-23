using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Aga.Controls.Tree;
using Aga.Controls.Tree.NodeControls;
using OpenHardwareMonitor.Hardware;
using OpenHardwareMonitor.Hardware.Storage;
using OpenHardwareMonitor.Utilities;
using OpenHardwareMonitor.WMI;
using Logger = OpenHardwareMonitor.Utilities.Logger;

namespace OpenHardwareMonitor.UI;

public sealed partial class MainForm : Form
{
    private readonly UserOption _autoStart;
    private readonly Computer _computer;
    private readonly SensorGadget _gadget;
    private readonly Logger _logger;
    private readonly UserRadioGroup _loggingInterval;
    private readonly UserRadioGroup _updateInterval;
    private readonly UserOption _throttleAtaUpdate;
    private readonly UserOption _logSensors;
    private readonly UserOption _minimizeOnClose;
    private readonly UserOption _minimizeToTray;
    private readonly UserOption _readBatterySensors;
    private readonly UserOption _readCpuSensors;
    private readonly UserOption _readFanControllersSensors;
    private readonly UserOption _readGpuSensors;
    private readonly UserOption _readHddSensors;
    private readonly UserOption _readMainboardSensors;
    private readonly UserOption _readNicSensors;
    private readonly UserOption _readPsuSensors;
    private readonly UserOption _readRamSensors;
    private readonly Node _root;
    private readonly UserOption _runWebServer;
    private readonly UserRadioGroup _sensorValuesTimeWindow;
    private readonly PersistentSettings _settings;
    private readonly UserOption _showGadget;
    private readonly StartupManager _startupManager = new();
    private readonly SystemTray _systemTray;
    private readonly UpdateVisitor _updateVisitor = new();
    private readonly WmiProvider _wmiProvider;

    private int _delayCount;
    private bool _selectionDragging;

    public MainForm()
    {
        InitializeComponent();

        this.sensor.WidthChanged += delegate { TreeView_ColumnWidthChanged(this.sensor); };
        this.value.WidthChanged += delegate { TreeView_ColumnWidthChanged(this.value); };
        this.min.WidthChanged += delegate { TreeView_ColumnWidthChanged(this.min); };
        this.max.WidthChanged += delegate { TreeView_ColumnWidthChanged(this.max); };

        _settings = new PersistentSettings();
        _settings.Load();

        this.MinimumSize = new Size(400, 200);
        Text = Updater.ApplicationTitle;
        Icon = Icon.ExtractAssociatedIcon(Updater.CurrentFileLocation);
        portableModeMenuItem.Checked = _settings.IsPortable;
        resetOnPowerChangedMenuItem.Checked = _settings.GetValue("resetOnPowerChangedMenuItem", false);

        // make sure the buffers used for double buffering are not disposed
        // after each draw call
        BufferedGraphicsManager.Current.MaximumBuffer = Screen.PrimaryScreen.Bounds.Size;

        // set the DockStyle here, to avoid conflicts with the MainMenu
        Font = SystemFonts.MessageBoxFont;
        treeView.Font = SystemFonts.MessageBoxFont;
        treeView.KeyDown += TreeView_KeyDown;

        // Set the bounds immediately, so that our child components can be
        // properly placed.
        Bounds = new Rectangle
        {
            X = _settings.GetValue("mainForm.Location.X", Location.X),
            Y = _settings.GetValue("mainForm.Location.Y", Location.Y),
            Width = _settings.GetValue("mainForm.Width", 470),
            Height = _settings.GetValue("mainForm.Height", 640)
        };

        nodeTextBoxText.DrawText += NodeTextBoxText_DrawText;
        nodeTextBoxValue.DrawText += NodeTextBoxText_DrawText;
        nodeTextBoxMin.DrawText += NodeTextBoxText_DrawText;
        nodeTextBoxMax.DrawText += NodeTextBoxText_DrawText;
        nodeTextBoxText.EditorShowing += NodeTextBoxText_EditorShowing;

        for (int i = 1; i < treeView.Columns.Count; i++)
        {
            TreeColumn column = treeView.Columns[i];
            column.Width = Math.Max(20, Math.Min(400, _settings.GetValue("treeView.Columns." + column.Header + ".Width", column.Width)));
        }

        TreeModel treeModel = new();
        _root = new Node(Environment.MachineName) { Image = EmbeddedResources.GetImage("computer.png") };

        treeModel.Nodes.Add(_root);
        treeView.Model = treeModel;
        treeView.DrawControl += (sender, args) =>
        {
            // if (args.Node.IsSelected)
            //     return;
            if (args.Node.Tag is SensorNode sensorNode && sensorNode.PenColor.HasValue)
                args.TextColor = sensorNode.PenColor.Value;
        };

        _computer = new Computer(_settings);

        _systemTray = new SystemTray(_computer, _settings);
        _systemTray.HideShowCommand += HideShowClick;
        _systemTray.ExitCommand += ExitClick;

        if (OperatingSystemHelper.IsUnix)
        {
            // Unix
            treeView.RowHeight = Math.Max(treeView.RowHeight, 18);
            treeView.BorderStyle = BorderStyle.Fixed3D;
            gadgetMenuItem.Visible = false;
            minCloseMenuItem.Visible = false;
            minTrayMenuItem.Visible = false;
            startMinMenuItem.Visible = false;
        }
        else
        {
            // Windows
            treeView.RowHeight = Math.Max(treeView.Font.Height + 1, 18);
            _gadget = new SensorGadget(_computer, _settings);
            _gadget.HideShowCommand += HideShowClick;
            _wmiProvider = new WmiProvider(_computer);
        }

        treeView.ShowNodeToolTips = true;
        NodeToolTipProvider tooltipProvider = new();
        nodeTextBoxText.ToolTipProvider = tooltipProvider;
        nodeTextBoxValue.ToolTipProvider = tooltipProvider;
        _logger = new Logger(_computer);

        _computer.HardwareAdded += HardwareAdded;
        _computer.HardwareRemoved += HardwareRemoved;
        _computer.Open(_settings.IsPortable);

        backgroundUpdater.DoWork += BackgroundUpdater_DoWork;
        timer.Enabled = true;

        UserOption showHiddenSensors = new("hiddenMenuItem", false, hiddenMenuItem, _settings);
        showHiddenSensors.Changed += delegate { treeModel.ForceVisible = showHiddenSensors.Value; };

        UserOption showPercentageIcons = new("showPercentageIcons", false, percentageIconsMenuItem, _settings);
        showPercentageIcons.Changed += delegate { _systemTray.ShowPercentageIcons = showPercentageIcons.Value; };

        UserOption showValue = new("valueMenuItem", true, valueMenuItem, _settings);
        showValue.Changed += delegate { treeView.Columns[1].IsVisible = showValue.Value; };

        UserOption showMin = new("minMenuItem", false, minMenuItem, _settings);
        showMin.Changed += (s, e) => {
            treeView.Columns[2].IsVisible = showMin.Value;
            TreeView_SizeChanged(s, e);
        };

        UserOption showMax = new("maxMenuItem", true, maxMenuItem, _settings);
        showMax.Changed += (s, e) => {
            treeView.Columns[3].IsVisible = showMax.Value;
            TreeView_SizeChanged(s, e);
        };

        var _ = new UserOption("startMinMenuItem", false, startMinMenuItem, _settings);
        _minimizeToTray = new UserOption("minTrayMenuItem", true, minTrayMenuItem, _settings);
        _minimizeToTray.Changed += delegate { _systemTray.IsMainIconEnabled = _minimizeToTray.Value; };

        _minimizeOnClose = new UserOption("minCloseMenuItem", false, minCloseMenuItem, _settings);

        _autoStart = new UserOption(null, _startupManager.Startup, startupMenuItem, _settings);
        _autoStart.Changed += delegate
        {
            try
            {
                _startupManager.Startup = _autoStart.Value;
            }
            catch (InvalidOperationException)
            {
                MessageBox.Show("Updating the auto-startup option failed.",
                                "Error",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);

                _autoStart.Value = _startupManager.Startup;
            }
        };

        if (OperatingSystemHelper.IsAdministrator())
        {
            _readMainboardSensors = new UserOption("mainboardMenuItem", true, mainboardMenuItem, _settings);
            _readMainboardSensors.Changed += delegate { _computer.IsMotherboardEnabled = _readMainboardSensors.Value; };

            _readCpuSensors = new UserOption("cpuMenuItem", true, cpuMenuItem, _settings);
            _readCpuSensors.Changed += delegate { _computer.IsCpuEnabled = _readCpuSensors.Value; };

            _readFanControllersSensors = new UserOption("fanControllerMenuItem", false, fanControllerMenuItem, _settings);
            _readFanControllersSensors.Changed += delegate { _computer.IsControllerEnabled = _readFanControllersSensors.Value; };

            _readHddSensors = new UserOption("hddMenuItem", false, hddMenuItem, _settings);
            _readHddSensors.Changed += delegate { _computer.IsStorageEnabled = _readHddSensors.Value; };
        }
        else
        {
            mainboardMenuItem.Enabled = false;
            cpuMenuItem.Enabled = false;
            fanControllerMenuItem.Enabled = false;
            hddMenuItem.Enabled = false;
        }

        _readRamSensors = new UserOption("ramMenuItem", true, ramMenuItem, _settings);
        _readRamSensors.Changed += delegate { _computer.IsMemoryEnabled = _readRamSensors.Value; };

        _readGpuSensors = new UserOption("gpuMenuItem", false, gpuMenuItem, _settings);
        _readGpuSensors.Changed += delegate { _computer.IsGpuEnabled = _readGpuSensors.Value; };

        _readNicSensors = new UserOption("nicMenuItem", false, nicMenuItem, _settings);
        _readNicSensors.Changed += delegate { _computer.IsNetworkEnabled = _readNicSensors.Value; };

        _readPsuSensors = new UserOption("psuMenuItem", false, psuMenuItem, _settings);
        _readPsuSensors.Changed += delegate { _computer.IsPsuEnabled = _readPsuSensors.Value; };

        _readBatterySensors = new UserOption("batteryMenuItem", true, batteryMenuItem, _settings);
        _readBatterySensors.Changed += delegate { _computer.IsBatteryEnabled = _readBatterySensors.Value; };

        _showGadget = new UserOption("gadgetMenuItem", false, gadgetMenuItem, _settings);

        // Prevent Menu From Closing When UnClicking Hardware Items
        menuItemFileHardware.DropDown.Closing += StopFileHardwareMenuFromClosing;


        _showGadget.Changed += delegate
        {
            if (_gadget != null)
                _gadget.Visible = _showGadget.Value;
        };

        Server = new HttpServer(_root,
                                _settings.GetValue("listenerIp", "?"),
                                _settings.GetValue("listenerPort", 8085),
                                _settings.GetValue("authenticationEnabled", false),
                                _settings.GetValue("authenticationUserName", ""),
                                _settings.GetValue("authenticationPassword", ""));

        if (Server.PlatformNotSupported)
        {
            webMenuItemSeparator.Visible = false;
            webMenuItem.Visible = false;
        }

        _runWebServer = new UserOption("runWebServerMenuItem", false, runWebServerMenuItem, _settings);
        _runWebServer.Changed += delegate
        {
            if (_runWebServer.Value)
                Server.StartHttpListener();
            else
                Server.StopHttpListener();
        };

        openWebServerMenuItem.Click += (s, e) => {
            System.Diagnostics.Process.Start("http://localhost:" + Server.ListenerPort);
        };

        authWebServerMenuItem.Checked = _settings.GetValue("authenticationEnabled", false);

        _logSensors = new UserOption("logSensorsMenuItem", false, logSensorsMenuItem, _settings);

        _loggingInterval = new UserRadioGroup("loggingInterval",
                                              0,
                                              new[]
                                              {
                                                  log1sMenuItem,
                                                  log2sMenuItem,
                                                  log5sMenuItem,
                                                  log10sMenuItem,
                                                  log30sMenuItem,
                                                  log1minMenuItem,
                                                  log2minMenuItem,
                                                  log5minMenuItem,
                                                  log10minMenuItem,
                                                  log30minMenuItem,
                                                  log1hMenuItem,
                                                  log2hMenuItem,
                                                  log6hMenuItem
                                              },
                                              _settings);

        _loggingInterval.Changed += (sender, e) =>
        {
            switch (_loggingInterval.Value)
            {
                case 0:
                    _logger.LoggingInterval = new TimeSpan(0, 0, 1);
                    break;
                case 1:
                    _logger.LoggingInterval = new TimeSpan(0, 0, 2);
                    break;
                case 2:
                    _logger.LoggingInterval = new TimeSpan(0, 0, 5);
                    break;
                case 3:
                    _logger.LoggingInterval = new TimeSpan(0, 0, 10);
                    break;
                case 4:
                    _logger.LoggingInterval = new TimeSpan(0, 0, 30);
                    break;
                case 5:
                    _logger.LoggingInterval = new TimeSpan(0, 1, 0);
                    break;
                case 6:
                    _logger.LoggingInterval = new TimeSpan(0, 2, 0);
                    break;
                case 7:
                    _logger.LoggingInterval = new TimeSpan(0, 5, 0);
                    break;
                case 8:
                    _logger.LoggingInterval = new TimeSpan(0, 10, 0);
                    break;
                case 9:
                    _logger.LoggingInterval = new TimeSpan(0, 30, 0);
                    break;
                case 10:
                    _logger.LoggingInterval = new TimeSpan(1, 0, 0);
                    break;
                case 11:
                    _logger.LoggingInterval = new TimeSpan(2, 0, 0);
                    break;
                case 12:
                    _logger.LoggingInterval = new TimeSpan(6, 0, 0);
                    break;
            }
        };

        _updateInterval = new UserRadioGroup("updateIntervalMenuItem",
                                             2,
                                             new[]
                                             {
                                                 updateInterval250msMenuItem,
                                                 updateInterval500msMenuItem,
                                                 updateInterval1sMenuItem,
                                                 updateInterval2sMenuItem,
                                                 updateInterval5sMenuItem,
                                                 updateInterval10sMenuItem
                                             },
                                             _settings);

        _updateInterval.Changed += (sender, e) =>
        {
            switch (_updateInterval.Value)
            {
                case 0:
                    timer.Interval = 250;
                    break;
                case 1:
                    timer.Interval = 500;
                    break;
                case 2:
                    timer.Interval = 1000;
                    break;
                case 3:
                    timer.Interval = 2000;
                    break;
                case 4:
                    timer.Interval = 5000;
                    break;
                case 5:
                    timer.Interval = 10000;
                    break;
            }
        };

        _throttleAtaUpdate = new UserOption("throttleAtaUpdateMenuItem", false, throttleAtaUpdateMenuItem, _settings);
        _throttleAtaUpdate.Changed += (sender, e) =>
        {
            switch (_throttleAtaUpdate.Value)
            {
                case true:
                    AtaStorage.ThrottleInterval = TimeSpan.FromSeconds(30);
                    break;

                case false:
                    AtaStorage.ThrottleInterval = TimeSpan.Zero;
                    break;
            }
        };

        _sensorValuesTimeWindow = new UserRadioGroup("sensorValuesTimeWindow",
                                                     10,
                                                     new[]
                                                     {
                                                         timeWindow30sMenuItem,
                                                         timeWindow1minMenuItem,
                                                         timeWindow2minMenuItem,
                                                         timeWindow5minMenuItem,
                                                         timeWindow10minMenuItem,
                                                         timeWindow30minMenuItem,
                                                         timeWindow1hMenuItem,
                                                         timeWindow2hMenuItem,
                                                         timeWindow6hMenuItem,
                                                         timeWindow12hMenuItem,
                                                         timeWindow24hMenuItem
                                                     },
                                                     _settings);

        perSessionFileRotationMenuItem.Checked = _logger.FileRotationMethod == LoggerFileRotation.PerSession;
        dailyFileRotationMenuItem.Checked = _logger.FileRotationMethod == LoggerFileRotation.Daily;

        _sensorValuesTimeWindow.Changed += (sender, e) =>
        {
            TimeSpan timeWindow = TimeSpan.Zero;
            switch (_sensorValuesTimeWindow.Value)
            {
                case 0:
                    timeWindow = new TimeSpan(0, 0, 30);
                    break;
                case 1:
                    timeWindow = new TimeSpan(0, 1, 0);
                    break;
                case 2:
                    timeWindow = new TimeSpan(0, 2, 0);
                    break;
                case 3:
                    timeWindow = new TimeSpan(0, 5, 0);
                    break;
                case 4:
                    timeWindow = new TimeSpan(0, 10, 0);
                    break;
                case 5:
                    timeWindow = new TimeSpan(0, 30, 0);
                    break;
                case 6:
                    timeWindow = new TimeSpan(1, 0, 0);
                    break;
                case 7:
                    timeWindow = new TimeSpan(2, 0, 0);
                    break;
                case 8:
                    timeWindow = new TimeSpan(6, 0, 0);
                    break;
                case 9:
                    timeWindow = new TimeSpan(12, 0, 0);
                    break;
                case 10:
                    timeWindow = new TimeSpan(24, 0, 0);
                    break;
            }

            _computer.Accept(new SensorVisitor(delegate(ISensor sensor) { sensor.ValuesTimeWindow = timeWindow; }));
        };

        InitializeTheme();

        startupMenuItem.Visible = _startupManager.IsAvailable;

        if (startMinMenuItem.Checked)
        {
            if (!minTrayMenuItem.Checked)
            {
                WindowState = FormWindowState.Minimized;
                Show();
            }
        }
        else
        {
            Show();
        }

        // Create a handle, otherwise calling Close() does not fire FormClosed

        // Make sure the settings are saved when the user logs off
        Microsoft.Win32.SystemEvents.SessionEnded += delegate
        {
            _computer.Close();
            SaveConfiguration();
            if (_runWebServer.Value)
                Server.Quit();
        };

        Microsoft.Win32.SystemEvents.PowerModeChanged += PowerModeChanged;
    }

    private void StopFileHardwareMenuFromClosing(object sender, ToolStripDropDownClosingEventArgs e)
    {
        if (e.CloseReason == ToolStripDropDownCloseReason.ItemClicked)
        {
            e.Cancel = true;
        }
    }

    public bool AuthWebServerMenuItemChecked
    {
        get { return authWebServerMenuItem.Checked; }
        set { authWebServerMenuItem.Checked = value; }
    }

    public HttpServer Server { get; }

    private void BackgroundUpdater_DoWork(object sender, DoWorkEventArgs e)
    {
        _computer.Accept(_updateVisitor);

        if (_logSensors != null && _logSensors.Value && _delayCount >= 4)
            _logger.Log();

        if (_delayCount < 4)
            _delayCount++;
    }

    private void PowerModeChanged(object sender, Microsoft.Win32.PowerModeChangedEventArgs eventArgs)
    {
        if (eventArgs.Mode == Microsoft.Win32.PowerModes.Resume || resetOnPowerChangedMenuItem.Checked)
        {
            _computer.Reset();
        }
    }

    private void ResetOnPowerChangedMenuItem_Click(object sender, EventArgs eventArgs)
    {
        resetOnPowerChangedMenuItem.Checked = !resetOnPowerChangedMenuItem.Checked;
        _settings.SetValue("resetOnPowerChangedMenuItem", resetOnPowerChangedMenuItem.Checked);
    }

    private void InitializeTheme()
    {
        mainMenu.Renderer = new ThemedToolStripRenderer();
        treeContextMenu.Renderer = new ThemedToolStripRenderer();
        ThemedVScrollIndicator.AddToControl(treeView);
        ThemedHScrollIndicator.AddToControl(treeView);
        TreeViewAdvThemeExtender.SubscribeToThemes();

        //themeMenuItem.MenuItems.Clear();
        var currentItem = CustomTheme.FillThemesMenu((title, theme, onClick) => {
            if (theme == null && onClick == null)
            {
                themeMenuItem.DropDownItems.Add(title);
                return null;
            }
            var item = new ToolStripRadioButtonMenuItem(title, null, onClick);
            themeMenuItem.DropDownItems.Add(item);
            return item;
        }, () => {
            _settings.SetValue("theme", Theme.IsAutoThemeEnabled ? "auto" : Theme.Current.Id);
        }, _settings.GetValue("theme", "auto"), "OpenHardwareMonitor.Resources.themes");
        currentItem?.PerformClick();
        Theme.Current.Apply(this);
    }

    private void InsertSorted(IList<Node> nodes, HardwareNode node)
    {
        int i = 0;
        while (i < nodes.Count && nodes[i] is HardwareNode && ((HardwareNode)nodes[i]).Hardware.HardwareType <= node.Hardware.HardwareType)
            i++;

        nodes.Insert(i, node);
    }

    private void SubHardwareAdded(IHardware hardware, Node node)
    {
        HardwareNode hardwareNode = new(hardware, _settings);
        InsertSorted(node.Nodes, hardwareNode);
        foreach (IHardware subHardware in hardware.SubHardware)
            SubHardwareAdded(subHardware, hardwareNode);
    }

    private void HardwareAdded(IHardware hardware)
    {
        SubHardwareAdded(hardware, _root);
    }

    private void HardwareRemoved(IHardware hardware)
    {
        List<HardwareNode> nodesToRemove = new();
        foreach (Node node in _root.Nodes)
        {
            if (node is HardwareNode hardwareNode && hardwareNode.Hardware == hardware)
                nodesToRemove.Add(hardwareNode);
        }

        foreach (HardwareNode hardwareNode in nodesToRemove)
        {
            _root.Nodes.Remove(hardwareNode);
        }
    }

    private void NodeTextBoxText_DrawText(object sender, DrawEventArgs e)
    {
        if (e.Node.Tag is Node node)
        {
            if (node.IsVisible)
            {
                //e.TextColor = color;
            }
            else
                e.TextColor = Color.DarkGray;
        }
    }

    private void NodeTextBoxText_EditorShowing(object sender, CancelEventArgs e)
    {
        e.Cancel = !(treeView.CurrentNode != null && (treeView.CurrentNode.Tag is SensorNode || treeView.CurrentNode.Tag is HardwareNode));
    }

    private void ExitClick(object sender, EventArgs e)
    {
        CloseApplication();
    }

    private void Timer_Tick(object sender, EventArgs e)
    {
        treeView.Invalidate();
        _systemTray.Redraw();
        _gadget?.Redraw();
        _wmiProvider?.Update();

        if (!backgroundUpdater.IsBusy)
            backgroundUpdater.RunWorkerAsync();

        RestoreCollapsedNodeState(treeView);
    }

    private void SaveConfiguration()
    {
        if (_settings == null)
            return;

        foreach (TreeColumn column in treeView.Columns)
            _settings.SetValue("treeView.Columns." + column.Header + ".Width", column.Width);

        _settings.SetValue("listenerIp", Server.ListenerIp);
        _settings.SetValue("listenerPort", Server.ListenerPort);
        _settings.SetValue("authenticationEnabled", Server.AuthEnabled);
        _settings.SetValue("authenticationUserName", Server.UserName);
        _settings.SetValue("authenticationPassword", Server.Password);

        _settings.Save();
    }

    private void MainForm_Load(object sender, EventArgs e)
    {
        Rectangle newBounds = new()
        {
            X = _settings.GetValue("mainForm.Location.X", Location.X),
            Y = _settings.GetValue("mainForm.Location.Y", Location.Y),
            Width = _settings.GetValue("mainForm.Width", 700),
            Height = _settings.GetValue("mainForm.Height", 640)
        };

        Rectangle fullWorkingArea = new(int.MaxValue, int.MaxValue, int.MinValue, int.MinValue);

        foreach (Screen screen in Screen.AllScreens)
            fullWorkingArea = Rectangle.Union(fullWorkingArea, screen.Bounds);

        Rectangle intersection = Rectangle.Intersect(fullWorkingArea, newBounds);
        if (intersection.Width < 20 || intersection.Height < 20 || !_settings.Contains("mainForm.Location.X"))
        {
            newBounds.X = (Screen.PrimaryScreen.WorkingArea.Width / 2) - (newBounds.Width / 2);
            newBounds.Y = (Screen.PrimaryScreen.WorkingArea.Height / 2) - (newBounds.Height / 2);
        }

        Bounds = newBounds;

        RestoreCollapsedNodeState(treeView);
        treeView.Width += 1; //just to apply column auto-resize

        Updater.Subscribe(
          (message, isError) => {
              MessageBox.Show(message, Updater.ApplicationName, MessageBoxButtons.OK, isError ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
          },
          (message) => {
              return MessageBox.Show(message, Updater.ApplicationName, MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK;
          },
          Application.Exit
        );

        //will display prompt only if update available & when main form displayed
        var timer = new Timer();
        timer.Interval = 3000;
        timer.Tick += async (_, _) => {
            timer.Enabled = false;
            timer.Enabled = ! await Updater.CheckForUpdatesAsync(true);
        };
        timer.Enabled = true;

        FormClosed += MainForm_FormClosed;
    }

    private void RestoreCollapsedNodeState(TreeViewAdv treeViewAdv)
    {
        var collapsedHwNodes = treeViewAdv.AllNodes
                                          .Where(n => n.IsExpanded && n.Tag is IExpandPersistNode expandPersistNode && !expandPersistNode.Expanded)
                                          .OrderByDescending(n => n.Level)
                                          .ToList();

        foreach (TreeNodeAdv node in collapsedHwNodes)
        {
            node.Collapse(false);
        }
    }

    private void CloseApplication()
    {
        FormClosed -= MainForm_FormClosed;

        Visible = false;
        _systemTray.IsMainIconEnabled = false;
        timer.Enabled = false;
        _computer.Close();
        SaveConfiguration();
        if (_runWebServer.Value)
            Server.Quit();

        _systemTray.Dispose();
        timer.Dispose();
        backgroundUpdater.Dispose();

        Application.Exit();
    }

    private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
    {
        CloseApplication();
    }

    private void menuItemSite_Click(object sender, EventArgs e)
    {
        Updater.VisitAppSite();
    }

    private void menuItemCheckUpdates_Click(object sender, EventArgs e)
    {
        Updater.CheckForUpdates(false);
    }

    private void AboutMenuItem_Click(object sender, EventArgs e)
    {
        _ = new AboutBox().ShowDialog();
    }

    private void TreeView_CollapsedOrExpanded(object sender, TreeViewAdvEventArgs info)
    {
        if (info.RaisedByUser && info.Node.Tag is IExpandPersistNode expandPersistNode)
            expandPersistNode.Expanded = info.Node.IsExpanded;
    }

    private void TreeView_KeyDown(object sender, KeyEventArgs e)
    {
        var node = treeView.SelectedNode;
        if (node is not {Tag: SensorNode sensorNode} || sensorNode.Sensor == null)
            return;
        switch (e.KeyCode) {
            case Keys.H:
                sensorNode.IsVisible = !sensorNode.IsVisible;
                treeView.SelectedNode = node.NextNode;
                break;
            case Keys.P:
                ShowParameterForm(sensorNode.Sensor);
                break;
            case Keys.R:
                sensorNode.PenColor = null;
                treeView.SelectedNode = node.NextNode;
                break;
        }
    }

    private void TreeView_Click(object sender, EventArgs e)
    {
        if (e is not MouseEventArgs m || (m.Button != MouseButtons.Left && m.Button != MouseButtons.Right))
            return;

        NodeControlInfo info = treeView.GetNodeControlInfoAt(new Point(m.X, m.Y));
        treeView.SelectedNode = info.Node;
        if (m.Button != MouseButtons.Right || info.Node == null)
            return;

        ToolStripMenuItem item;
        switch (info.Node.Tag) {
            case SensorNode node when node.Sensor != null: {
                treeContextMenu.Items.Clear();
                if (node.Sensor.Parameters.Count > 0)
                {
                    item = new ToolStripMenuItem("Parameters... (P)");
                    item.Click += delegate { ShowParameterForm(node.Sensor); };
                    treeContextMenu.Items.Add(item);
                }

                if (nodeTextBoxText.EditEnabled)
                {
                    item = new ToolStripMenuItem("Rename (F2)");
                    item.Click += delegate { nodeTextBoxText.BeginEdit(); };
                    treeContextMenu.Items.Add(item);
                }

                if (node.IsVisible)
                {
                    item = new ToolStripMenuItem("Hide (H)");
                    item.Click += delegate { node.IsVisible = false; };
                    treeContextMenu.Items.Add(item);
                }
                else
                {
                    item = new ToolStripMenuItem("Unhide (H)");
                    item.Click += delegate { node.IsVisible = true; };
                    treeContextMenu.Items.Add(item);
                }

                treeContextMenu.Items.Add(new ToolStripSeparator());
                item = new ToolStripMenuItem("Pen Color...");
                item.Click += delegate
                {
                    using (var dialog = new ColorDialog())
                    {
                        dialog.Color = node.PenColor.GetValueOrDefault();
                        if (dialog.ShowDialog() == DialogResult.OK)
                            node.PenColor = dialog.Color;
                    }
                };
                treeContextMenu.Items.Add(item);

                item = new ToolStripMenuItem("Reset Pen Color (R)");
                item.Click += delegate { node.PenColor = null; };
                treeContextMenu.Items.Add(item);

                treeContextMenu.Items.Add(new ToolStripSeparator());
                item = new ToolStripMenuItem("Show in Tray") { Checked = _systemTray.Contains(node.Sensor) };
                item.Click += (s, _) =>
                {
                    if (s is not ToolStripMenuItem menuItem)
                        return;
                    if (menuItem.Checked)
                        _systemTray.Remove(node.Sensor);
                    else
                        _systemTray.Add(node.Sensor, true);
                };
                treeContextMenu.Items.Add(item);

                if (_gadget != null)
                {
                    item = new ToolStripMenuItem("Show in Gadget") { Checked = _gadget.Contains(node.Sensor) };
                    item.Click += (s, _) =>
                    {
                        if (s is not ToolStripMenuItem menuItem)
                            return;
                        if (menuItem.Checked)
                            _gadget.Remove(node.Sensor);
                        else
                            _gadget.Add(node.Sensor);
                    };
                    treeContextMenu.Items.Add(item);
                }

                if (node.Sensor.Control != null)
                {
                    treeContextMenu.Items.Add(new ToolStripSeparator());
                    IControl control = node.Sensor.Control;
                    ToolStripMenuItem controlItem = new("Control");
                    ToolStripItem defaultItem = new ToolStripMenuItem("Default") { Checked = control.ControlMode == ControlMode.Default };
                    controlItem.DropDownItems.Add(defaultItem);
                    defaultItem.Click += delegate { control.SetDefault(); };
                    ToolStripMenuItem manualItem = new("Manual");
                    controlItem.DropDownItems.Add(manualItem);
                    manualItem.Checked = control.ControlMode == ControlMode.Software;
                    for (int i = 0; i <= 100; i += 5)
                    {
                        if (!(i <= control.MaxSoftwareValue) || !(i >= control.MinSoftwareValue))
                            continue;
                        item = new ToolStripRadioButtonMenuItem(i + " %");
                        manualItem.DropDownItems.Add(item);
                        item.Checked = control.ControlMode == ControlMode.Software && Math.Round(control.SoftwareValue) == i;
                        int softwareValue = i;
                        item.Click += delegate { control.SetSoftware(softwareValue); };
                    }
                    treeContextMenu.Items.Add(controlItem);
                }
                treeContextMenu.Show(treeView, new Point(m.X, m.Y));
                break;
            }
            case HardwareNode hardwareNode when hardwareNode.Hardware != null: {
                treeContextMenu.Items.Clear();
                if (nodeTextBoxText.EditEnabled)
                {
                    item = new ToolStripMenuItem("Rename (F2)");
                    item.Click += delegate { nodeTextBoxText.BeginEdit(); };
                    treeContextMenu.Items.Add(item);
                }
                treeContextMenu.Show(treeView, new Point(m.X, m.Y));
                break;
            }
        }
    }

    private void SaveReportMenuItem_Click(object sender, EventArgs e)
    {
        string report = _computer.GetReport();
        if (saveFileDialog.ShowDialog() == DialogResult.OK)
        {
            using (TextWriter w = new StreamWriter(saveFileDialog.FileName))
            {
                w.Write(report);
            }
        }
    }

    private void SysTrayHideShow()
    {
        Visible = !Visible;
        if (Visible)
            Activate();
    }

    protected override void WndProc(ref Message m)
    {
        if (_minimizeToTray.Value && m.Msg == WinApiHelper.WM_SYS_COMMAND && m.WParam.ToInt64() == WinApiHelper.SC_MINIMIZE)
        {
            SysTrayHideShow();
        }
        //else if (m.Msg == WinApiHelper.WM_WININICHANGE && Marshal.PtrToStringUni(m.LParam) == "ImmersiveColorSet" && Theme.IsAutoThemeEnabled)
        //{
        //    Theme.SetAutoTheme();
        //}
        else if (_minimizeOnClose.Value && m.Msg == WinApiHelper.WM_SYS_COMMAND && m.WParam.ToInt64() == WinApiHelper.SC_CLOSE)
        {
            //Apparently the user wants to minimize rather than close
            //Now we still need to check if we're going to the tray or not
            //Note: the correct way to do this would be to send out SC_MINIMIZE,
            //but since the code here is so simple,
            //that would just be a waste of time.
            if (_minimizeToTray.Value)
                SysTrayHideShow();
            else
                WindowState = FormWindowState.Minimized;
        }
        else if (m.Msg == WinApiHelper.WM_SHOWME)
        {
            Visible = true;
            WindowState = FormWindowState.Normal;
            Activate();
            BringToFront();
            WinApiHelper.SetForegroundWindow(this.Handle);
        }
        else
        {
            base.WndProc(ref m);
        }
    }

    private void HideShowClick(object sender, EventArgs e)
    {
        SysTrayHideShow();
    }

    private void ShowParameterForm(ISensor sensorForm)
    {
        ParameterForm form = new() { Parameters = sensorForm.Parameters, captionLabel = { Text = sensorForm.Name } };
        form.ShowDialog();
    }

    private void TreeView_NodeMouseDoubleClick(object sender, TreeNodeAdvMouseEventArgs e)
    {
        if (e.Node.Tag is SensorNode node && node.Sensor != null && node.Sensor.Parameters.Count > 0)
            ShowParameterForm(node.Sensor);
    }

    private void ResetMinMaxMenuItem_Click(object sender, EventArgs e)
    {
        _computer.Accept(new SensorVisitor(delegate(ISensor sensorClick)
        {
            sensorClick.ResetMin();
            sensorClick.ResetMax();
        }));
    }

    private void ExpandAllNodes_Click(object sender, EventArgs e)
    {
        treeView.ExpandAll();
    }

    private void CollapsepAllNodes_Click(object sender, EventArgs e)
    {
        treeView.CollapseAll();
    }

    private void MainForm_MoveOrResize(object sender, EventArgs e)
    {
        if (WindowState != FormWindowState.Minimized)
        {
            _settings.SetValue("mainForm.Location.X", Bounds.X);
            _settings.SetValue("mainForm.Location.Y", Bounds.Y);
            _settings.SetValue("mainForm.Width", Bounds.Width);
            _settings.SetValue("mainForm.Height", Bounds.Height);
        }
    }

    private void ResetClick(object sender, EventArgs e)
    {
        // disable the fallback MainIcon during reset, otherwise icon visibility
        // might be lost
        _systemTray.IsMainIconEnabled = false;
        _computer.Reset();
        // restore the MainIcon setting
        _systemTray.IsMainIconEnabled = _minimizeToTray.Value;
    }

    private void TreeView_MouseMove(object sender, MouseEventArgs e)
    {
        _selectionDragging &= (e.Button & (MouseButtons.Left | MouseButtons.Right)) > 0;
        if (_selectionDragging)
            treeView.SelectedNode = treeView.GetNodeAt(e.Location);
    }

    private void TreeView_MouseDown(object sender, MouseEventArgs e)
    {
        _selectionDragging = true;
    }

    private void TreeView_MouseUp(object sender, MouseEventArgs e)
    {
        _selectionDragging = false;
    }

    private void TreeView_SizeChanged(object sender, EventArgs e)
    {
        int newWidth = treeView.Width;
        for (int i = 1; i < treeView.Columns.Count; i++)
        {
            if (treeView.Columns[i].IsVisible)
                newWidth -= treeView.Columns[i].Width;
        }
        treeView.Columns[0].Width = newWidth;
    }

    private void TreeView_ColumnWidthChanged(TreeColumn column)
    {
        int index = treeView.Columns.IndexOf(column);
        int columnsWidth = 0;
        foreach (TreeColumn treeColumn in treeView.Columns)
        {
            if (treeColumn.IsVisible)
                columnsWidth += treeColumn.Width;
        }

        int nextColumnIndex = index + 1;
        while (nextColumnIndex < treeView.Columns.Count && treeView.Columns[nextColumnIndex].IsVisible == false)
            nextColumnIndex++;

        if (nextColumnIndex < treeView.Columns.Count) {
            int diff = treeView.Width - columnsWidth;
            treeView.Columns[nextColumnIndex].Width = Math.Max(20, treeView.Columns[nextColumnIndex].Width + diff);
        }
    }

    private void ServerInterfacePortMenuItem_Click(object sender, EventArgs e)
    {
        new InterfacePortForm(this).ShowDialog();
    }

    private void AuthWebServerMenuItem_Click(object sender, EventArgs e)
    {
        new AuthForm(this).ShowDialog();
    }

    private void perSessionFileRotationMenuItem_Click(object sender, EventArgs e)
    {
        dailyFileRotationMenuItem.Checked = false;
        perSessionFileRotationMenuItem.Checked = true;
        _logger.FileRotationMethod = LoggerFileRotation.PerSession;
    }

    private void dailyFileRotationMenuItem_Click(object sender, EventArgs e)
    {
        dailyFileRotationMenuItem.Checked = true;
        perSessionFileRotationMenuItem.Checked = false;
        _logger.FileRotationMethod = LoggerFileRotation.Daily;
    }

    private void PortableModeMenu_Click(object sender, EventArgs e)
    {
        //var dlg = new SaveFileDialog
        //{
        //    DefaultExt = ".config",
        //    FileName = "OpenHardwareMonitor.config",
        //    Filter = "Config files|*.config",
        //    RestoreDirectory = false,
        //    Title = "Export Settings As",
        //    InitialDirectory = Path.GetDirectoryName(Application.ExecutablePath),
        //};
        //if (dlg.ShowDialog() != DialogResult.OK)
        //    return;
        //var oldPortableValue = _settings.IsPortable;
        _settings.IsPortable = !_settings.IsPortable;
        portableModeMenuItem.Checked = _settings.IsPortable;
        _settings.Save();
        Updater.RestartApp();
        //if (!oldPortableValue)
        //    _settings.IsPortable = oldPortableValue;
        //MessageBox.Show("Settings export completed successfully!", "Export Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
}
