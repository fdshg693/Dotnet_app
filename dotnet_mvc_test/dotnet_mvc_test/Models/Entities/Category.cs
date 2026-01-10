namespace dotnet_mvc_test.Models.Entities;

public class Category
{
    public int Id { get; set; }
    
    public required string Name { get; set; }
    
    public required string Slug { get; set; }
    
    public string? Description { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    // Navigation property
    public ICollection<Article> Articles { get; set; } = new List<Article>();
}
