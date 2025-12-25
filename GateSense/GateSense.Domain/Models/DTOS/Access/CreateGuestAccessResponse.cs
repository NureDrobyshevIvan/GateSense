namespace Domain.Models.DTOS.Access;

public class CreateGuestAccessResponse
{
    public int Id { get; set; }
    public string Token { get; set; } = default!;
    public DateTimeOffset? ExpiresOn { get; set; }
}

