namespace Domain.Models.DTOS.Gates;

public class GateCommandRequest
{
    public required int GarageId { get; set; }

    public string? AccessKeyToken { get; set; }
}

