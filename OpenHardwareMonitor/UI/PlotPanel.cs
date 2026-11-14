using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using OpenHardwareMonitor.Hardware;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.WindowsForms;
using OxyPlot.Series;
using sergiye.Common;

namespace OpenHardwareMonitor.UI;

internal class PlotPanel : UserControl
{
    private readonly PersistentSettings _settings;
        private readonly PlotView _plot;
    private readonly PlotModel _model;
    private readonly DateTimeAxis _timeAxis = new DateTimeAxis();
    private readonly SortedDictionary<SensorType, LinearAxis> _axes = new SortedDictionary<SensorType, LinearAxis>();
    private readonly Dictionary<SensorType, LineAnnotation> _annotations = new Dictionary<SensorType, LineAnnotation>();
    private UserOption _stackedAxes;
    private UserOption _showAxesLabels;
    private UserOption _timeAxisEnableZoom;
    private UserOption _yAxesEnableZoom;
    private UserOption _yAxesAutoscale;
    private UserOption _timeAxisAutoscale;
    private readonly Dictionary<ISensor, List<SensorValue>> _history = new Dictionary<ISensor, List<SensorValue>>();
    private float _dpiX;
    private float _dpiY;
    private double _dpiXScale = 1;
    private double _dpiYScale = 1;
    private Point _rightClickEnter;
    private bool _cancelContextMenu = false;

    public PlotPanel(PersistentSettings settings)
    {
        _settings = settings;

        SetDpi();
        _model = CreatePlotModel();

        _plot = new PlotView { Dock = DockStyle.Fill, Model = _model, ContextMenuStrip = CreateMenu() };
        _plot.MouseDown += (sender, e) =>
        {
            if (e.Button == MouseButtons.Right)
            {
                _rightClickEnter = e.Location;
            }
        };
        _plot.MouseMove += (sender, e) =>
        {
            if (e.Button == MouseButtons.Right)
            {
                if (!_cancelContextMenu && e.Location.DistanceTo(_rightClickEnter) > 10.0f)
                {
                    _cancelContextMenu = true;
                    OnUserNavigated(true);
                }
            }
        };
        _plot.MouseWheel += (sender, e) => OnUserNavigated(false);

        UpdateAxesPosition();

        SuspendLayout();
        Controls.Add(_plot);
        ResumeLayout(true);
        _plot.ShowTracker(new TrackerHitResult());
        _plot.HideTracker();
        foreach (Control plotControl in _plot.Controls)
        {
            plotControl.BackColor = Theme.Current.BackgroundColor;
            plotControl.ForeColor = Theme.Current.ForegroundColor;
        }
        ApplyTheme();
        Theme.OnCurrentChanged += ApplyTheme;
        Disposed += (_, _) => Theme.OnCurrentChanged -= ApplyTheme;
    }

    public static readonly Color[] LightPalette =
    {
        Color.FromArgb(0xD3, 0x2F, 0x2F), Color.FromArgb(0x19, 0x76, 0xD2), Color.FromArgb(0x38, 0x8E, 0x3C), Color.FromArgb(0xF5, 0x7C, 0x00),
        Color.FromArgb(0x7B, 0x1F, 0xA2), Color.FromArgb(0x00, 0x97, 0xA7), Color.FromArgb(0xC2, 0x18, 0x5B), Color.FromArgb(0x5D, 0x40, 0x37),
        Color.FromArgb(0x68, 0x9F, 0x38), Color.FromArgb(0x30, 0x3F, 0x9F), Color.FromArgb(0xAF, 0xB4, 0x2B), Color.FromArgb(0x45, 0x5A, 0x64)
    };

    public static readonly Color[] DarkPalette =
    {
        Color.FromArgb(0xF0, 0x71, 0x78), Color.FromArgb(0x82, 0xAA, 0xFF), Color.FromArgb(0xC3, 0xE8, 0x8D), Color.FromArgb(0xFF, 0xCB, 0x6B),
        Color.FromArgb(0x00, 0x96, 0x88), Color.FromArgb(0x89, 0xDD, 0xF3), Color.FromArgb(0xFF, 0xE0, 0x82), Color.FromArgb(0x79, 0x86, 0xCB),
        Color.FromArgb(0xC7, 0x92, 0xEA), Color.FromArgb(0xFF, 0x8A, 0x65), Color.FromArgb(0x4D, 0xD0, 0xE1), Color.FromArgb(0xF0, 0x62, 0x92)
    };

