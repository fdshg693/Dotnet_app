using dotnet_mvc_test.Data;
using dotnet_mvc_test.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace dotnet_mvc_test.Services
{
    public class ArticleService : IArticleService
    {
        private readonly ApplicationDbContext _context;

        public ArticleService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Article>> GetAllArticlesAsync()
        {
            return await _context.Articles
                .Include(a => a.Author)
                .Include(a => a.Category)
                .Include(a => a.ArticleTags)
                    .ThenInclude(at => at.Tag)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Article>> GetPublishedArticlesAsync()
        {
            return await _context.Articles
                .Include(a => a.Author)
                .Include(a => a.Category)
                .Include(a => a.ArticleTags)
                    .ThenInclude(at => at.Tag)
                .Where(a => a.IsPublished && a.PublishedAt <= DateTime.UtcNow)
                .OrderByDescending(a => a.PublishedAt)
                .ToListAsync();
        }

        public async Task<Article?> GetArticleByIdAsync(int id)
        {
            return await _context.Articles
                .Include(a => a.Author)
                .Include(a => a.Category)
                .Include(a => a.ArticleTags)
                    .ThenInclude(at => at.Tag)
                .Include(a => a.Comments)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<Article?> GetArticleBySlugAsync(string slug)
        {
            return await _context.Articles
                .Include(a => a.Author)
                .Include(a => a.Category)
                .Include(a => a.ArticleTags)
                    .ThenInclude(at => at.Tag)
                .Include(a => a.Comments.Where(c => c.IsApproved))
                .FirstOrDefaultAsync(a => a.Slug == slug && a.IsPublished);
        }

        public async Task<Article> CreateArticleAsync(Article article)
        {
            article.CreatedAt = DateTime.UtcNow;
            article.UpdatedAt = DateTime.UtcNow;
            
            if (article.IsPublished && article.PublishedAt == null)
            {
                article.PublishedAt = DateTime.UtcNow;
            }

            _context.Articles.Add(article);
            await _context.SaveChangesAsync();
            return article;
        }

        public async Task<bool> UpdateArticleAsync(Article article)
        {
            var existingArticle = await _context.Articles
                .Include(a => a.ArticleTags)
                .FirstOrDefaultAsync(a => a.Id == article.Id);

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

            _context.Articles.Update(existingArticle);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteArticleAsync(int id)
        {
            var article = await _context.Articles.FindAsync(id);
            if (article == null)
                return false;

            _context.Articles.Remove(article);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Article>> GetArticlesByCategoryAsync(int categoryId)
        {
            return await _context.Articles
                .Include(a => a.Author)
                .Include(a => a.Category)
                .Include(a => a.ArticleTags)
                    .ThenInclude(at => at.Tag)
                .Where(a => a.CategoryId == categoryId && a.IsPublished)
                .OrderByDescending(a => a.PublishedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Article>> GetArticlesByTagAsync(int tagId)
        {
            return await _context.Articles
                .Include(a => a.Author)
                .Include(a => a.Category)
                .Include(a => a.ArticleTags)
                    .ThenInclude(at => at.Tag)
                .Where(a => a.ArticleTags.Any(at => at.TagId == tagId) && a.IsPublished)
                .OrderByDescending(a => a.PublishedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Article>> SearchArticlesAsync(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return Enumerable.Empty<Article>();

            return await _context.Articles
                .Include(a => a.Author)
                .Include(a => a.Category)
                .Include(a => a.ArticleTags)
                    .ThenInclude(at => at.Tag)
                .Where(a => a.IsPublished && 
                    (a.Title.Contains(keyword) || 
                     a.Content.Contains(keyword) ||
                     (a.Excerpt != null && a.Excerpt.Contains(keyword))))
                .OrderByDescending(a => a.PublishedAt)
                .ToListAsync();
        }

        public async Task<bool> IsSlugUniqueAsync(string slug, int? articleId = null)
        {
            if (articleId.HasValue)
            {
                return !await _context.Articles
                    .AnyAsync(a => a.Slug == slug && a.Id != articleId.Value);
            }
            
            return !await _context.Articles.AnyAsync(a => a.Slug == slug);
        }
    }
}
