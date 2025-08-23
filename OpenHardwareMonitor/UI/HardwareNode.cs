using OpenHardwareMonitor.Hardware;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenHardwareMonitor.UI;

public class HardwareNode : Node, IExpandPersistNode
{
    private readonly PersistentSettings _settings;
    private readonly List<TypeNode> _typeNodes = new List<TypeNode>();
    private readonly string _expandedIdentifier;
    private bool _expanded;

    public HardwareNode(IHardware hardware, PersistentSettings settings)
    {
        _settings = settings;
        _expandedIdentifier = new Identifier(hardware.Identifier, "expanded").ToString();
        Hardware = hardware;
        Image = HardwareTypeImage.Instance.GetImage(hardware.HardwareType);

        foreach (SensorType sensorType in Enum.GetValues(typeof(SensorType)))
            _typeNodes.Add(new TypeNode(sensorType, hardware.Identifier, _settings));

        foreach (ISensor sensor in hardware.Sensors)
            SensorAdded(sensor);

        hardware.SensorAdded += SensorAdded;
        hardware.SensorRemoved += SensorRemoved;

        _expanded = settings.GetValue(_expandedIdentifier, true);
    }


    public override string Text
    {
        get { return Hardware.Name; }
        set { Hardware.Name = value; }
    }

    public override string ToolTip
    {
        get
        {
            IDictionary<string, string> properties = Hardware.Properties;

            if (properties.Count > 0)
            {
                StringBuilder stringBuilder = new();
                stringBuilder.AppendLine("Hardware properties:");

                foreach (KeyValuePair<string, string> property in properties)
                    stringBuilder.AppendFormat(" • {0}: {1}\n", property.Key, property.Value);

                return stringBuilder.ToString();
            }

            return null;
        }
    }

    public IHardware Hardware { get; }

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

    private void UpdateNode(TypeNode node)
    {
        if (node.Nodes.Count > 0)
        {
            if (!Nodes.Contains(node))
            {
                int i = 0;
                while (i < Nodes.Count && ((TypeNode)Nodes[i]).SensorType < node.SensorType)
                    i++;

                Nodes.Insert(i, node);
            }
        }
        else
        {
            if (Nodes.Contains(node))
                Nodes.Remove(node);
        }
    }

    private void SensorRemoved(ISensor sensor)
    {
        foreach (TypeNode typeNode in _typeNodes)
        {
            if (typeNode.SensorType == sensor.SensorType)
            {
                SensorNode sensorNode = null;
                foreach (Node node in typeNode.Nodes)
                {
                    if (node is SensorNode n && n.Sensor == sensor)
                        sensorNode = n;
                }
                if (sensorNode != null)
                {
                    typeNode.Nodes.Remove(sensorNode);
                    UpdateNode(typeNode);
                }
            }
        }
    }

    private void InsertSorted(Node node, ISensor sensor)
    {
        int i = 0;
        while (i < node.Nodes.Count && ((SensorNode)node.Nodes[i]).Sensor.Index < sensor.Index)
            i++;

        SensorNode sensorNode = new SensorNode(sensor, _settings);
        node.Nodes.Insert(i, sensorNode);
    }

    private void SensorAdded(ISensor sensor)
    {
        foreach (TypeNode typeNode in _typeNodes)
        {
            if (typeNode.SensorType == sensor.SensorType)
            {
                InsertSorted(typeNode, sensor);
                UpdateNode(typeNode);
            }
        }
    }
}
