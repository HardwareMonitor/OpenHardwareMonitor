using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using OpenHardwareMonitor.Hardware;

namespace OpenHardwareMonitor.UI;

internal class SensorGadget : Gadget
{
    private const int BorderWidth = 6;

    private readonly float _scale;
    private float _fontSize;
    private int _iconSize;
    private int _hardwareLineHeight;
    private int _sensorLineHeight;
    private int _rightMargin;
    private int _leftMargin;
    private int _topMargin;
    private int _bottomMargin;
    private int _progressWidth;

    private readonly IDictionary<IHardware, IList<ISensor>> _sensors = new SortedDictionary<IHardware, IList<ISensor>>(new HardwareComparer());
    private const int GraphTimeWindowSeconds = 90;
    private const int GraphPadding = 2;
    private int GraphHeight => 3 * _sensorLineHeight;

    private struct HistoryPoint
    {
        public DateTime Time;
        public float Value;
    }

    private readonly Dictionary<string, List<HistoryPoint>> _history = new();
    private DateTime _lastSample = DateTime.MinValue;
    private Font _captionFont;
    private readonly Pen _borderPen = new Pen(Color.LightSlateGray, 1f);
    private readonly Brush _captionBrush = new SolidBrush(Color.FromArgb(150, 150, 150));
    private static readonly Color[] _graphPalette =
    {
        Color.FromArgb(0x90, 0xEE, 0x90), Color.FromArgb(0xFF, 0xF6, 0x2A), Color.FromArgb(0xFA, 0xBA, 0x00), Color.FromArgb(0xEC, 0x75, 0x27),
        Color.FromArgb(0xE5, 0x31, 0x6C), Color.FromArgb(0xE2, 0x00, 0x3B), Color.FromArgb(0xD7, 0x00, 0x7A), Color.FromArgb(0xAE, 0x32, 0x88),
        Color.FromArgb(0x89, 0x4B, 0x94), Color.FromArgb(0x5B, 0x50, 0x99), Color.FromArgb(0x4D, 0x61, 0xA8), Color.FromArgb(0x4D, 0x81, 0xA2),
        Color.FromArgb(0x4D, 0xA0, 0xAE), Color.FromArgb(0x4D, 0xB8, 0x96), Color.FromArgb(0x64, 0xC5, 0x66), Color.FromArgb(0x79, 0xB6, 0x3E)
    };

    // The base palette is a smooth hue cycle; every next cycle is alternately lightened and darkened so that colors do not repeat.
    private static Color GetPaletteColor(int index)
    {
        Color color = _graphPalette[index % _graphPalette.Length];
        int cycle = index / _graphPalette.Length;
        if (cycle == 0)
            return color;

        double amount = Math.Min(0.6, 0.3 + 0.1 * ((cycle - 1) / 2));
        int target = cycle % 2 == 1 ? 255 : 0;
        return Color.FromArgb(Blend(color.R, target, amount), Blend(color.G, target, amount), Blend(color.B, target, amount));
    }

    private static int Blend(int from, int to, double amount)
    {
        return (int)Math.Round(from + (to - from) * amount);
    }

    private readonly IDictionary<string, SensorStyle> _styles = new Dictionary<string, SensorStyle>();
    private readonly PersistentSettings _settings;
    private readonly UserOption _hardwareNames;

    private Font _largeFont;
    private Font _smallFont;
    private Brush _backBrush;
    private Brush _textBrush;
    private StringFormat _stringFormat;
    private StringFormat _trimStringFormat;
    private StringFormat _alignRightStringFormat;

