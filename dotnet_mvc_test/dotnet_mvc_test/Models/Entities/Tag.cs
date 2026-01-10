namespace dotnet_mvc_test.Models.Entities;

public class Tag
{
    public int Id { get; set; }
    
    public required string Name { get; set; }
    
    public required string Slug { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    // Navigation property
    public ICollection<ArticleTag> ArticleTags { get; set; } = new List<ArticleTag>();
}
