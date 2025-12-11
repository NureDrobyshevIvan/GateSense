namespace Domain.Models.Auth;

public class AccessToken : BaseEntity
{
    public string Token { get; set; }
    
    public int UserId { get; set; }
    
    public ApplicationUser User { get; set; }
}