    public static Color[] Palette => Theme.Current.IsDark ? DarkPalette : LightPalette;

    private static Color GridMajorColor => Blend(Theme.Current.BackgroundColor, Theme.Current.ForegroundColor, 0.35);

    private static Color GridMinorColor => Blend(Theme.Current.BackgroundColor, Theme.Current.ForegroundColor, 0.15);

    private static Color Blend(Color from, Color to, double ratio)
    {
        return Color.FromArgb((int)(from.R + (to.R - from.R) * ratio),
                              (int)(from.G + (to.G - from.G) * ratio),
                              (int)(from.B + (to.B - from.B) * ratio));
    }

    public void ApplyTheme()
    {
        _model.Background = Theme.Current.BackgroundColor.ToOxyColor();
        _model.PlotAreaBorderColor = Theme.Current.ForegroundColor.ToOxyColor();
        foreach (Axis axis in _model.Axes)
        {
            axis.AxislineColor = Theme.Current.ForegroundColor.ToOxyColor();
            axis.MajorGridlineColor = GridMajorColor.ToOxyColor();
            axis.MinorGridlineColor = GridMinorColor.ToOxyColor();
            axis.TextColor = Theme.Current.ForegroundColor.ToOxyColor();
            axis.TitleColor = Theme.Current.ForegroundColor.ToOxyColor();
            axis.MinorTicklineColor = Theme.Current.ForegroundColor.ToOxyColor();
            axis.TicklineColor = Theme.Current.ForegroundColor.ToOxyColor();
        }
        foreach (LineAnnotation annotation in _model.Annotations.Select(x => x as LineAnnotation).Where(x => x != null))
        {
            annotation.Color = Theme.Current.ForegroundColor.ToOxyColor();
        }
    }

    public void SetCurrentSettings()
    {
        foreach (LinearAxis axis in _axes.Values)
        {
            _settings.SetValue("plotPanel.Min" + axis.Key, (float)axis.ActualMinimum);
            _settings.SetValue("plotPanel.Max" + axis.Key, (float)axis.ActualMaximum);
        }
    }

    private ContextMenuStrip CreateMenu()
    {
        ContextMenuStrip menu = new ContextMenuStrip();
        menu.Renderer = new ThemedToolStripRenderer();
        menu.Opening += (sender, e) =>
        {
            if (_cancelContextMenu)
            {
                e.Cancel = true;
                _cancelContextMenu = false;
            }
        };

        ToolStripMenuItem stackedAxesMenuItem = new ToolStripMenuItem("Stacked Axes");
        _stackedAxes = new UserOption("stackedAxes", true, stackedAxesMenuItem, _settings);
        _stackedAxes.Changed += (sender, e) =>
        {
            UpdateAxesPosition();
            RefreshPlot();
        };
        menu.Items.Add(stackedAxesMenuItem);

        ToolStripMenuItem showAxesLabelsMenuItem = new ToolStripMenuItem("Show Axes Labels");
        _showAxesLabels = new UserOption("showAxesLabels", true, showAxesLabelsMenuItem, _settings);
        _showAxesLabels.Changed += (sender, e) =>
        {
            if (_showAxesLabels.Value)
                _model.PlotMargins = new OxyThickness(double.NaN);
            else
                _model.PlotMargins = new OxyThickness(0);
        };
        menu.Items.Add(showAxesLabelsMenuItem);

        ToolStripMenuItem timeAxisMenuItem = new ToolStripMenuItem("Time Axis");
        ToolStripMenuItem[] timeAxisMenuItems =
        { new ToolStripMenuItem("Enable Zoom"),
            new ToolStripMenuItem("Autoscale") };

        foreach (ToolStripItem mi in timeAxisMenuItems)
            timeAxisMenuItem.DropDownItems.Add(mi);
        menu.Items.Add(timeAxisMenuItem);

        _timeAxisEnableZoom = new UserOption("timeAxisEnableZoom", true, timeAxisMenuItems[0], _settings);
        _timeAxisEnableZoom.Changed += (sender, e) =>
        {
            _timeAxis.IsZoomEnabled = _timeAxisEnableZoom.Value;
        };

        _timeAxisAutoscale = new UserOption("timeAxisAutoscale", true, timeAxisMenuItems[1], _settings);
        _timeAxisAutoscale.Changed += (sender, e) =>
        {
            if (_timeAxisAutoscale.Value)
                AutoscaleTimeAxis();
        };

        ToolStripMenuItem yAxesMenuItem = new ToolStripMenuItem("Value Axes");
        ToolStripMenuItem[] yAxesMenuItems =
        { new ToolStripMenuItem("Enable Zoom"),
            new ToolStripMenuItem("Autoscale"),
            new ToolStripMenuItem("Autoscale All", null, (s, e) => { AutoscaleAllYAxes(); }) };

        foreach (ToolStripItem mi in yAxesMenuItems)
            yAxesMenuItem.DropDownItems.Add(mi);
        menu.Items.Add(yAxesMenuItem);

        _yAxesEnableZoom = new UserOption("yAxesEnableZoom", true, yAxesMenuItems[0], _settings);
        _yAxesEnableZoom.Changed += (sender, e) =>
        {
            foreach (LinearAxis axis in _axes.Values)
                axis.IsZoomEnabled = _yAxesEnableZoom.Value;
        };

        _yAxesAutoscale = new UserOption("yAxesAutoscale", false, yAxesMenuItems[1], _settings);
        _yAxesAutoscale.Changed += (sender, e) =>
        {
            if (_yAxesAutoscale.Value)
                AutoscaleAllYAxes();
        };

        return menu;
    }

