namespace dotnet_mvc_test.Models.ViewModels
{
    /// <summary>
    /// 一般ユーザー向け記事詳細表示用ViewModel
    /// </summary>
    public class ArticleDetailViewModel
    {
        public int Id { get; set; }
        
        public required string Title { get; set; }
        
        public required string Slug { get; set; }
        
        public required string RenderedContent { get; set; }
        
        public string? CategoryName { get; set; }
        
        public required List<string> TagNames { get; set; } = new();
        
        public required string AuthorName { get; set; }
        
        public DateTime PublishedAt { get; set; }
    }
}