    public SensorGadget(IComputer computer, PersistentSettings settings)
    {
        _settings = settings;
        computer.HardwareAdded += HardwareAdded;
        computer.HardwareRemoved += HardwareRemoved;

        _stringFormat = new StringFormat { FormatFlags = StringFormatFlags.NoWrap };
        _trimStringFormat = new StringFormat { Trimming = StringTrimming.EllipsisCharacter, FormatFlags = StringFormatFlags.NoWrap };
        _alignRightStringFormat = new StringFormat { Alignment = StringAlignment.Far, FormatFlags = StringFormatFlags.NoWrap };

        Location = new Point(settings.GetValue("sensorGadget.Location.X", 100), settings.GetValue("sensorGadget.Location.Y", 100));
        LocationChanged += delegate
        {
            settings.SetValue("sensorGadget.Location.X", Location.X);
            settings.SetValue("sensorGadget.Location.Y", Location.Y);
        };

        // get the custom to default dpi ratio
        using (Bitmap b = new Bitmap(1, 1))
        {
            _scale = b.HorizontalResolution / 96.0f;
        }

        SetFontSize(settings.GetValue("sensorGadget.FontSize", 7.5f));
        Resize(settings.GetValue("sensorGadget.Width", Size.Width));

        ContextMenuStrip contextMenuStrip = new ContextMenuStrip();
        contextMenuStrip.Renderer = new ThemedToolStripRenderer();
        ToolStripMenuItem hardwareNamesItem = new ToolStripMenuItem("Hardware Names");
        contextMenuStrip.Items.Add(hardwareNamesItem);
        contextMenuStrip.Items.Add(new ToolStripMenuItem("Increase Font Size", null, (_, _) =>
        {
            float newSize = _fontSize + 1;
            SetFontSize(newSize);
            settings.SetValue("sensorGadget.FontSize", newSize);
        }));
        contextMenuStrip.Items.Add(new ToolStripMenuItem("Decrease Font Size", null, (_, _) =>
        {
            float newSize = _fontSize - 1;
            if (newSize <= 0) return;
            SetFontSize(newSize);
            settings.SetValue("sensorGadget.FontSize", newSize);
        }));

        Color fontColor = settings.GetValue("sensorGadget.FontColor", Color.White);
        SetFontColor(fontColor);
        contextMenuStrip.Items.Add(new ToolStripMenuItem("Font Color", null, (_, _) =>
        {
            using var dialog = new ColorDialog();
            dialog.Color = fontColor;
            if (dialog.ShowDialog() != DialogResult.OK) return;
            SetFontColor(dialog.Color);
            settings.SetValue("sensorGadget.FontColor", dialog.Color);
        }));
        Color backColor = settings.GetValue("sensorGadget.BackColor", Color.Transparent);
        SetBackColor(backColor);
        contextMenuStrip.Items.Add(new ToolStripMenuItem("Back Color", null, (_, _) =>
        {
            using var dialog = new ColorDialog();
            dialog.Color = backColor;
            if (dialog.ShowDialog() != DialogResult.OK) return;
            SetBackColor(dialog.Color);
            settings.SetValue("sensorGadget.BackColor", dialog.Color);
        }));
        contextMenuStrip.Items.Add(new ToolStripSeparator());
        ToolStripMenuItem lockItem = new ToolStripMenuItem("Lock Position and Size");
        contextMenuStrip.Items.Add(lockItem);
        contextMenuStrip.Items.Add(new ToolStripSeparator());
        ToolStripMenuItem alwaysOnTopItem = new ToolStripMenuItem("Always on Top");
        contextMenuStrip.Items.Add(alwaysOnTopItem);
        ToolStripMenuItem opacityMenu = new ToolStripMenuItem("Opacity");
        contextMenuStrip.Items.Add(opacityMenu);
        Opacity = (byte)settings.GetValue("sensorGadget.Opacity", 255);

        for (int i = 0; i < 5; i++)
        {
            ToolStripMenuItem item = new ToolStripMenuItem($"{20 * (i + 1)} %");
            byte o = (byte)(51 * (i + 1));
            item.Checked = Opacity == o;
            item.Click += delegate
            {
                Opacity = o;
                settings.SetValue("sensorGadget.Opacity", Opacity);
                foreach (ToolStripMenuItem mi in opacityMenu.DropDownItems)
                    mi.Checked = mi == item;
            };
            opacityMenu.DropDownItems.Add(item);
        }
        ContextMenuStrip = contextMenuStrip;

        _hardwareNames = new UserOption("sensorGadget.Hardwarenames", true, hardwareNamesItem, settings);
        _hardwareNames.Changed += delegate
        {
            Resize();
        };

        UserOption alwaysOnTop = new UserOption("sensorGadget.AlwaysOnTop", false, alwaysOnTopItem, settings);
        alwaysOnTop.Changed += delegate
        {
            AlwaysOnTop = alwaysOnTop.Value;
        };
        UserOption lockPositionAndSize = new UserOption("sensorGadget.LockPositionAndSize", false, lockItem, settings);
        lockPositionAndSize.Changed += delegate
        {
            LockPositionAndSize = lockPositionAndSize.Value;
        };

        HitTest += delegate (object sender, HitTestEventArgs e)
        {
            if (lockPositionAndSize.Value)
                return;

            if (e.Location.X < BorderWidth)
            {
                e.HitResult = HitResult.Left;
                return;
            }
            if (e.Location.X > Size.Width - 1 - BorderWidth)
            {
                e.HitResult = HitResult.Right;
            }
        };

        SizeChanged += delegate
        {
            settings.SetValue("sensorGadget.Width", Size.Width);
            Redraw();
        };

        VisibleChanged += delegate
        {
            Rectangle bounds = new Rectangle(Location, Size);
            Screen screen = Screen.FromRectangle(bounds);
            Rectangle intersection = Rectangle.Intersect(screen.WorkingArea, bounds);
            if (intersection.Width < Math.Min(16, bounds.Width) || intersection.Height < Math.Min(16, bounds.Height))
            {
                Location = new Point(screen.WorkingArea.Width / 2 - bounds.Width / 2, screen.WorkingArea.Height / 2 - bounds.Height / 2);
            }
        };

        MouseDoubleClick += delegate
        {
            SendHideShowCommand();
        };
    }

