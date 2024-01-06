using System.ComponentModel.DataAnnotations;
namespace Domain.Entity;

public class RefreshToken : IdEntity<string>
{
    public string Token { get; set; }
    public string UserId { get; set; }
    public User User { get; set; }
    public DateTime ExpiryDate { get; set; }
    public bool IsRevoked { get; set; }
    public long RefreshCount { get; set; }
}