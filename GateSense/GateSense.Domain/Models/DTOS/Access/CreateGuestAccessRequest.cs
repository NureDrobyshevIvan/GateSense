namespace Domain.Models.DTOS.Access;

public class CreateGuestAccessRequest
{
    public required int GarageId { get; set; }

    public required string RecipientName { get; set; }

    public string? RecipientEmail { get; set; }

    public DateTimeOffset? ExpiresOn { get; set; }
}