    public override void Dispose()
    {

        _largeFont.Dispose();
        _largeFont = null;

        _smallFont.Dispose();
        _smallFont = null;

        _captionFont.Dispose();
        _captionFont = null;

        _textBrush.Dispose();
        _textBrush = null;

        _backBrush.Dispose();
        _backBrush = null;

        _stringFormat.Dispose();
        _stringFormat = null;

        _trimStringFormat.Dispose();
        _trimStringFormat = null;

        _alignRightStringFormat.Dispose();
        _alignRightStringFormat = null;

        _borderPen.Dispose();
        _captionBrush.Dispose();

        base.Dispose();
    }

    private void HardwareRemoved(IHardware hardware)
    {
        hardware.SensorAdded -= SensorAdded;
        hardware.SensorRemoved -= SensorRemoved;

        foreach (ISensor sensor in hardware.Sensors)
            SensorRemoved(sensor);

        foreach (IHardware subHardware in hardware.SubHardware)
            HardwareRemoved(subHardware);
    }

    private void HardwareAdded(IHardware hardware)
    {
        foreach (ISensor sensor in hardware.Sensors)
            SensorAdded(sensor);

        hardware.SensorAdded += SensorAdded;
        hardware.SensorRemoved += SensorRemoved;

        foreach (IHardware subHardware in hardware.SubHardware)
            HardwareAdded(subHardware);
    }

    private void SensorAdded(ISensor sensor)
    {
        SensorStyle style = GetStyle(sensor);
        if (style.Value || style.Graph)
            Insert(sensor);
    }

    private void SensorRemoved(ISensor sensor)
    {
        Detach(sensor);
    }

    // true if the sensor value row is shown (the sensor can also be listed only because of its graph)
    public bool Contains(ISensor sensor)
    {
        return GetStyle(sensor).Value && IsListed(sensor);
    }

    private bool IsListed(ISensor sensor)
    {
        lock (_sensors)
        return _sensors.Values.Any(list => list.Any(x => x.Identifier.ToString() == sensor.Identifier.ToString()));
    }

