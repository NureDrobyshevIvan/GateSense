namespace Domain.Models.DTOS.Admin;

public class AdminUpdateUserRequest
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public bool? EmailConfirmed { get; set; }
}

