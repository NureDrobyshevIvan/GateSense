namespace Domain.Models.Auth;

public class RefreshToken : BaseEntity
{
    public string Token { get; set; }
    
    public int UserId { get; set; }
    
    public ApplicationUser User { get; set; } 
    
    public DateTime ExpiresOnUtc { get; set; }
}