    public bool Add(ISensor sensor)
    {
        SensorStyle style = GetStyle(sensor);
        if (style.Value)
            return false;

        style.Value = true;
        _settings.SetValue(new Identifier(sensor.Identifier, "gadget").ToString(), true);
        Insert(sensor);
        return true;
    }

    public void Remove(ISensor sensor)
    {
        SensorStyle style = GetStyle(sensor);
        style.Value = false;
        _settings.Remove(new Identifier(sensor.Identifier, "gadget").ToString());

        if (style.Graph)
            Resize();
        else
            Detach(sensor);
    }

    private void Insert(ISensor sensor)
    {
        if (!IsListed(sensor))
        {
            // get the right hardware
            IHardware hardware = sensor.Hardware;
            while (hardware.Parent != null)
                hardware = hardware.Parent;

            // get the sensor list associated with the hardware
            lock (_sensors)
            {
                if (!_sensors.TryGetValue(hardware, out IList<ISensor> list))
                {
                    list = new List<ISensor>();
                    _sensors.Add(hardware, list);
                }

                // insert the sensor at the right position
                int i = 0;
                while (i < list.Count && (list[i].SensorType < sensor.SensorType || list[i].SensorType == sensor.SensorType && list[i].Index < sensor.Index))
                    i++;

                list.Insert(i, sensor);
            }
        }

        Resize();
    }

    private void Detach(ISensor sensor)
    {
        lock (_sensors)
        foreach (KeyValuePair<IHardware, IList<ISensor>> keyValue in _sensors)
        {
            if (keyValue.Value.Remove(sensor))
            {
                if (keyValue.Value.Count == 0)
                {
                    _sensors.Remove(keyValue.Key);
                    break;
                }
            }
        }
        Resize();
    }

    public event EventHandler HideShowCommand;

    public void SendHideShowCommand()
    {
        HideShowCommand?.Invoke(this, null);
    }

    private Font CreateFont(float size, FontStyle style)
    {
        try
        {
            return new Font(SystemFonts.MessageBoxFont.FontFamily, size, style);
        }
        catch (ArgumentException)
        {
            // if the style is not supported, fall back to the original one
            return new Font(SystemFonts.MessageBoxFont.FontFamily, size,
                            SystemFonts.MessageBoxFont.Style);
        }
    }

    private void SetFontSize(float size)
    {
        _fontSize = size;
        _largeFont?.Dispose();
        _smallFont?.Dispose();
        _captionFont?.Dispose();
        _largeFont = CreateFont(_fontSize, FontStyle.Bold);
        _smallFont = CreateFont(_fontSize, FontStyle.Regular);
        _captionFont = CreateFont(Math.Max(1f, _fontSize * 0.8f), FontStyle.Regular);

        double scaledFontSize = _fontSize * _scale;
        _iconSize = (int)Math.Round(1.5 * scaledFontSize);
        _hardwareLineHeight = (int)Math.Round(1.66 * scaledFontSize);
        _sensorLineHeight = (int)Math.Round(1.33 * scaledFontSize);
        _leftMargin = BorderWidth + (int)Math.Round(0.3 * scaledFontSize);
        _rightMargin = BorderWidth + (int)Math.Round(0.3 * scaledFontSize);
        _topMargin = BorderWidth;
        _bottomMargin = BorderWidth + (int)Math.Round(0.3 * scaledFontSize);
        _progressWidth = (int)Math.Round(5.3 * scaledFontSize);

        Resize((int)Math.Round(17.3 * scaledFontSize));
    }

    private void SetFontColor(Color color)
    {
        _textBrush?.Dispose();
        _textBrush = new SolidBrush(color);
    }

    private void SetBackColor(Color color)
    {
        _backBrush?.Dispose();
        _backBrush = new SolidBrush(color);
    }

    private void Resize()
    {
        Resize(Size.Width);
    }

