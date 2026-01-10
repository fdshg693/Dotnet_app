using dotnet_mvc_test.Models.Entities;

namespace dotnet_mvc_test.Models.ViewModels.Admin
{
    public class ArticleListViewModel
    {
        public IEnumerable<ArticleListItemViewModel> Articles { get; set; } = new List<ArticleListItemViewModel>();
    }

    public class ArticleListItemViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? CategoryName { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public bool IsPublished { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? PublishedAt { get; set; }
        public int CommentCount { get; set; }
        public List<string> Tags { get; set; } = new();
    }
}
