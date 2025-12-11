namespace Domain.Models.DTOS.Sensors;

public class SensorDataSubmissionRequest
{
    public string SerialNumber { get; set; } = default!;

    public string SensorType { get; set; } = default!;

    public decimal Value { get; set; }

    public string Unit { get; set; } = default!;

    public DateTimeOffset RecordedOn { get; set; }
}

