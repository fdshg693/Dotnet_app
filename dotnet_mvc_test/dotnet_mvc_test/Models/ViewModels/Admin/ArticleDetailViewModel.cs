using dotnet_mvc_test.Models.Entities;

namespace dotnet_mvc_test.Models.ViewModels.Admin
{
    public class ArticleDetailViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? Excerpt { get; set; }
        public string? CategoryName { get; set; }
        public string? FeaturedImageUrl { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public bool IsPublished { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? PublishedAt { get; set; }
        public List<string> Tags { get; set; } = new();
        public int CommentCount { get; set; }
    }
}
