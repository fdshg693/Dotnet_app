using dotnet_mvc_test.Models.Entities;
using dotnet_mvc_test.Repositories;

namespace dotnet_mvc_test.Services
{
    public class ArticleService : IArticleService
    {
        private readonly IArticleRepository _repository;

        public ArticleService(IArticleRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<Article>> GetAllArticlesAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<IEnumerable<Article>> GetPublishedArticlesAsync()
        {
            return await _repository.GetPublishedAsync();
        }

        public async Task<Article?> GetArticleByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<Article?> GetArticleBySlugAsync(string slug)
        {
            return await _repository.GetBySlugAsync(slug);
        }

        public async Task<Article> CreateArticleAsync(Article article)
        {
            article.CreatedAt = DateTime.UtcNow;
            article.UpdatedAt = DateTime.UtcNow;
            
            if (article.IsPublished && article.PublishedAt == null)
            {
                article.PublishedAt = DateTime.UtcNow;
            }

            return await _repository.AddAsync(article);
        }

        public async Task<bool> UpdateArticleAsync(Article article)
        {
            var existingArticle = await _repository.GetByIdAsync(article.Id);

            if (existingArticle == null)
                return false;

            existingArticle.Title = article.Title;
            existingArticle.Slug = article.Slug;
            existingArticle.Content = article.Content;
            existingArticle.Excerpt = article.Excerpt;
            existingArticle.CategoryId = article.CategoryId;
            existingArticle.FeaturedImageUrl = article.FeaturedImageUrl;
            existingArticle.IsPublished = article.IsPublished;
            existingArticle.UpdatedAt = DateTime.UtcNow;

            // 公開状態が変わった場合
            if (article.IsPublished && existingArticle.PublishedAt == null)
            {
                existingArticle.PublishedAt = DateTime.UtcNow;
            }

            // タグの更新
            existingArticle.ArticleTags.Clear();
            if (article.ArticleTags != null)
            {
                foreach (var articleTag in article.ArticleTags)
                {
                    existingArticle.ArticleTags.Add(articleTag);
                }
            }

            return await _repository.UpdateAsync(existingArticle);
        }

        public async Task<bool> DeleteArticleAsync(int id)
        {
            return await _repository.DeleteAsync(id);
        }

        public async Task<IEnumerable<Article>> GetArticlesByCategoryAsync(int categoryId)
        {
            return await _repository.GetByCategoryIdAsync(categoryId);
        }

        public async Task<IEnumerable<Article>> GetArticlesByTagAsync(int tagId)
        {
            return await _repository.GetByTagIdAsync(tagId);
        }

        public async Task<IEnumerable<Article>> SearchArticlesAsync(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return Enumerable.Empty<Article>();

            return await _repository.SearchAsync(keyword);
        }

        public async Task<bool> IsSlugUniqueAsync(string slug, int? articleId = null)
        {
            return await _repository.IsSlugUniqueAsync(slug, articleId);
        }
    }
}
