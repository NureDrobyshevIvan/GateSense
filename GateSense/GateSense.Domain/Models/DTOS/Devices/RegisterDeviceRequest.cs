namespace Domain.Models.DTOS.Devices;

public class RegisterDeviceRequest
{
    public string SerialNumber { get; set; } = default!;

    public string? FirmwareVersion { get; set; }

    public string DeviceType { get; set; } = default!;
}

