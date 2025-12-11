namespace Domain.Models.DTOS.Garages;

public class CreateGarageRequest
{
    public required string Name { get; set; }

    public string? Address { get; set; }

    public string? TimeZone { get; set; }
}