    private void Resize(int width)
    {
        int y = _topMargin;

        lock (_sensors)
        foreach (KeyValuePair<IHardware, IList<ISensor>> pair in _sensors)
        {
            if (_hardwareNames.Value)
            {
                if (y > _topMargin)
                    y += _hardwareLineHeight - _sensorLineHeight;
                y += _hardwareLineHeight;
            }
            y += pair.Value.Count(sensor => GetStyle(sensor).Value) * _sensorLineHeight;
            y += GetGraphTypes(pair.Value).Count * (GraphHeight + 2 * GraphPadding);
        }

        if (_sensors.Count == 0)
            y += 4 * _sensorLineHeight + _hardwareLineHeight;

        y += _bottomMargin;
        Size = new Size(width, y);
    }

    public static bool IsGraphical(ISensor sensor)
    {
        return sensor.SensorType is SensorType.Load or SensorType.Control or SensorType.Level or SensorType.Humidity;
    }

    private SensorStyle GetStyle(ISensor sensor)
    {
        string key = sensor.Identifier.ToString();
        lock (_styles)
        {
            if (!_styles.TryGetValue(key, out SensorStyle style))
            {
                string colorId = new Identifier(sensor.Identifier, "gadgetColor").ToString();
                string graphId = new Identifier(sensor.Identifier, "gadgetGraph").ToString();
                style = new SensorStyle
                {
                    Value = _settings.GetValue(new Identifier(sensor.Identifier, "gadget").ToString(), false),
                    Color = _settings.Contains(colorId) ? _settings.GetValue(colorId, Color.Black) : null,
                    Graph = _settings.GetValue(graphId, false)
                };
                _styles.Add(key, style);
            }
            return style;
        }
    }

    public Color? GetColor(ISensor sensor)
    {
        return GetStyle(sensor).Color;
    }

    public bool GetGraph(ISensor sensor)
    {
        return GetStyle(sensor).Graph;
    }

    public void SetColor(ISensor sensor, Color? color)
    {
        GetStyle(sensor).Color = color;
        string id = new Identifier(sensor.Identifier, "gadgetColor").ToString();
        if (color.HasValue)
            _settings.SetValue(id, color.Value);
        else
            _settings.Remove(id);
        Redraw();
    }

    public void SetGraph(ISensor sensor, bool graph)
    {
        GetStyle(sensor).Graph = graph;
        string id = new Identifier(sensor.Identifier, "gadgetGraph").ToString();
        if (graph)
        {
            _settings.SetValue(id, true);
            Insert(sensor);
        }
        else
        {
            _settings.Remove(id);
            if (GetStyle(sensor).Value)
                Resize();
            else
                Detach(sensor);
        }
    }

    private void DrawProgress(Graphics g, float x, float y, float width, float height, float progress, Color color)
    {
        // A NaN, infinite or huge value makes GDI+ throw an OverflowException.
        if (float.IsNaN(progress) || progress < 0)
            progress = 0;
        else if (progress > 1)
            progress = 1;

        g.DrawRectangle(_borderPen, x, y, width, height);

        using Brush brush = new SolidBrush(color);
        g.FillRectangle(brush, x, y, width * progress, height);
    }

    private static bool HasFixedScale(SensorType type)
    {
        return type is SensorType.Load or SensorType.Control or SensorType.Level or SensorType.Humidity;
    }

    private List<SensorType> GetGraphTypes(IList<ISensor> sensors)
    {
        return sensors.Where(s => GetStyle(s).Graph).Select(s => s.SensorType).Distinct().ToList();
    }

    // the graph of a type goes right after the last sensor of that type
    private int DrawTypeGraph(Graphics g, int w, int y, IList<ISensor> sensors, ISensor sensor)
    {
        if (!IsGraphEnd(sensors, sensor))
            return y;

        DrawGraph(g, _leftMargin, y + GraphPadding, w - _leftMargin - _rightMargin, GraphHeight, sensors, sensor.SensorType);
        return y + GraphHeight + 2 * GraphPadding;
    }

