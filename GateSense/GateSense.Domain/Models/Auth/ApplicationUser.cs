using Microsoft.AspNetCore.Identity;

namespace Domain.Models.Auth;

public class ApplicationUser : IdentityUser<int>
{
    public string? FirstName { get; set; }
    
    public string? LastName { get; set; }
}