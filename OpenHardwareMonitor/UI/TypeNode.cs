using OpenHardwareMonitor.Hardware;
using OpenHardwareMonitor.Utilities;

namespace OpenHardwareMonitor.UI;

public sealed class TypeNode : Node, IExpandPersistNode
{
    private readonly PersistentSettings _settings;
    private readonly string _expandedIdentifier;
    private bool _expanded;

    public TypeNode(SensorType sensorType, Identifier parentId, PersistentSettings settings)
    {
        SensorType = sensorType;
        _expandedIdentifier = new Identifier(parentId, SensorType.ToString(), ".expanded").ToString();
        _settings = settings;

        switch (sensorType)
        {
            case SensorType.Voltage:
                Image = EmbeddedResources.GetImage("voltage.png");
                Text = "Voltages";
                break;
            case SensorType.Current:
                Image = EmbeddedResources.GetImage("voltage.png");
                Text = "Currents";
                break;
            case SensorType.Energy:
                Image = EmbeddedResources.GetImage("battery.png");
                Text = "Capacities";
                break;
            case SensorType.Clock:
                Image = EmbeddedResources.GetImage("clock.png");
                Text = "Clocks";
                break;
            case SensorType.Load:
                Image = EmbeddedResources.GetImage("load.png");
                Text = "Load";
                break;
            case SensorType.Temperature:
                Image = EmbeddedResources.GetImage("temperature.png");
                Text = "Temperatures";
                break;
            case SensorType.Fan:
                Image = EmbeddedResources.GetImage("fan.png");
                Text = "Fans";
                break;
            case SensorType.Flow:
                Image = EmbeddedResources.GetImage("flow.png");
                Text = "Flows";
                break;
            case SensorType.Control:
                Image = EmbeddedResources.GetImage("control.png");
                Text = "Controls";
                break;
            case SensorType.Level:
                Image = EmbeddedResources.GetImage("level.png");
                Text = "Levels";
                break;
            case SensorType.Power:
                Image = EmbeddedResources.GetImage("power.png");
                Text = "Powers";
                break;
            case SensorType.Data:
                Image = EmbeddedResources.GetImage("data.png");
                Text = "Data";
                break;
            case SensorType.SmallData:
                Image = EmbeddedResources.GetImage("data.png");
                Text = "Data";
                break;
            case SensorType.Factor:
            case SensorType.IntFactor:
                Image = EmbeddedResources.GetImage("factor.png");
                Text = "Factors";
                break;
            case SensorType.Frequency:
                Image = EmbeddedResources.GetImage("clock.png");
                Text = "Frequencies";
                break;
            case SensorType.Throughput:
                Image = EmbeddedResources.GetImage("throughput.png");
                Text = "Throughput";
                break;
            case SensorType.TimeSpan:
                Image = EmbeddedResources.GetImage("time.png");
                Text = "Times";
                break;
            case SensorType.Timing:
                Image = EmbeddedResources.GetImage("time.png");
                Text = "Timings";
                break;
            case SensorType.Noise:
                Image = EmbeddedResources.GetImage("loudspeaker.png");
                Text = "Noise Levels";
                break;
            case SensorType.Conductivity:
                Image = EmbeddedResources.GetImage("voltage.png");
                Text = "Conductivities";
                break;
            case SensorType.Humidity:
                Image = EmbeddedResources.GetImage("humidity.png");
                Text = "Humidity Levels";
                break;
        }

        NodeAdded += TypeNode_NodeAdded;
        NodeRemoved += TypeNode_NodeRemoved;
        _expanded = settings.GetValue(_expandedIdentifier, true);
    }

    private void TypeNode_NodeRemoved(Node node)
    {
        node.IsVisibleChanged -= Node_IsVisibleChanged;
        Node_IsVisibleChanged(null);
    }

    private void TypeNode_NodeAdded(Node node)
    {
        node.IsVisibleChanged += Node_IsVisibleChanged;
        Node_IsVisibleChanged(null);
    }

    private void Node_IsVisibleChanged(Node node)
    {
        foreach (Node n in Nodes)
        {
            if (n.IsVisible)
            {
                IsVisible = true;
                return;
            }
        }
        IsVisible = false;
    }

    public SensorType SensorType { get; }

    public bool Expanded
    {
        get => _expanded;
        set
        {
            if (_expanded == value) return;
            _expanded = value;
            _settings.SetValue(_expandedIdentifier, _expanded);
        }
    }
}
