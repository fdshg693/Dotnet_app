namespace dotnet_mvc_test.Models.Entities;

public class Article
{
    public int Id { get; set; }
    
    public required string Title { get; set; }
    
    public required string Slug { get; set; }
    
    public required string Content { get; set; }
    
    public string? Summary { get; set; }
    
    public string? Excerpt { get; set; }
    
    public string? FeaturedImageUrl { get; set; }
    
    public int? CategoryId { get; set; }
    
    public required string AuthorId { get; set; }
    
    public bool IsPublished { get; set; }
    
    public DateTime? PublishedAt { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }
    
    public bool IsDeleted { get; set; }
    
    // Navigation properties
    public Category? Category { get; set; }
    
    public ApplicationUser Author { get; set; } = null!;
    
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    
    public ICollection<ArticleTag> ArticleTags { get; set; } = new List<ArticleTag>();
}