    private PlotModel CreatePlotModel()
    {
        _timeAxis.Position = AxisPosition.Bottom;
        _timeAxis.MajorGridlineStyle = LineStyle.Solid;
        _timeAxis.MajorGridlineThickness = 1;
        _timeAxis.MajorGridlineColor = GridMajorColor.ToOxyColor();
        _timeAxis.MinorGridlineStyle = LineStyle.Solid;
        _timeAxis.MinorGridlineThickness = 1;
        _timeAxis.MinorGridlineColor = GridMinorColor.ToOxyColor();
        _timeAxis.IntervalLength = 100 * _dpiXScale;
        _timeAxis.MinimumPadding = 0;
        _timeAxis.MaximumPadding = 0;

        var units = new Dictionary<SensorType, string>
        {
            { SensorType.Voltage, "V" },
            { SensorType.Current, "A" },
            { SensorType.Clock, "MHz" },
            { SensorType.Temperature, "°C" },
            { SensorType.Load, "%" },
            { SensorType.Fan, "RPM" },
            { SensorType.Flow, "L/h" },
            { SensorType.Control, "%" },
            { SensorType.Level, "%" },
            { SensorType.Factor, "1" },
            { SensorType.Power, "W" },
            { SensorType.Data, "GB" },
            { SensorType.Frequency, "Hz" },
            { SensorType.Energy, "mWh" },
            { SensorType.Noise, "dBA" },
            { SensorType.Conductivity, "µS/cm" },
            { SensorType.Humidity, "%" },
            { SensorType.SmallData, "MB" },
            { SensorType.Throughput, "B/s" },
            { SensorType.Timing, "ns" },
            { SensorType.TimeSpan, "s" }
        };

        foreach (SensorType type in Enum.GetValues(typeof(SensorType)))
        {
            if (type == SensorType.Text)
                continue;

            string typeName = type.ToString();
            var axis = new LinearAxis
            {
                Position = AxisPosition.Left,
                MajorGridlineStyle = LineStyle.Solid,
                MajorGridlineThickness = 1,
                MajorGridlineColor = _timeAxis.MajorGridlineColor,
                MinorGridlineStyle = LineStyle.Solid,
                MinorGridlineThickness = 1,
                MinorGridlineColor = _timeAxis.MinorGridlineColor,
                AxislineStyle = LineStyle.Solid,
                Title = typeName,
                Key = typeName,
            };

            var annotation = new LineAnnotation
            {
                Type = LineAnnotationType.Horizontal,
                ClipByXAxis = false,
                ClipByYAxis = false,
                LineStyle = LineStyle.Solid,
                Color = Theme.Current.ForegroundColor.ToOxyColor(),
                YAxisKey = typeName,
                StrokeThickness = 2,
            };

#pragma warning disable CS0618 //obsolete warning

            axis.AxisChanged += (sender, args) => annotation.Y = axis.ActualMinimum;
            axis.TransformChanged += (sender, args) => annotation.Y = axis.ActualMinimum;

#pragma warning restore CS0618 //obsolete warning

            float min = _settings.GetValue("plotPanel.Min" + axis.Key, float.NaN);
            float max = _settings.GetValue("plotPanel.Max" + axis.Key, float.NaN);

            // Skip unset and broken ranges (e.g. Infinity saved after zooming out too far), as OxyPlot can't draw them.
            if (min < max && !float.IsInfinity(min) && !float.IsInfinity(max))
                axis.Zoom(min, max);

            if (units.ContainsKey(type))
                axis.Unit = units[type];

            _axes.Add(type, axis);
            _annotations.Add(type, annotation);
        }

        var model = new ScaledPlotModel(_dpiXScale, _dpiYScale);
        model.Axes.Add(_timeAxis);
        foreach (LinearAxis axis in _axes.Values)
            model.Axes.Add(axis);
        model.IsLegendVisible = false;

        return model;
    }

