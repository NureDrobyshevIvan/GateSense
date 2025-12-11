using Domain.Models;
using Domain.Models.Devices;

namespace Domain.Models.Sensors;

public class SensorReading : BaseEntity
{
    public int DeviceId { get; set; }

    public IoTDevice Device { get; set; } = default!;

    public SensorType SensorType { get; set; }

    public decimal Value { get; set; }

    public string Unit { get; set; } = default!;

    public DateTimeOffset RecordedOn { get; set; }
}

