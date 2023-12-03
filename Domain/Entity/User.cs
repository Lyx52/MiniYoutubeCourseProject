using Microsoft.AspNetCore.Identity;

namespace Domain.Entity;

public class User : IdentityUser
{
    public string? Icon { get; set; }
    public IEnumerable<Video> Videos { get; set; }
}