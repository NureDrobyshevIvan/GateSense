namespace Domain.Models.DTOS.Admin;

public class AdminGarageResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string Address { get; set; } = default!;
    public string TimeZone { get; set; } = default!;
    public int OwnerId { get; set; }
    public string? OwnerEmail { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
}

