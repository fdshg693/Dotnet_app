namespace dotnet_mvc_test.Models.Entities;

public class Comment
{
    public int Id { get; set; }
    
    public int ArticleId { get; set; }
    
    public required string AuthorName { get; set; }
    
    public required string AuthorEmail { get; set; }
    
    public required string Content { get; set; }
    
    public bool IsApproved { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    // Navigation property
    public Article Article { get; set; } = null!;
}