    private void SetDpi()
    {
        // https://msdn.microsoft.com/en-us/library/windows/desktop/dn469266(v=vs.85).aspx
        const int defaultDpi = 96;
        Graphics g = CreateGraphics();

        try
        {
            _dpiX = g.DpiX;
            _dpiY = g.DpiY;
        }
        finally
        {
            g.Dispose();
        }

        if (_dpiX > 0)
            _dpiXScale = _dpiX / defaultDpi;
        if (_dpiY > 0)
            _dpiYScale = _dpiY / defaultDpi;
    }

    public void SetSensors(List<ISensor> sensors, IDictionary<ISensor, Color> colors, double strokeThickness)
    {
        List<ISensor> plotted = sensors.Where(x => _axes.ContainsKey(x.SensorType)).ToList();
        foreach (ISensor removed in _history.Keys.Where(x => !plotted.Contains(x)).ToList())
            _history.Remove(removed);

        // A newly plotted sensor has no points of its own yet, so it starts from the history the sensor has collected.
        foreach (ISensor sensor in plotted.Where(x => !_history.ContainsKey(x)))
            _history[sensor] = sensor.Values.Select(x => new SensorValue(x.Value, x.Time.ToLocalTime())).ToList();

        _model.Series.Clear();
        var types = new HashSet<SensorType>();


        foreach (ISensor sensor in plotted)
        {
            var series = new LineSeries
            {
                ItemsSource = GetPoints(sensor),
                Color = colors[sensor].ToOxyColor(),
                StrokeThickness = strokeThickness,
                YAxisKey = _axes[sensor.SensorType].Key,
                Title = sensor.Hardware.Name + " " + sensor.Name,
                TrackerFormatString = "{0}\n{2:HH:mm:ss}\n{3}: {4:0.##}"
            };

            _model.Series.Add(series);

            types.Add(sensor.SensorType);
        }

        foreach (KeyValuePair<SensorType, LinearAxis> pair in _axes.Reverse())
        {
            LinearAxis axis = pair.Value;
            SensorType type = pair.Key;
            axis.IsAxisVisible = types.Contains(type);
        }

        UpdateAxesPosition();
        RefreshPlot();
    }

    private IEnumerable<DataPoint> GetPoints(ISensor sensor)
    {
        if (!_history.TryGetValue(sensor, out List<SensorValue> points))
            yield break;

        foreach (SensorValue value in points)
            yield return CreatePoint(sensor.SensorType, value.Time, value.Value);
    }

    private static DataPoint CreatePoint(SensorType type, DateTime time, float value)
    {
        float displayedValue = type == SensorType.Temperature && UnitManager.IsFahrenheitUsed
            ? UnitManager.CelsiusToFahrenheit(value).Value
            : value;

        return new DataPoint(DateTimeAxis.ToDouble(time), displayedValue);
    }

    public void UpdateStrokeThickness(double strokeThickness)
    {
        foreach (LineSeries series in _model.Series)
        {
            series.StrokeThickness = strokeThickness;
        }
        RefreshPlot();
    }

