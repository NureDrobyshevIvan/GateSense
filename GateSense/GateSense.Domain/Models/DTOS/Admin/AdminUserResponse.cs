namespace Domain.Models.DTOS.Admin;

public class AdminUserResponse
{
    public int Id { get; set; }
    public string UserName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public bool EmailConfirmed { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public IList<string> Roles { get; set; } = new List<string>();
}

