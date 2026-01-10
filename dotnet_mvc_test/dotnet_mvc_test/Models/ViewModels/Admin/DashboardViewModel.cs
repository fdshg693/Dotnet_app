namespace dotnet_mvc_test.Models.ViewModels.Admin
{
    public class DashboardViewModel
    {
        public int TotalArticles { get; set; }
        public int PublishedArticles { get; set; }
        public int DraftArticles { get; set; }
        public int TotalComments { get; set; }
        public int TotalCategories { get; set; }
        public int TotalTags { get; set; }
        public List<DashboardArticleViewModel> RecentArticles { get; set; } = new();
        public List<DashboardCommentViewModel> RecentComments { get; set; } = new();
    }

    public class DashboardArticleViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public bool IsPublished { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class DashboardCommentViewModel
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public string ArticleTitle { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