    private void UpdateAxesPosition()
    {
        if (_stackedAxes.Value)
        {
            int count = _axes.Values.Count(axis => axis.IsAxisVisible);
            double start = 0.0;
            foreach (KeyValuePair<SensorType, LinearAxis> pair in _axes.Reverse())
            {
                LinearAxis axis = pair.Value;
                axis.StartPosition = start;
                double delta = axis.IsAxisVisible ? 1.0 / count : 0;
                start += delta;
                axis.EndPosition = start;
                axis.PositionTier = 0;
                axis.MajorGridlineStyle = LineStyle.Solid;
                axis.MinorGridlineStyle = LineStyle.Solid;
                LineAnnotation annotation = _annotations[pair.Key];
                annotation.Y = axis.ActualMinimum;
                if (!_model.Annotations.Contains(annotation)) 
                    _model.Annotations.Add(annotation);
            }
        }
        else
        {
            int tier = 0;

            foreach (KeyValuePair<SensorType, LinearAxis> pair in _axes.Reverse())
            {
                LinearAxis axis = pair.Value;

                if (axis.IsAxisVisible)
                {
                    axis.StartPosition = 0;
                    axis.EndPosition = 1;
                    axis.PositionTier = tier;
                    tier++;
                }
                else
                {
                    axis.StartPosition = 0;
                    axis.EndPosition = 0;
                    axis.PositionTier = 0;
                }
                axis.MajorGridlineStyle = LineStyle.None;
                axis.MinorGridlineStyle = LineStyle.None;
                LineAnnotation annotation = _annotations[pair.Key];
                if (_model.Annotations.Contains(annotation)) 
                    _model.Annotations.Remove(_annotations[pair.Key]);
            }
        }
    }

    // Called on every sensor update: records a point of its own for each plotted sensor, which is
    // denser than the history kept by the sensor itself (it averages several updates into one point).
    public void InvalidatePlot()
    {
        DateTime now = DateTime.Now;

        foreach (KeyValuePair<ISensor, List<SensorValue>> pair in _history)
        {
            ISensor sensor = pair.Key;
            List<SensorValue> points = pair.Value;
            TimeSpan window = sensor.ValuesTimeWindow;

            int stale = 0;
            while (stale < points.Count && now - points[stale].Time > window)
                stale++;
            if (stale > 0)
                points.RemoveRange(0, stale);

            if (sensor.Value.HasValue)
                points.Add(new SensorValue(sensor.Value.Value, now));
        }

        RefreshPlot();
    }

    public void ClearHistory()
    {
        foreach (List<SensorValue> points in _history.Values)
            points.Clear();

        RefreshPlot();
    }

    private void RefreshPlot()
    {
        if (_timeAxisAutoscale != null && _timeAxisAutoscale.Value)
            AutoscaleTimeAxis();

        if (_yAxesAutoscale != null && _yAxesAutoscale.Value)
            AutoscaleAllYAxes();

        if (_axes != null)
        {
            foreach (KeyValuePair<SensorType, LinearAxis> pair in _axes)
            {
                LinearAxis axis = pair.Value;
                SensorType type = pair.Key;
                if (type == SensorType.Temperature)
                    axis.Unit = UnitManager.IsFahrenheitUsed ? "°F" : "°C";
                    
                if (!_stackedAxes.Value) 
                    continue;

                var annotation = _annotations[pair.Key];
                annotation.Y = axis.ActualMaximum;
            }
        }

        _plot?.InvalidatePlot(true);
    }

    // Only a real mouse pan or zoom switches the automatic modes off: a slight movement of the mouse
    // while right-clicking to open the menu also shifts the axes, but is reverted on the next update.
    private void OnUserNavigated(bool pan)
    {
        if (_yAxesAutoscale.Value)
            _yAxesAutoscale.Value = false;

        if (_timeAxisAutoscale.Value && (pan || _timeAxisEnableZoom.Value))
            _timeAxisAutoscale.Value = false;
    }

    // The right edge is the current time rather than the last point, so the plot scrolls smoothly.
    private void AutoscaleTimeAxis()
    {
        DateTime? first = null;
        foreach (List<SensorValue> points in _history.Values)
        {
            if (points.Count > 0 && (first == null || points[0].Time < first))
                first = points[0].Time;
        }

        DateTime now = DateTime.Now;
        if (first == null || first.Value >= now)
        {
            _timeAxis.Reset();
            return;
        }

        bool isZoomEnabled = _timeAxis.IsZoomEnabled;
        try
        {
            _timeAxis.IsZoomEnabled = true;
            _timeAxis.Zoom(DateTimeAxis.ToDouble(first.Value), DateTimeAxis.ToDouble(now));
        }
        finally
        {
            _timeAxis.IsZoomEnabled = isZoomEnabled;
        }
    }

    public void AutoscaleAllYAxes()
    {
        foreach (LinearAxis axis in _axes.Values)
            axis.Reset();
    }
}
