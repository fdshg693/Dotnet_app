using dotnet_mvc_test.Models.Entities;

namespace dotnet_mvc_test.Services
{
    public interface IArticleService
    {
        // 記事一覧取得
        Task<IEnumerable<Article>> GetAllArticlesAsync();
        
        // 公開記事一覧取得
        Task<IEnumerable<Article>> GetPublishedArticlesAsync();
        
        // 記事詳細取得（ID指定）
        Task<Article?> GetArticleByIdAsync(int id);
        
        // 記事詳細取得（スラッグ指定）
        Task<Article?> GetArticleBySlugAsync(string slug);
        
        // 記事作成
        Task<Article> CreateArticleAsync(Article article);
        
        // 記事更新
        Task<bool> UpdateArticleAsync(Article article);
        
        // 記事削除
        Task<bool> DeleteArticleAsync(int id);
        
        // カテゴリ別記事取得
        Task<IEnumerable<Article>> GetArticlesByCategoryAsync(int categoryId);
        
        // タグ別記事取得
        Task<IEnumerable<Article>> GetArticlesByTagAsync(int tagId);
        
        // 記事検索
        Task<IEnumerable<Article>> SearchArticlesAsync(string keyword);
        
        // スラッグの一意性確認
        Task<bool> IsSlugUniqueAsync(string slug, int? articleId = null);
    }
}
