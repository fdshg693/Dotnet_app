namespace dotnet_mvc_test.Models.Entities;

public class ArticleTag
{
    public int ArticleId { get; set; }
    public Article Article { get; set; } = null!;
    
    public int TagId { get; set; }
    public Tag Tag { get; set; } = null!;
}