    private bool IsGraphEnd(IList<ISensor> sensors, ISensor sensor)
    {
        return sensors.Last(s => s.SensorType == sensor.SensorType) == sensor && sensors.Any(s => s.SensorType == sensor.SensorType && GetStyle(s).Graph);
    }

    private Color GetGraphColor(IList<ISensor> sensors, ISensor sensor)
    {
        SensorStyle style = GetStyle(sensor);
        if (style.Color.HasValue)
            return style.Color.Value;

        if (sensor.Name.Contains('#'))
        {
            // position among sensors of the same type keeps the color stable when other graphs are toggled
            int index = sensors.Where(s => s.SensorType == sensor.SensorType).ToList().IndexOf(sensor);
            return GetPaletteColor(Math.Max(index, 0));
        }

        return Color.Yellow;
    }

    private void SampleHistory(DateTime now)
    {
        if ((now - _lastSample).TotalMilliseconds < 400)
            return;
        _lastSample = now;

        lock (_history)
        lock (_sensors)
        {
            HashSet<string> active = new();
            foreach (IList<ISensor> list in _sensors.Values)
            {
                foreach (ISensor sensor in list)
                {
                    if (!GetStyle(sensor).Graph)
                        continue;

                    string key = sensor.Identifier.ToString();
                    active.Add(key);
                    if (!_history.TryGetValue(key, out List<HistoryPoint> points))
                    {
                        points = new List<HistoryPoint>();
                        _history.Add(key, points);
                    }

                    if (sensor.Value.HasValue && !float.IsNaN(sensor.Value.Value) && !float.IsInfinity(sensor.Value.Value))
                        points.Add(new HistoryPoint { Time = now, Value = sensor.Value.Value });

                    int stale = 0;
                    while (stale < points.Count && (now - points[stale].Time).TotalSeconds > GraphTimeWindowSeconds + 5)
                        stale++;
                    points.RemoveRange(0, stale);
                }
            }

            foreach (string key in _history.Keys.Where(k => !active.Contains(k)).ToList())
                _history.Remove(key);
        }
    }

    private void DrawGraph(Graphics g, float x, float y, float width, float height, IList<ISensor> sensors, SensorType type)
    {
        DateTime now = DateTime.UtcNow;
        List<ISensor> series = sensors.Where(s => s.SensorType == type && GetStyle(s).Graph).ToList();

        using (Brush back = new SolidBrush(Color.FromArgb(8, 10, 12)))
            g.FillRectangle(back, x, y, width, height);

        List<(Color Color, List<HistoryPoint> Points)> lines = new();
        lock (_history)
        {
            foreach (ISensor sensor in series)
            {
                if (_history.TryGetValue(sensor.Identifier.ToString(), out List<HistoryPoint> points) && points.Count > 1)
                    lines.Add((GetGraphColor(sensors, sensor), points.ToList()));
            }
        }

        float dataMin = 0, dataMax = 100;
        bool fixedScale = HasFixedScale(type);
        if (!fixedScale && lines.Count > 0)
        {
            dataMin = lines.Min(l => l.Points.Min(p => p.Value));
            dataMax = lines.Max(l => l.Points.Max(p => p.Value));
        }

        float scaleMin = dataMin, scaleMax = dataMax;
        if (!fixedScale)
        {
            float margin = Math.Max(dataMax - dataMin, Math.Max(1f, Math.Abs(dataMax) * 0.1f)) * 0.1f;
            scaleMin = dataMin - margin;
            scaleMax = dataMax + margin;
            if (dataMin >= 0 && scaleMin < 0)
                scaleMin = 0;
        }

        System.Drawing.Drawing2D.GraphicsState state = g.Save();
        try
        {
            g.SetClip(new RectangleF(x, y, width, height), System.Drawing.Drawing2D.CombineMode.Intersect);

            for (int i = 1; i < 4; i++)
                g.DrawLine(_borderPen, x, y + height * i / 4, x + width, y + height * i / 4);

            for (int i = 1; i < 10; i++)
            {
                float gx = x + width * i / 10;
                g.DrawLine(_borderPen, gx, y, gx, y + height);
            }

            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            float range = scaleMax - scaleMin;
            foreach (var line in lines)
            {
                PointF[] points = line.Points.Select(p => new PointF(
                    x + width * (float)(1 - (now - p.Time).TotalSeconds / GraphTimeWindowSeconds),
                    y + (height - 1) * (1 - (Math.Max(scaleMin, Math.Min(scaleMax, p.Value)) - scaleMin) / range))).ToArray();
                using Pen pen = new Pen(line.Color, 1f);
                g.DrawLines(pen, points);
            }

            if (!fixedScale && lines.Count > 0)
            {
                bool fahrenheit = type == SensorType.Temperature && UnitManager.IsFahrenheitUsed;
                float lo = fahrenheit ? dataMin * 1.8f + 32 : dataMin;
                float hi = fahrenheit ? dataMax * 1.8f + 32 : dataMax;
                g.DrawString($"{type}  {lo:0.#}–{hi:0.#}", _captionFont, _captionBrush, x + 2, y);
            }
        }
        finally
        {
            g.Restore(state);
        }

        g.DrawRectangle(_borderPen, x, y, width - 1, height - 1);
    }

