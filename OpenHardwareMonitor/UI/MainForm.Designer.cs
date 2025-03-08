namespace OpenHardwareMonitor.UI
{
    sealed partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
      this.components = new System.ComponentModel.Container();
      this.sensor = new Aga.Controls.Tree.TreeColumn();
      this.value = new Aga.Controls.Tree.TreeColumn();
      this.min = new Aga.Controls.Tree.TreeColumn();
      this.max = new Aga.Controls.Tree.TreeColumn();
      this.nodeImage = new Aga.Controls.Tree.NodeControls.NodeIcon();
      this.nodeTextBoxText = new Aga.Controls.Tree.NodeControls.NodeTextBox();
      this.nodeTextBoxValue = new Aga.Controls.Tree.NodeControls.NodeTextBox();
      this.nodeTextBoxMin = new Aga.Controls.Tree.NodeControls.NodeTextBox();
      this.nodeTextBoxMax = new Aga.Controls.Tree.NodeControls.NodeTextBox();
      this.mainMenu = new System.Windows.Forms.MenuStrip();
      this.fileMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.saveReportMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.portableModeMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.resetOnPowerChangedMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.MenuItem2 = new System.Windows.Forms.ToolStripSeparator();
      this.resetMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.menuItemFileHardware = new System.Windows.Forms.ToolStripMenuItem();
      this.mainboardMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.cpuMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.ramMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.gpuMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.fanControllerMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.hddMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.nicMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.psuMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.batteryMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.menuItem6 = new System.Windows.Forms.ToolStripSeparator();
      this.exitMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.viewMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.resetMinMaxMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.MenuItem3 = new System.Windows.Forms.ToolStripSeparator();
      this.hiddenMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.gadgetMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.MenuItem1 = new System.Windows.Forms.ToolStripSeparator();
      this.columnsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.valueMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.minMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.maxMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.optionsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.startMinMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.minTrayMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.minCloseMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.startupMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.separatorMenuItem = new System.Windows.Forms.ToolStripSeparator();
      this.temperatureUnitsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.celsiusMenuItem = new OpenHardwareMonitor.UI.ToolStripRadioButtonMenuItem();
      this.fahrenheitMenuItem = new OpenHardwareMonitor.UI.ToolStripRadioButtonMenuItem();
      this.themeMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.logSeparatorMenuItem = new System.Windows.Forms.ToolStripSeparator();
      this.logSensorsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.loggingIntervalMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.log1sMenuItem = new OpenHardwareMonitor.UI.ToolStripRadioButtonMenuItem();
      this.log2sMenuItem = new OpenHardwareMonitor.UI.ToolStripRadioButtonMenuItem();
      this.log5sMenuItem = new OpenHardwareMonitor.UI.ToolStripRadioButtonMenuItem();
      this.log10sMenuItem = new OpenHardwareMonitor.UI.ToolStripRadioButtonMenuItem();
      this.log30sMenuItem = new OpenHardwareMonitor.UI.ToolStripRadioButtonMenuItem();
      this.log1minMenuItem = new OpenHardwareMonitor.UI.ToolStripRadioButtonMenuItem();
      this.log2minMenuItem = new OpenHardwareMonitor.UI.ToolStripRadioButtonMenuItem();
      this.log5minMenuItem = new OpenHardwareMonitor.UI.ToolStripRadioButtonMenuItem();
      this.log10minMenuItem = new OpenHardwareMonitor.UI.ToolStripRadioButtonMenuItem();
      this.log30minMenuItem = new OpenHardwareMonitor.UI.ToolStripRadioButtonMenuItem();
      this.log1hMenuItem = new OpenHardwareMonitor.UI.ToolStripRadioButtonMenuItem();
      this.log2hMenuItem = new OpenHardwareMonitor.UI.ToolStripRadioButtonMenuItem();
      this.log6hMenuItem = new OpenHardwareMonitor.UI.ToolStripRadioButtonMenuItem();
      this.updateIntervalMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.updateInterval250msMenuItem = new OpenHardwareMonitor.UI.ToolStripRadioButtonMenuItem();
      this.updateInterval500msMenuItem = new OpenHardwareMonitor.UI.ToolStripRadioButtonMenuItem();
      this.updateInterval1sMenuItem = new OpenHardwareMonitor.UI.ToolStripRadioButtonMenuItem();
      this.updateInterval2sMenuItem = new OpenHardwareMonitor.UI.ToolStripRadioButtonMenuItem();
      this.updateInterval5sMenuItem = new OpenHardwareMonitor.UI.ToolStripRadioButtonMenuItem();
      this.updateInterval10sMenuItem = new OpenHardwareMonitor.UI.ToolStripRadioButtonMenuItem();
      this.fileRotationMethod = new System.Windows.Forms.ToolStripMenuItem();
      this.perSessionFileRotationMenuItem = new OpenHardwareMonitor.UI.ToolStripRadioButtonMenuItem();
      this.dailyFileRotationMenuItem = new OpenHardwareMonitor.UI.ToolStripRadioButtonMenuItem();
      this.sensorValuesTimeWindowMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.timeWindow30sMenuItem = new OpenHardwareMonitor.UI.ToolStripRadioButtonMenuItem();
      this.timeWindow1minMenuItem = new OpenHardwareMonitor.UI.ToolStripRadioButtonMenuItem();
      this.timeWindow2minMenuItem = new OpenHardwareMonitor.UI.ToolStripRadioButtonMenuItem();
      this.timeWindow5minMenuItem = new OpenHardwareMonitor.UI.ToolStripRadioButtonMenuItem();
      this.timeWindow10minMenuItem = new OpenHardwareMonitor.UI.ToolStripRadioButtonMenuItem();
      this.timeWindow30minMenuItem = new OpenHardwareMonitor.UI.ToolStripRadioButtonMenuItem();
      this.timeWindow1hMenuItem = new OpenHardwareMonitor.UI.ToolStripRadioButtonMenuItem();
      this.timeWindow2hMenuItem = new OpenHardwareMonitor.UI.ToolStripRadioButtonMenuItem();
      this.timeWindow6hMenuItem = new OpenHardwareMonitor.UI.ToolStripRadioButtonMenuItem();
      this.timeWindow12hMenuItem = new OpenHardwareMonitor.UI.ToolStripRadioButtonMenuItem();
      this.timeWindow24hMenuItem = new OpenHardwareMonitor.UI.ToolStripRadioButtonMenuItem();
      this.webMenuItemSeparator = new System.Windows.Forms.ToolStripSeparator();
      this.webMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.runWebServerMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.openWebServerMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.serverPortMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.authWebServerMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.helpMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.menuItemCheckUpdates = new System.Windows.Forms.ToolStripMenuItem();
      this.aboutMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.treeContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
      this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
      this.timer = new System.Windows.Forms.Timer(this.components);
      this.treeView = new Aga.Controls.Tree.TreeViewAdv();
      this.backgroundUpdater = new System.ComponentModel.BackgroundWorker();
      this.mainMenu.SuspendLayout();
      this.SuspendLayout();
      //
      // sensor
      //
      this.sensor.Header = "Sensor";
      this.sensor.SortOrder = System.Windows.Forms.SortOrder.None;
      this.sensor.TooltipText = null;
      this.sensor.Width = 250;
      //
      // value
      //
      this.value.Header = "Value";
      this.value.SortOrder = System.Windows.Forms.SortOrder.None;
      this.value.TooltipText = null;
      this.value.Width = 100;
      //
      // min
      //
      this.min.Header = "Min";
      this.min.SortOrder = System.Windows.Forms.SortOrder.None;
      this.min.TooltipText = null;
      this.min.Width = 100;
      //
      // max
      //
      this.max.Header = "Max";
      this.max.SortOrder = System.Windows.Forms.SortOrder.None;
      this.max.TooltipText = null;
      this.max.Width = 100;
      //
      // nodeImage
      //
      this.nodeImage.DataPropertyName = "Image";
      this.nodeImage.LeftMargin = 1;
      this.nodeImage.ParentColumn = this.sensor;
      this.nodeImage.ScaleMode = Aga.Controls.Tree.ImageScaleMode.Fit;
      //
      // nodeTextBoxText
      //
      this.nodeTextBoxText.DataPropertyName = "Text";
      this.nodeTextBoxText.EditEnabled = true;
      this.nodeTextBoxText.IncrementalSearchEnabled = true;
      this.nodeTextBoxText.LeftMargin = 3;
      this.nodeTextBoxText.ParentColumn = this.sensor;
      this.nodeTextBoxText.Trimming = System.Drawing.StringTrimming.EllipsisCharacter;
      this.nodeTextBoxText.UseCompatibleTextRendering = true;
      //
      // nodeTextBoxValue
      //
      this.nodeTextBoxValue.DataPropertyName = "Value";
      this.nodeTextBoxValue.IncrementalSearchEnabled = true;
      this.nodeTextBoxValue.LeftMargin = 3;
      this.nodeTextBoxValue.ParentColumn = this.value;
      this.nodeTextBoxValue.Trimming = System.Drawing.StringTrimming.EllipsisCharacter;
      this.nodeTextBoxValue.UseCompatibleTextRendering = true;
      //
      // nodeTextBoxMin
      //
      this.nodeTextBoxMin.DataPropertyName = "Min";
      this.nodeTextBoxMin.IncrementalSearchEnabled = true;
      this.nodeTextBoxMin.LeftMargin = 3;
      this.nodeTextBoxMin.ParentColumn = this.min;
      this.nodeTextBoxMin.Trimming = System.Drawing.StringTrimming.EllipsisCharacter;
      this.nodeTextBoxMin.UseCompatibleTextRendering = true;
      //
      // nodeTextBoxMax
      //
      this.nodeTextBoxMax.DataPropertyName = "Max";
      this.nodeTextBoxMax.IncrementalSearchEnabled = true;
      this.nodeTextBoxMax.LeftMargin = 3;
      this.nodeTextBoxMax.ParentColumn = this.max;
      this.nodeTextBoxMax.Trimming = System.Drawing.StringTrimming.EllipsisCharacter;
      this.nodeTextBoxMax.UseCompatibleTextRendering = true;
      //
      // mainMenu
      //
      this.mainMenu.ImageScalingSize = new System.Drawing.Size(24, 24);
      this.mainMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileMenuItem,
            this.viewMenuItem,
            this.optionsMenuItem,
            this.helpMenuItem});
      this.mainMenu.Location = new System.Drawing.Point(0, 0);
      this.mainMenu.Name = "mainMenu";
      this.mainMenu.Padding = new System.Windows.Forms.Padding(4, 1, 0, 1);
      this.mainMenu.Size = new System.Drawing.Size(418, 24);
      this.mainMenu.TabIndex = 1;
      //
      // fileMenuItem
      //
      this.fileMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.saveReportMenuItem,
            this.portableModeMenuItem,
            this.MenuItem2,
            this.resetMenuItem,
            this.resetOnPowerChangedMenuItem,
            this.menuItemFileHardware,
            this.menuItem6,
            this.exitMenuItem});
      this.fileMenuItem.Name = "fileMenuItem";
      this.fileMenuItem.Size = new System.Drawing.Size(37, 22);
      this.fileMenuItem.Text = "File";
      //
      // saveReportMenuItem
      //
      this.saveReportMenuItem.Name = "saveReportMenuItem";
      this.saveReportMenuItem.Size = new System.Drawing.Size(152, 22);
      this.saveReportMenuItem.Text = "Save Report...";
      this.saveReportMenuItem.Click += new System.EventHandler(this.SaveReportMenuItem_Click);
      //
      // portableModeMenuItem
      //
      this.portableModeMenuItem.Name = "portableModeMenuItem";
      this.portableModeMenuItem.Size = new System.Drawing.Size(152, 22);
      this.portableModeMenuItem.Text = "Portable mode";
      this.portableModeMenuItem.Click += new System.EventHandler(this.PortableModeMenu_Click);
      //
      // resetOnPowerChangedMenuItem
      //
      this.resetOnPowerChangedMenuItem.Name = "resetOnPowerChangedMenuItem";
      this.resetOnPowerChangedMenuItem.Size = new System.Drawing.Size(152, 22);
      this.resetOnPowerChangedMenuItem.Text = "Reset on power changed";
      this.resetOnPowerChangedMenuItem.Click += new System.EventHandler(this.ResetOnPowerChangedMenuItem_Click);
      //
      // MenuItem2
      //
      this.MenuItem2.Name = "MenuItem2";
      this.MenuItem2.Size = new System.Drawing.Size(149, 6);
      //
      // resetMenuItem
      //
      this.resetMenuItem.Name = "resetMenuItem";
      this.resetMenuItem.Size = new System.Drawing.Size(152, 22);
      this.resetMenuItem.Text = "Reset";
      this.resetMenuItem.Click += new System.EventHandler(this.ResetClick);
      //
      // menuItemFileHardware
      //
      this.menuItemFileHardware.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mainboardMenuItem,
            this.cpuMenuItem,
            this.ramMenuItem,
            this.gpuMenuItem,
            this.fanControllerMenuItem,
            this.hddMenuItem,
            this.nicMenuItem,
            this.psuMenuItem,
            this.batteryMenuItem});
      this.menuItemFileHardware.Name = "menuItemFileHardware";
      this.menuItemFileHardware.Size = new System.Drawing.Size(152, 22);
      this.menuItemFileHardware.Text = "Hardware";
      //
      // mainboardMenuItem
      //
      this.mainboardMenuItem.Name = "mainboardMenuItem";
      this.mainboardMenuItem.Size = new System.Drawing.Size(157, 22);
      this.mainboardMenuItem.Text = "Motherboard";
      //
      // cpuMenuItem
      //
      this.cpuMenuItem.Name = "cpuMenuItem";
      this.cpuMenuItem.Size = new System.Drawing.Size(157, 22);
      this.cpuMenuItem.Text = "CPU";
      //
      // ramMenuItem
      //
      this.ramMenuItem.Name = "ramMenuItem";
      this.ramMenuItem.Size = new System.Drawing.Size(157, 22);
      this.ramMenuItem.Text = "RAM";
      //
      // gpuMenuItem
      //
      this.gpuMenuItem.Name = "gpuMenuItem";
      this.gpuMenuItem.Size = new System.Drawing.Size(157, 22);
      this.gpuMenuItem.Text = "GPU";
      //
      // fanControllerMenuItem
      //
      this.fanControllerMenuItem.Name = "fanControllerMenuItem";
      this.fanControllerMenuItem.Size = new System.Drawing.Size(157, 22);
      this.fanControllerMenuItem.Text = "Fan Controllers";
      //
      // hddMenuItem
      //
      this.hddMenuItem.Name = "hddMenuItem";
      this.hddMenuItem.Size = new System.Drawing.Size(157, 22);
      this.hddMenuItem.Text = "Storage Devices";
      //
      // nicMenuItem
      //
      this.nicMenuItem.Name = "nicMenuItem";
      this.nicMenuItem.Size = new System.Drawing.Size(157, 22);
      this.nicMenuItem.Text = "Network";
      //
      // psuMenuItem
      //
      this.psuMenuItem.Name = "psuMenuItem";
      this.psuMenuItem.Size = new System.Drawing.Size(157, 22);
      this.psuMenuItem.Text = "Power supplies";
      //
      // batteryMenuItem
      //
      this.batteryMenuItem.Name = "batteryMenuItem";
      this.batteryMenuItem.Size = new System.Drawing.Size(157, 22);
      this.batteryMenuItem.Text = "Batteries";
      //
      // menuItem6
      //
      this.menuItem6.Name = "menuItem6";
      this.menuItem6.Size = new System.Drawing.Size(149, 6);
      //
      // exitMenuItem
      //
      this.exitMenuItem.Name = "exitMenuItem";
      this.exitMenuItem.Size = new System.Drawing.Size(152, 22);
      this.exitMenuItem.Text = "Exit";
      this.exitMenuItem.Click += new System.EventHandler(this.ExitClick);
      //
      // viewMenuItem
      //
      this.viewMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.resetMinMaxMenuItem,
            this.MenuItem3,
            this.hiddenMenuItem,
            this.gadgetMenuItem,
            this.MenuItem1,
            this.columnsMenuItem});
      this.viewMenuItem.Name = "viewMenuItem";
      this.viewMenuItem.Size = new System.Drawing.Size(44, 22);
      this.viewMenuItem.Text = "View";
      //
      // resetMinMaxMenuItem
      //
      this.resetMinMaxMenuItem.Name = "resetMinMaxMenuItem";
      this.resetMinMaxMenuItem.Size = new System.Drawing.Size(188, 22);
      this.resetMinMaxMenuItem.Text = "Reset Min/Max";
      this.resetMinMaxMenuItem.Click += new System.EventHandler(this.ResetMinMaxMenuItem_Click);
      //
      // MenuItem3
      //
      this.MenuItem3.Name = "MenuItem3";
      this.MenuItem3.Size = new System.Drawing.Size(185, 6);
      //
      // hiddenMenuItem
      //
      this.hiddenMenuItem.Name = "hiddenMenuItem";
      this.hiddenMenuItem.Size = new System.Drawing.Size(188, 22);
      this.hiddenMenuItem.Text = "Show Hidden Sensors";
      //
      // gadgetMenuItem
      //
      this.gadgetMenuItem.Name = "gadgetMenuItem";
      this.gadgetMenuItem.Size = new System.Drawing.Size(188, 22);
      this.gadgetMenuItem.Text = "Show Gadget";
      //
      // MenuItem1
      //
      this.MenuItem1.Name = "MenuItem1";
      this.MenuItem1.Size = new System.Drawing.Size(185, 6);
      //
      // columnsMenuItem
      //
      this.columnsMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.valueMenuItem,
            this.minMenuItem,
            this.maxMenuItem});
      this.columnsMenuItem.Name = "columnsMenuItem";
      this.columnsMenuItem.Size = new System.Drawing.Size(188, 22);
      this.columnsMenuItem.Text = "Columns";
      //
      // valueMenuItem
      //
      this.valueMenuItem.Name = "valueMenuItem";
      this.valueMenuItem.Size = new System.Drawing.Size(102, 22);
      this.valueMenuItem.Text = "Value";
      //
      // minMenuItem
      //
      this.minMenuItem.Name = "minMenuItem";
      this.minMenuItem.Size = new System.Drawing.Size(102, 22);
      this.minMenuItem.Text = "Min";
      //
      // maxMenuItem
      //
      this.maxMenuItem.Name = "maxMenuItem";
      this.maxMenuItem.Size = new System.Drawing.Size(102, 22);
      this.maxMenuItem.Text = "Max";
      //
      // optionsMenuItem
      //
      this.optionsMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.startMinMenuItem,
            this.minTrayMenuItem,
            this.minCloseMenuItem,
            this.startupMenuItem,
            this.separatorMenuItem,
            this.temperatureUnitsMenuItem,
            this.themeMenuItem,
            this.logSeparatorMenuItem,
            this.logSensorsMenuItem,
            this.loggingIntervalMenuItem,
            this.updateIntervalMenuItem,
            this.fileRotationMethod,
            this.sensorValuesTimeWindowMenuItem,
            this.webMenuItemSeparator,
            this.webMenuItem});
      this.optionsMenuItem.Name = "optionsMenuItem";
      this.optionsMenuItem.Size = new System.Drawing.Size(61, 22);
      this.optionsMenuItem.Text = "Options";
      //
      // startMinMenuItem
      //
      this.startMinMenuItem.Name = "startMinMenuItem";
      this.startMinMenuItem.Size = new System.Drawing.Size(221, 22);
      this.startMinMenuItem.Text = "Start Minimized";
      //
      // minTrayMenuItem
      //
      this.minTrayMenuItem.Name = "minTrayMenuItem";
      this.minTrayMenuItem.Size = new System.Drawing.Size(221, 22);
      this.minTrayMenuItem.Text = "Minimize To Tray";
      //
      // minCloseMenuItem
      //
      this.minCloseMenuItem.Name = "minCloseMenuItem";
      this.minCloseMenuItem.Size = new System.Drawing.Size(221, 22);
      this.minCloseMenuItem.Text = "Minimize On Close";
      //
      // startupMenuItem
      //
      this.startupMenuItem.Name = "startupMenuItem";
      this.startupMenuItem.Size = new System.Drawing.Size(221, 22);
      this.startupMenuItem.Text = "Run On Windows Startup";
      //
      // separatorMenuItem
      //
      this.separatorMenuItem.Name = "separatorMenuItem";
      this.separatorMenuItem.Size = new System.Drawing.Size(218, 6);
      //
      // temperatureUnitsMenuItem
      //
      this.temperatureUnitsMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.celsiusMenuItem,
            this.fahrenheitMenuItem});
      this.temperatureUnitsMenuItem.Name = "temperatureUnitsMenuItem";
      this.temperatureUnitsMenuItem.Size = new System.Drawing.Size(221, 22);
      this.temperatureUnitsMenuItem.Text = "Temperature Unit";
      //
      // celsiusMenuItem
      //
      this.celsiusMenuItem.CheckOnClick = true;
      this.celsiusMenuItem.Name = "celsiusMenuItem";
      this.celsiusMenuItem.Size = new System.Drawing.Size(180, 22);
      this.celsiusMenuItem.Text = "Celsius";
      this.celsiusMenuItem.Click += new System.EventHandler(this.CelsiusMenuItem_Click);
      //
      // fahrenheitMenuItem
      //
      this.fahrenheitMenuItem.CheckOnClick = true;
      this.fahrenheitMenuItem.Name = "fahrenheitMenuItem";
      this.fahrenheitMenuItem.Size = new System.Drawing.Size(180, 22);
      this.fahrenheitMenuItem.Text = "Fahrenheit";
      this.fahrenheitMenuItem.Click += new System.EventHandler(this.FahrenheitMenuItem_Click);
      //
      // themeMenuItem
      //
      this.themeMenuItem.Name = "themeMenuItem";
      this.themeMenuItem.Size = new System.Drawing.Size(221, 22);
      this.themeMenuItem.Text = "Theme";
      //
      // logSeparatorMenuItem
      //
      this.logSeparatorMenuItem.Name = "logSeparatorMenuItem";
      this.logSeparatorMenuItem.Size = new System.Drawing.Size(218, 6);
      //
      // logSensorsMenuItem
      //
      this.logSensorsMenuItem.Name = "logSensorsMenuItem";
      this.logSensorsMenuItem.Size = new System.Drawing.Size(221, 22);
      this.logSensorsMenuItem.Text = "Log Sensors";
      //
      // loggingIntervalMenuItem
      //
      this.loggingIntervalMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.log1sMenuItem,
            this.log2sMenuItem,
            this.log5sMenuItem,
            this.log10sMenuItem,
            this.log30sMenuItem,
            this.log1minMenuItem,
            this.log2minMenuItem,
            this.log5minMenuItem,
            this.log10minMenuItem,
            this.log30minMenuItem,
            this.log1hMenuItem,
            this.log2hMenuItem,
            this.log6hMenuItem});
      this.loggingIntervalMenuItem.Name = "loggingIntervalMenuItem";
      this.loggingIntervalMenuItem.Size = new System.Drawing.Size(221, 22);
      this.loggingIntervalMenuItem.Text = "Logging Interval";
      //
      // log1sMenuItem
      //
      this.log1sMenuItem.CheckOnClick = true;
      this.log1sMenuItem.Name = "log1sMenuItem";
      this.log1sMenuItem.Size = new System.Drawing.Size(107, 22);
      this.log1sMenuItem.Text = "1s";
      //
      // log2sMenuItem
      //
      this.log2sMenuItem.CheckOnClick = true;
      this.log2sMenuItem.Name = "log2sMenuItem";
      this.log2sMenuItem.Size = new System.Drawing.Size(107, 22);
      this.log2sMenuItem.Text = "2s";
      //
      // log5sMenuItem
      //
      this.log5sMenuItem.CheckOnClick = true;
      this.log5sMenuItem.Name = "log5sMenuItem";
      this.log5sMenuItem.Size = new System.Drawing.Size(107, 22);
      this.log5sMenuItem.Text = "5s";
      //
      // log10sMenuItem
      //
      this.log10sMenuItem.CheckOnClick = true;
      this.log10sMenuItem.Name = "log10sMenuItem";
      this.log10sMenuItem.Size = new System.Drawing.Size(107, 22);
      this.log10sMenuItem.Text = "10s";
      //
      // log30sMenuItem
      //
      this.log30sMenuItem.CheckOnClick = true;
      this.log30sMenuItem.Name = "log30sMenuItem";
      this.log30sMenuItem.Size = new System.Drawing.Size(107, 22);
      this.log30sMenuItem.Text = "30s";
      //
      // log1minMenuItem
      //
      this.log1minMenuItem.CheckOnClick = true;
      this.log1minMenuItem.Name = "log1minMenuItem";
      this.log1minMenuItem.Size = new System.Drawing.Size(107, 22);
      this.log1minMenuItem.Text = "1min";
      //
      // log2minMenuItem
      //
      this.log2minMenuItem.CheckOnClick = true;
      this.log2minMenuItem.Name = "log2minMenuItem";
      this.log2minMenuItem.Size = new System.Drawing.Size(107, 22);
      this.log2minMenuItem.Text = "2min";
      //
      // log5minMenuItem
      //
      this.log5minMenuItem.CheckOnClick = true;
      this.log5minMenuItem.Name = "log5minMenuItem";
      this.log5minMenuItem.Size = new System.Drawing.Size(107, 22);
      this.log5minMenuItem.Text = "5min";
      //
      // log10minMenuItem
      //
      this.log10minMenuItem.CheckOnClick = true;
      this.log10minMenuItem.Name = "log10minMenuItem";
      this.log10minMenuItem.Size = new System.Drawing.Size(107, 22);
      this.log10minMenuItem.Text = "10min";
      //
      // log30minMenuItem
      //
      this.log30minMenuItem.CheckOnClick = true;
      this.log30minMenuItem.Name = "log30minMenuItem";
      this.log30minMenuItem.Size = new System.Drawing.Size(107, 22);
      this.log30minMenuItem.Text = "30min";
      //
      // log1hMenuItem
      //
      this.log1hMenuItem.CheckOnClick = true;
      this.log1hMenuItem.Name = "log1hMenuItem";
      this.log1hMenuItem.Size = new System.Drawing.Size(107, 22);
      this.log1hMenuItem.Text = "1h";
      //
      // log2hMenuItem
      //
      this.log2hMenuItem.CheckOnClick = true;
      this.log2hMenuItem.Name = "log2hMenuItem";
      this.log2hMenuItem.Size = new System.Drawing.Size(107, 22);
      this.log2hMenuItem.Text = "2h";
      //
      // log6hMenuItem
      //
      this.log6hMenuItem.CheckOnClick = true;
      this.log6hMenuItem.Name = "log6hMenuItem";
      this.log6hMenuItem.Size = new System.Drawing.Size(107, 22);
      this.log6hMenuItem.Text = "6h";
      //
      // updateIntervalMenuItem
      //
      this.updateIntervalMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.updateInterval250msMenuItem,
            this.updateInterval500msMenuItem,
            this.updateInterval1sMenuItem,
            this.updateInterval2sMenuItem,
            this.updateInterval5sMenuItem,
            this.updateInterval10sMenuItem});
      this.updateIntervalMenuItem.Name = "updateIntervalMenuItem";
      this.updateIntervalMenuItem.Size = new System.Drawing.Size(221, 22);
      this.updateIntervalMenuItem.Text = "Update Interval";
      //
      // updateInterval250msMenuItem
      //
      this.updateInterval250msMenuItem.CheckOnClick = true;
      this.updateInterval250msMenuItem.Name = "updateInterval250msMenuItem";
      this.updateInterval250msMenuItem.Size = new System.Drawing.Size(108, 22);
      this.updateInterval250msMenuItem.Text = "250ms";
      //
      // updateInterval500msMenuItem
      //
      this.updateInterval500msMenuItem.CheckOnClick = true;
      this.updateInterval500msMenuItem.Name = "updateInterval500msMenuItem";
      this.updateInterval500msMenuItem.Size = new System.Drawing.Size(108, 22);
      this.updateInterval500msMenuItem.Text = "500ms";
      //
      // updateInterval1sMenuItem
      //
      this.updateInterval1sMenuItem.CheckOnClick = true;
      this.updateInterval1sMenuItem.Name = "updateInterval1sMenuItem";
      this.updateInterval1sMenuItem.Size = new System.Drawing.Size(108, 22);
      this.updateInterval1sMenuItem.Text = "1s";
      //
      // updateInterval2sMenuItem
      //
      this.updateInterval2sMenuItem.CheckOnClick = true;
      this.updateInterval2sMenuItem.Name = "updateInterval2sMenuItem";
      this.updateInterval2sMenuItem.Size = new System.Drawing.Size(108, 22);
      this.updateInterval2sMenuItem.Text = "2s";
      //
      // updateInterval5sMenuItem
      //
      this.updateInterval5sMenuItem.CheckOnClick = true;
      this.updateInterval5sMenuItem.Name = "updateInterval5sMenuItem";
      this.updateInterval5sMenuItem.Size = new System.Drawing.Size(108, 22);
      this.updateInterval5sMenuItem.Text = "5s";
      //
      // updateInterval10sMenuItem
      //
      this.updateInterval10sMenuItem.CheckOnClick = true;
      this.updateInterval10sMenuItem.Name = "updateInterval10sMenuItem";
      this.updateInterval10sMenuItem.Size = new System.Drawing.Size(108, 22);
      this.updateInterval10sMenuItem.Text = "10s";
      //
      // fileRotationMethod
      //
      this.fileRotationMethod.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.perSessionFileRotationMenuItem,
            this.dailyFileRotationMenuItem});
      this.fileRotationMethod.Name = "fileRotationMethod";
      this.fileRotationMethod.Size = new System.Drawing.Size(221, 22);
      this.fileRotationMethod.Text = "File rotation method";
      this.fileRotationMethod.ToolTipText = "Determine how the log file should rotate.";
      //
      // perSessionFileRotationMenuItem
      //
      this.perSessionFileRotationMenuItem.CheckOnClick = true;
      this.perSessionFileRotationMenuItem.Name = "perSessionFileRotationMenuItem";
      this.perSessionFileRotationMenuItem.Size = new System.Drawing.Size(132, 22);
      this.perSessionFileRotationMenuItem.Text = "Per session";
      this.perSessionFileRotationMenuItem.ToolTipText = "Create a new log file for each logging session";
      this.perSessionFileRotationMenuItem.Click += new System.EventHandler(this.perSessionFileRotationMenuItem_Click);
      //
      // dailyFileRotationMenuItem
      //
      this.dailyFileRotationMenuItem.CheckOnClick = true;
      this.dailyFileRotationMenuItem.Name = "dailyFileRotationMenuItem";
      this.dailyFileRotationMenuItem.Size = new System.Drawing.Size(132, 22);
      this.dailyFileRotationMenuItem.Text = "Daily";
      this.dailyFileRotationMenuItem.ToolTipText = "Create a new log file every day.\n If a file already exists for the current day, n" +
    "ew data will be appended at the end.";
      this.dailyFileRotationMenuItem.Click += new System.EventHandler(this.dailyFileRotationMenuItem_Click);
      //
      // sensorValuesTimeWindowMenuItem
      //
      this.sensorValuesTimeWindowMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.timeWindow30sMenuItem,
            this.timeWindow1minMenuItem,
            this.timeWindow2minMenuItem,
            this.timeWindow5minMenuItem,
            this.timeWindow10minMenuItem,
            this.timeWindow30minMenuItem,
            this.timeWindow1hMenuItem,
            this.timeWindow2hMenuItem,
            this.timeWindow6hMenuItem,
            this.timeWindow12hMenuItem,
            this.timeWindow24hMenuItem});
      this.sensorValuesTimeWindowMenuItem.Name = "sensorValuesTimeWindowMenuItem";
      this.sensorValuesTimeWindowMenuItem.Size = new System.Drawing.Size(221, 22);
      this.sensorValuesTimeWindowMenuItem.Text = "Sensor Values Time Window";
      //
      // timeWindow30sMenuItem
      //
      this.timeWindow30sMenuItem.CheckOnClick = true;
      this.timeWindow30sMenuItem.Name = "timeWindow30sMenuItem";
      this.timeWindow30sMenuItem.Size = new System.Drawing.Size(107, 22);
      this.timeWindow30sMenuItem.Text = "30s";
      //
      // timeWindow1minMenuItem
      //
      this.timeWindow1minMenuItem.CheckOnClick = true;
      this.timeWindow1minMenuItem.Name = "timeWindow1minMenuItem";
      this.timeWindow1minMenuItem.Size = new System.Drawing.Size(107, 22);
      this.timeWindow1minMenuItem.Text = "1min";
      //
      // timeWindow2minMenuItem
      //
      this.timeWindow2minMenuItem.CheckOnClick = true;
      this.timeWindow2minMenuItem.Name = "timeWindow2minMenuItem";
      this.timeWindow2minMenuItem.Size = new System.Drawing.Size(107, 22);
      this.timeWindow2minMenuItem.Text = "2min";
      //
      // timeWindow5minMenuItem
      //
      this.timeWindow5minMenuItem.CheckOnClick = true;
      this.timeWindow5minMenuItem.Name = "timeWindow5minMenuItem";
      this.timeWindow5minMenuItem.Size = new System.Drawing.Size(107, 22);
      this.timeWindow5minMenuItem.Text = "5min";
      //
      // timeWindow10minMenuItem
      //
      this.timeWindow10minMenuItem.CheckOnClick = true;
      this.timeWindow10minMenuItem.Name = "timeWindow10minMenuItem";
      this.timeWindow10minMenuItem.Size = new System.Drawing.Size(107, 22);
      this.timeWindow10minMenuItem.Text = "10min";
      //
      // timeWindow30minMenuItem
      //
      this.timeWindow30minMenuItem.CheckOnClick = true;
      this.timeWindow30minMenuItem.Name = "timeWindow30minMenuItem";
      this.timeWindow30minMenuItem.Size = new System.Drawing.Size(107, 22);
      this.timeWindow30minMenuItem.Text = "30min";
      //
      // timeWindow1hMenuItem
      //
      this.timeWindow1hMenuItem.CheckOnClick = true;
      this.timeWindow1hMenuItem.Name = "timeWindow1hMenuItem";
      this.timeWindow1hMenuItem.Size = new System.Drawing.Size(107, 22);
      this.timeWindow1hMenuItem.Text = "1h";
      //
      // timeWindow2hMenuItem
      //
      this.timeWindow2hMenuItem.CheckOnClick = true;
      this.timeWindow2hMenuItem.Name = "timeWindow2hMenuItem";
      this.timeWindow2hMenuItem.Size = new System.Drawing.Size(107, 22);
      this.timeWindow2hMenuItem.Text = "2h";
      //
      // timeWindow6hMenuItem
      //
      this.timeWindow6hMenuItem.CheckOnClick = true;
      this.timeWindow6hMenuItem.Name = "timeWindow6hMenuItem";
      this.timeWindow6hMenuItem.Size = new System.Drawing.Size(107, 22);
      this.timeWindow6hMenuItem.Text = "6h";
      //
      // timeWindow12hMenuItem
      //
      this.timeWindow12hMenuItem.CheckOnClick = true;
      this.timeWindow12hMenuItem.Name = "timeWindow12hMenuItem";
      this.timeWindow12hMenuItem.Size = new System.Drawing.Size(107, 22);
      this.timeWindow12hMenuItem.Text = "12h";
      //
      // timeWindow24hMenuItem
      //
      this.timeWindow24hMenuItem.CheckOnClick = true;
      this.timeWindow24hMenuItem.Name = "timeWindow24hMenuItem";
      this.timeWindow24hMenuItem.Size = new System.Drawing.Size(107, 22);
      this.timeWindow24hMenuItem.Text = "24h";
      //
      // webMenuItemSeparator
      //
      this.webMenuItemSeparator.Name = "webMenuItemSeparator";
      this.webMenuItemSeparator.Size = new System.Drawing.Size(218, 6);
      //
      // webMenuItem
      //
      this.webMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.runWebServerMenuItem,
            this.openWebServerMenuItem,
            this.serverPortMenuItem,
            this.authWebServerMenuItem});
      this.webMenuItem.Name = "webMenuItem";
      this.webMenuItem.Size = new System.Drawing.Size(221, 22);
      this.webMenuItem.Text = "Remote Web Server";
      //
      // runWebServerMenuItem
      //
      this.runWebServerMenuItem.Name = "runWebServerMenuItem";
      this.runWebServerMenuItem.Size = new System.Drawing.Size(153, 22);
      this.runWebServerMenuItem.Text = "Run";
      //
      // openWebServerMenuItem
      //
      this.openWebServerMenuItem.Name = "openWebServerMenuItem";
      this.openWebServerMenuItem.Size = new System.Drawing.Size(153, 22);
      this.openWebServerMenuItem.Text = "Open";
      //
      // serverPortMenuItem
      //
      this.serverPortMenuItem.Name = "serverPortMenuItem";
      this.serverPortMenuItem.Size = new System.Drawing.Size(153, 22);
      this.serverPortMenuItem.Text = "Port";
      this.serverPortMenuItem.Click += new System.EventHandler(this.ServerInterfacePortMenuItem_Click);
      //
      // authWebServerMenuItem
      //
      this.authWebServerMenuItem.Name = "authWebServerMenuItem";
      this.authWebServerMenuItem.Size = new System.Drawing.Size(153, 22);
      this.authWebServerMenuItem.Text = "Authentication";
      this.authWebServerMenuItem.Click += new System.EventHandler(this.AuthWebServerMenuItem_Click);
      //
      // helpMenuItem
      //
      this.helpMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItemCheckUpdates,
            this.aboutMenuItem});
      this.helpMenuItem.Name = "helpMenuItem";
      this.helpMenuItem.Size = new System.Drawing.Size(44, 22);
      this.helpMenuItem.Text = "Help";
      //
      // menuItemCheckUpdates
      //
      this.menuItemCheckUpdates.Name = "menuItemCheckUpdates";
      this.menuItemCheckUpdates.Size = new System.Drawing.Size(170, 22);
      this.menuItemCheckUpdates.Text = "Check for updates";
      this.menuItemCheckUpdates.Click += new System.EventHandler(this.menuItemCheckUpdates_Click);
      //
      // aboutMenuItem
      //
      this.aboutMenuItem.Name = "aboutMenuItem";
      this.aboutMenuItem.Size = new System.Drawing.Size(170, 22);
      this.aboutMenuItem.Text = "About";
      this.aboutMenuItem.Click += new System.EventHandler(this.AboutMenuItem_Click);
      //
      // treeContextMenu
      //
      this.treeContextMenu.ImageScalingSize = new System.Drawing.Size(24, 24);
      this.treeContextMenu.Name = "treeContextMenu";
      this.treeContextMenu.Size = new System.Drawing.Size(61, 4);
      //
      // saveFileDialog
      //
      this.saveFileDialog.DefaultExt = "txt";
      this.saveFileDialog.FileName = "OpenHardwareMonitor.Report.txt";
      this.saveFileDialog.Filter = "Text Documents|*.txt|All Files|*.*";
      this.saveFileDialog.RestoreDirectory = true;
      this.saveFileDialog.Title = "Save Report As";
      //
      // timer
      //
      this.timer.Interval = 1000;
      this.timer.Tick += new System.EventHandler(this.Timer_Tick);
      //
      // treeView
      //
      this.treeView.BackColor = System.Drawing.SystemColors.Window;
      this.treeView.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.treeView.Columns.Add(this.sensor);
      this.treeView.Columns.Add(this.value);
      this.treeView.Columns.Add(this.min);
      this.treeView.Columns.Add(this.max);
      this.treeView.DefaultToolTipProvider = null;
      this.treeView.Dock = System.Windows.Forms.DockStyle.Fill;
      this.treeView.DragDropMarkColor = System.Drawing.Color.Black;
      this.treeView.FullRowSelect = true;
      this.treeView.GridLineStyle = Aga.Controls.Tree.GridLineStyle.Horizontal;
      this.treeView.LineColor = System.Drawing.SystemColors.ControlDark;
      this.treeView.Location = new System.Drawing.Point(0, 24);
      this.treeView.Model = null;
      this.treeView.Name = "treeView";
      this.treeView.NodeControls.Add(this.nodeImage);
      this.treeView.NodeControls.Add(this.nodeTextBoxText);
      this.treeView.NodeControls.Add(this.nodeTextBoxValue);
      this.treeView.NodeControls.Add(this.nodeTextBoxMin);
      this.treeView.NodeControls.Add(this.nodeTextBoxMax);
      this.treeView.SelectedNode = null;
      this.treeView.Size = new System.Drawing.Size(418, 509);
      this.treeView.TabIndex = 0;
      this.treeView.Text = "treeView";
      this.treeView.UseColumns = true;
      this.treeView.Collapsed += new System.EventHandler<Aga.Controls.Tree.TreeViewAdvEventArgs>(this.TreeView_CollapsedOrExpanded);
      this.treeView.Expanded += new System.EventHandler<Aga.Controls.Tree.TreeViewAdvEventArgs>(this.TreeView_CollapsedOrExpanded);
      this.treeView.NodeMouseDoubleClick += new System.EventHandler<Aga.Controls.Tree.TreeNodeAdvMouseEventArgs>(this.TreeView_NodeMouseDoubleClick);
      this.treeView.SizeChanged += new System.EventHandler(this.TreeView_SizeChanged);
      this.treeView.Click += new System.EventHandler(this.TreeView_Click);
      this.treeView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TreeView_MouseDown);
      this.treeView.MouseMove += new System.Windows.Forms.MouseEventHandler(this.TreeView_MouseMove);
      this.treeView.MouseUp += new System.Windows.Forms.MouseEventHandler(this.TreeView_MouseUp);
      //
      // MainForm
      //
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(418, 533);
      this.Controls.Add(this.treeView);
      this.Controls.Add(this.mainMenu);
      this.MainMenuStrip = this.mainMenu;
      this.Name = "MainForm";
      this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
      this.Load += new System.EventHandler(this.MainForm_Load);
      this.ResizeEnd += new System.EventHandler(this.MainForm_MoveOrResize);
      this.Move += new System.EventHandler(this.MainForm_MoveOrResize);
      this.mainMenu.ResumeLayout(false);
      this.mainMenu.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

        }

        #endregion

        private Aga.Controls.Tree.TreeViewAdv treeView;
        private System.Windows.Forms.MenuStrip mainMenu;
        private System.Windows.Forms.ToolStripMenuItem fileMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitMenuItem;
        private Aga.Controls.Tree.TreeColumn sensor;
        private Aga.Controls.Tree.TreeColumn value;
        private Aga.Controls.Tree.TreeColumn min;
        private Aga.Controls.Tree.TreeColumn max;
        private Aga.Controls.Tree.NodeControls.NodeIcon nodeImage;
        private Aga.Controls.Tree.NodeControls.NodeTextBox nodeTextBoxText;
        private Aga.Controls.Tree.NodeControls.NodeTextBox nodeTextBoxValue;
        private Aga.Controls.Tree.NodeControls.NodeTextBox nodeTextBoxMin;
        private Aga.Controls.Tree.NodeControls.NodeTextBox nodeTextBoxMax;
        private System.Windows.Forms.ToolStripMenuItem viewMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpMenuItem;
        private System.Windows.Forms.ToolStripMenuItem menuItemCheckUpdates;
        private System.Windows.Forms.ToolStripMenuItem aboutMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveReportMenuItem;
        private System.Windows.Forms.ToolStripMenuItem portableModeMenuItem;
        private System.Windows.Forms.ToolStripMenuItem resetOnPowerChangedMenuItem;
        private System.Windows.Forms.ToolStripMenuItem optionsMenuItem;
        private System.Windows.Forms.ToolStripMenuItem hddMenuItem;
        private System.Windows.Forms.ToolStripMenuItem minTrayMenuItem;
        private System.Windows.Forms.ToolStripSeparator separatorMenuItem;
        private System.Windows.Forms.ContextMenuStrip treeContextMenu;
        private System.Windows.Forms.ToolStripMenuItem startMinMenuItem;
        private System.Windows.Forms.ToolStripMenuItem startupMenuItem;
        private System.Windows.Forms.SaveFileDialog saveFileDialog;
        private System.Windows.Forms.Timer timer;
        private System.Windows.Forms.ToolStripMenuItem hiddenMenuItem;
        private System.Windows.Forms.ToolStripSeparator MenuItem1;
        private System.Windows.Forms.ToolStripMenuItem columnsMenuItem;
        private System.Windows.Forms.ToolStripMenuItem valueMenuItem;
        private System.Windows.Forms.ToolStripMenuItem minMenuItem;
        private System.Windows.Forms.ToolStripMenuItem maxMenuItem;
        private System.Windows.Forms.ToolStripMenuItem temperatureUnitsMenuItem;
        private System.Windows.Forms.ToolStripSeparator webMenuItemSeparator;
        private ToolStripRadioButtonMenuItem celsiusMenuItem;
        private ToolStripRadioButtonMenuItem fahrenheitMenuItem;
        private System.Windows.Forms.ToolStripSeparator MenuItem2;
        private System.Windows.Forms.ToolStripMenuItem resetMinMaxMenuItem;
        private System.Windows.Forms.ToolStripSeparator MenuItem3;
        private System.Windows.Forms.ToolStripMenuItem gadgetMenuItem;
        private System.Windows.Forms.ToolStripMenuItem minCloseMenuItem;
        private System.Windows.Forms.ToolStripMenuItem resetMenuItem;
        private System.Windows.Forms.ToolStripSeparator menuItem6;
        private System.Windows.Forms.ToolStripMenuItem webMenuItem;
        private System.Windows.Forms.ToolStripMenuItem runWebServerMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openWebServerMenuItem;
        private System.Windows.Forms.ToolStripMenuItem serverPortMenuItem;
        private System.Windows.Forms.ToolStripMenuItem menuItemFileHardware;
        private System.Windows.Forms.ToolStripMenuItem mainboardMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cpuMenuItem;
        private System.Windows.Forms.ToolStripMenuItem gpuMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fanControllerMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ramMenuItem;
        private System.Windows.Forms.ToolStripMenuItem logSensorsMenuItem;
        private System.Windows.Forms.ToolStripSeparator logSeparatorMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loggingIntervalMenuItem;
        private ToolStripRadioButtonMenuItem log1sMenuItem;
        private ToolStripRadioButtonMenuItem log2sMenuItem;
        private ToolStripRadioButtonMenuItem log5sMenuItem;
        private ToolStripRadioButtonMenuItem log10sMenuItem;
        private ToolStripRadioButtonMenuItem log30sMenuItem;
        private ToolStripRadioButtonMenuItem log1minMenuItem;
        private ToolStripRadioButtonMenuItem log2minMenuItem;
        private ToolStripRadioButtonMenuItem log5minMenuItem;
        private ToolStripRadioButtonMenuItem log10minMenuItem;
        private ToolStripRadioButtonMenuItem log30minMenuItem;
        private ToolStripRadioButtonMenuItem log1hMenuItem;
        private ToolStripRadioButtonMenuItem log2hMenuItem;
        private ToolStripRadioButtonMenuItem log6hMenuItem;
        private System.Windows.Forms.ToolStripMenuItem updateIntervalMenuItem;
        private ToolStripRadioButtonMenuItem updateInterval250msMenuItem;
        private ToolStripRadioButtonMenuItem updateInterval500msMenuItem;
        private ToolStripRadioButtonMenuItem updateInterval1sMenuItem;
        private ToolStripRadioButtonMenuItem updateInterval2sMenuItem;
        private ToolStripRadioButtonMenuItem updateInterval5sMenuItem;
        private ToolStripRadioButtonMenuItem updateInterval10sMenuItem;
        private System.Windows.Forms.ToolStripMenuItem nicMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sensorValuesTimeWindowMenuItem;
        private ToolStripRadioButtonMenuItem timeWindow30sMenuItem;
        private ToolStripRadioButtonMenuItem timeWindow1minMenuItem;
        private ToolStripRadioButtonMenuItem timeWindow2minMenuItem;
        private ToolStripRadioButtonMenuItem timeWindow5minMenuItem;
        private ToolStripRadioButtonMenuItem timeWindow10minMenuItem;
        private ToolStripRadioButtonMenuItem timeWindow30minMenuItem;
        private ToolStripRadioButtonMenuItem timeWindow1hMenuItem;
        private ToolStripRadioButtonMenuItem timeWindow2hMenuItem;
        private ToolStripRadioButtonMenuItem timeWindow6hMenuItem;
        private ToolStripRadioButtonMenuItem timeWindow12hMenuItem;
        private ToolStripRadioButtonMenuItem timeWindow24hMenuItem;
        private System.Windows.Forms.ToolStripMenuItem authWebServerMenuItem;
        private System.Windows.Forms.ToolStripMenuItem psuMenuItem;
        private System.Windows.Forms.ToolStripMenuItem batteryMenuItem;
        private System.ComponentModel.BackgroundWorker backgroundUpdater;
        private System.Windows.Forms.ToolStripMenuItem fileRotationMethod;
        private ToolStripRadioButtonMenuItem perSessionFileRotationMenuItem;
        private ToolStripRadioButtonMenuItem dailyFileRotationMenuItem;
        private System.Windows.Forms.ToolStripMenuItem themeMenuItem;
    }
}

