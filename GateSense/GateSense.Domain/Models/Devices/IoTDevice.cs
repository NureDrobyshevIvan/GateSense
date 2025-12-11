using Domain.Models;
using Domain.Models.Garages;
using Domain.Models.Sensors;

namespace Domain.Models.Devices;

public class IoTDevice : BaseEntity
{
    public int GarageId { get; set; }

    public Garage Garage { get; set; } = default!;

    public string SerialNumber { get; set; } = default!;

    public DeviceType DeviceType { get; set; }

    public DeviceStatus Status { get; set; } = DeviceStatus.Online;

    public string? FirmwareVersion { get; set; }

    public DateTimeOffset? LastHeartbeatOn { get; set; }

    public ICollection<SensorReading> SensorReadings { get; set; } = new List<SensorReading>();
}

