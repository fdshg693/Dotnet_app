using Microsoft.AspNetCore.Identity;

namespace dotnet_mvc_test.Models.Entities;

public class ApplicationUser : IdentityUser
{
    public string? DisplayName { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    // Navigation property
    public ICollection<Article> Articles { get; set; } = new List<Article>();
}