    private class SensorStyle
    {
        public Color? Color;
        public bool Value;
        public bool Graph;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        try
        {
            Graphics g = e.Graphics;
            int w = Size.Width;

            g.Clear(Color.Transparent);
            g.FillRectangle(_backBrush, 0, 0, Size.Width, Size.Height);

            SampleHistory(DateTime.UtcNow);

            int x;
            int y = _topMargin;

            if (_sensors.Count == 0)
            {
                x = BorderWidth + 1;
                g.DrawString("Right-click on a sensor in the main window and select " +
                             "\"Show in Gadget\" to show the sensor here.",
                             _smallFont, _textBrush,
                             new Rectangle(x, y - 1, w - BorderWidth - x, 0));
            }

            lock (_sensors)
            foreach (KeyValuePair<IHardware, IList<ISensor>> pair in _sensors)
            {
                if (_hardwareNames.Value)
                {
                    if (y > _topMargin)
                        y += _hardwareLineHeight - _sensorLineHeight;

                    x = BorderWidth + 1;
                    g.DrawImage(HardwareTypeImage.Instance.GetImage(pair.Key.HardwareType), new Rectangle(x, y + 1, _iconSize, _iconSize));
                    x += _iconSize + 1;
                    g.DrawString(pair.Key.Name, _largeFont, _textBrush, new Rectangle(x, y - 1, w - BorderWidth - x, 0), _stringFormat);
                    y += _hardwareLineHeight;
                }

                foreach (ISensor sensor in pair.Value.ToList())
                {
                    if (!GetStyle(sensor).Value)
                    {
                        y = DrawTypeGraph(g, w, y, pair.Value, sensor);
                        continue;
                    }

                    int remainingWidth;

                    if (sensor.SensorType != SensorType.Load &&
                         sensor.SensorType != SensorType.Control &&
                         sensor.SensorType != SensorType.Level &&
                         sensor.SensorType != SensorType.Humidity || !sensor.Value.HasValue)
                    {
                        string formatted;

                        if (sensor.SensorType == SensorType.Text)
                        {
                            formatted = sensor.Text ?? "-";
                        }
                        else if (sensor.Value.HasValue)
                        {
                            string format = sensor.SensorType switch
                            {
                                SensorType.Voltage => "{0:0.###} V",
                                SensorType.Current => "{0:0.###} A",
                                SensorType.Clock => "{0:F0} MHz",
                                SensorType.Frequency => "{0:F0} Hz",
                                SensorType.Temperature => "{0:0.#} °C",
                                SensorType.Fan => "{0:F0} RPM",
                                SensorType.Flow => "{0:F0} L/h",
                                SensorType.Power => "{0:0.#} W",
                                SensorType.Data => "{0:0.#} GB",
                                SensorType.SmallData => "{0:F0} MB",
                                SensorType.Factor => "{0:0.###}",
                                SensorType.IntFactor => "{0:F0}",
                                SensorType.TimeSpan => "{0:g}",
                                SensorType.Timing => "{0:0.###} ns",
                                SensorType.Energy => "{0:F0} mWh",
                                SensorType.Noise => "{0:F0} dBA",
                                SensorType.Conductivity => "{0:0.#} µS/cm",
                                _ => ""
                            };

                            if (sensor.SensorType == SensorType.Temperature && UnitManager.IsFahrenheitUsed)
                            {
                                formatted = $"{UnitManager.CelsiusToFahrenheit(sensor.Value):0.#} °F";
                            }
                            else if (sensor.SensorType == SensorType.Throughput)
                            {
                                string result;
                                switch (sensor.Name)
                                {
                                    case "Connection Speed":
                                        {
                                            switch (sensor.Value)
                                            {
                                                case 100000000:
                                                    result = "100Mbps";
                                                    break;
                                                case 1000000000:
                                                    result = "1Gbps";
                                                    break;
                                                default:
                                                    {
                                                        if (sensor.Value < 1024)
                                                            result = $"{sensor.Value:F0} bps";
                                                        else if (sensor.Value < 1048576)
                                                            result = $"{sensor.Value / 1024:0.#} Kbps";
                                                        else if (sensor.Value < 1073741824)
                                                            result = $"{sensor.Value / 1048576:0.#} Mbps";
                                                        else
                                                            result = $"{sensor.Value / 1073741824:0.#} Gbps";
                                                    }
                                                    break;
                                            }
                                        }
                                        break;
                                    default:
                                        {
                                            if (sensor.Value < 1048576)
                                                result = $"{sensor.Value / 1024:0.#} KB/s";
                                            else
                                                result = $"{sensor.Value / 1048576:0.#} MB/s";
                                        }
                                        break;
                                }
                                formatted = result;
                            }
                            else if (sensor.SensorType == SensorType.TimeSpan)
                            {
                                formatted = string.Format(format, TimeSpan.FromSeconds(sensor.Value.Value));
                            }
                            else
                            {
                                formatted = string.Format(format, sensor.Value);
                            }
                        }
                        else
                        {
                            formatted = "-";
                        }

                        g.DrawString(formatted, _smallFont, _textBrush, new RectangleF(-1, y - 1, w - _rightMargin + 3, 0), _alignRightStringFormat);

                        remainingWidth = w - (int)Math.Floor(g.MeasureString(formatted, _smallFont, w, StringFormat.GenericTypographic).Width) - _rightMargin;
                    }
                    else
                    {
                        float barX = w - _progressWidth - _rightMargin;
                        float barY = y + 0.35f * _sensorLineHeight;
                        float barHeight = 0.6f * _sensorLineHeight;
                        DrawProgress(g, barX, barY, _progressWidth, barHeight, 0.01f * sensor.Value.Value, GetGraphColor(pair.Value, sensor));
                        remainingWidth = w - _progressWidth - _rightMargin;
                    }

                    remainingWidth -= _leftMargin + 2;
                    if (remainingWidth > 0)
                    {
                        g.DrawString(sensor.Name, _smallFont, _textBrush, new RectangleF(_leftMargin - 1, y - 1, remainingWidth, 0), _trimStringFormat);
                    }
                    y += _sensorLineHeight;

                    y = DrawTypeGraph(g, w, y, pair.Value, sensor);
                }
            }
        }
        catch (ArgumentException)
        {
            // #1425.
        }
    }

    private class HardwareComparer : IComparer<IHardware>
    {
        public int Compare(IHardware x, IHardware y)
        {
            switch (x)
            {
                case null when y == null:
                    return 0;
                case null:
                    return -1;
            }

            if (y == null)
                return 1;

            if (x.HardwareType != y.HardwareType)
                return x.HardwareType.CompareTo(y.HardwareType);

            return x.Identifier.CompareTo(y.Identifier);
        }
    }
}
