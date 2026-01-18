namespace dotnet_mvc_test.Models.ViewModels
{
    /// <summary>
    /// 一般ユーザー向け記事一覧表示用ViewModel
    /// </summary>
    public class ArticleListViewModel
    {
        public int Id { get; set; }
        
        public required string Title { get; set; }
        
        public required string Slug { get; set; }
        
        public string? Excerpt { get; set; }
        
        public string? FeaturedImageUrl { get; set; }
        
        public string? CategoryName { get; set; }
        
        public required string AuthorName { get; set; }
        
        public DateTime PublishedAt { get; set; }
    }
}
