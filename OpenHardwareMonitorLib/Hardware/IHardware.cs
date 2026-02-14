using System.Collections.Generic;

namespace OpenHardwareMonitor.Hardware;

/// <summary>
/// Handler that will trigger the actions assigned to it when the event occurs.
/// </summary>
/// <param name="sensor">Component returned to the assigned action(s).</param>
public delegate void SensorEventHandler(ISensor sensor);

/// <summary>
/// Abstract object that stores information about a device. All sensors are available as an array of <see cref="Sensors"/>.
/// <para>
/// Can contain <see cref="SubHardware"/>.
/// Type specified in <see cref="HardwareType"/>.
/// </para>
/// </summary>
public interface IHardware : IElement
{
    /// <summary>
    /// <inheritdoc cref="OpenHardwareMonitor.Hardware.HardwareType"/>
    /// </summary>
    HardwareType HardwareType { get; }

    /// <summary>
    /// Gets a unique hardware ID that represents its location.
    /// </summary>
    Identifier Identifier { get; }

    /// <summary>
    /// Gets or sets device name.
    /// </summary>
    string Name { get; set; }

    /// <summary>
    /// Gets the device that is the parent of the current hardware. For example, the motherboard is the parent of SuperIO.
    /// </summary>
    IHardware Parent { get; }

    /// <summary>
    /// Gets an array of all sensors such as <see cref="SensorType.Temperature"/>, <see cref="SensorType.Clock"/>, <see cref="SensorType.Load"/> etc.
    /// </summary>
    ISensor[] Sensors { get; }

    /// <summary>
    /// Gets child devices, e.g. <see cref="Motherboard.Lpc.LpcIO"/> of the <see cref="Motherboard.Motherboard"/>.
    /// </summary>
    IHardware[] SubHardware { get; }

    /// <summary>
    /// Report containing most of the known information about the current device.
    /// </summary>
    /// <returns>A formatted text string with hardware information.</returns>
    string GetReport();

    /// <summary>
    /// Refreshes the information stored in <see cref="Sensors"/> array.
    /// </summary>
    void Update();

    /// <summary>
    /// An <see langword="event"/> that will be triggered when a new sensor appears.
    /// </summary>
    event SensorEventHandler SensorAdded;

    /// <summary>
    /// An <see langword="event"/> that will be triggered when one of the sensors is removed.
    /// </summary>
    event SensorEventHandler SensorRemoved;

    /// <summary>
    /// Gets rarely changed hardware properties that can't be represented as sensors.
    /// </summary>
    IDictionary<string, string> Properties { get; }
}
