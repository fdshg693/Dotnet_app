using dotnet_mvc_test.Data;
using dotnet_mvc_test.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace dotnet_mvc_test.Repositories;

/// <summary>
/// 記事データアクセス用リポジトリ実装
/// </summary>
public class ArticleRepository : IArticleRepository
{
    private readonly ApplicationDbContext _context;

    public ArticleRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Article>> GetAllAsync()
    {
        return await _context.Articles
            .Include(a => a.Author)
            .Include(a => a.Category)
            .Include(a => a.ArticleTags)
                .ThenInclude(at => at.Tag)
            .Where(a => !a.IsDeleted)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Article>> GetPublishedAsync()
    {
        return await _context.Articles
            .Include(a => a.Author)
            .Include(a => a.Category)
            .Include(a => a.ArticleTags)
                .ThenInclude(at => at.Tag)
            .Where(a => a.IsPublished && a.PublishedAt <= DateTime.UtcNow && !a.IsDeleted)
            .OrderByDescending(a => a.PublishedAt)
            .ToListAsync();
    }

    public async Task<Article?> GetByIdAsync(int id)
    {
        return await _context.Articles
            .Include(a => a.Author)
            .Include(a => a.Category)
            .Include(a => a.ArticleTags)
                .ThenInclude(at => at.Tag)
            .Include(a => a.Comments)
            .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);
    }

    public async Task<Article?> GetBySlugAsync(string slug)
    {
        return await _context.Articles
            .Include(a => a.Author)
            .Include(a => a.Category)
            .Include(a => a.ArticleTags)
                .ThenInclude(at => at.Tag)
            .Include(a => a.Comments.Where(c => c.IsApproved))
            .FirstOrDefaultAsync(a => a.Slug == slug && a.IsPublished && a.PublishedAt <= DateTime.UtcNow && !a.IsDeleted);
    }

    public async Task<IEnumerable<Article>> GetByCategoryIdAsync(int categoryId)
    {
        return await _context.Articles
            .Include(a => a.Author)
            .Include(a => a.Category)
            .Include(a => a.ArticleTags)
                .ThenInclude(at => at.Tag)
            .Where(a => a.CategoryId == categoryId && a.IsPublished && a.PublishedAt <= DateTime.UtcNow && !a.IsDeleted)
            .OrderByDescending(a => a.PublishedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Article>> GetByTagIdAsync(int tagId)
    {
        return await _context.Articles
            .Include(a => a.Author)
            .Include(a => a.Category)
            .Include(a => a.ArticleTags)
                .ThenInclude(at => at.Tag)
            .Where(a => a.ArticleTags.Any(at => at.TagId == tagId) && a.IsPublished && a.PublishedAt <= DateTime.UtcNow && !a.IsDeleted)
            .OrderByDescending(a => a.PublishedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Article>> SearchAsync(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            return Enumerable.Empty<Article>();

        return await _context.Articles
            .Include(a => a.Author)
            .Include(a => a.Category)
            .Include(a => a.ArticleTags)
                .ThenInclude(at => at.Tag)
            .Where(a => a.IsPublished && 
                a.PublishedAt <= DateTime.UtcNow &&
                !a.IsDeleted &&
                (a.Title.Contains(keyword) || 
                 a.Content.Contains(keyword) ||
                 (a.Excerpt != null && a.Excerpt.Contains(keyword))))
            .OrderByDescending(a => a.PublishedAt)
            .ToListAsync();
    }

    public async Task<Article> AddAsync(Article article)
    {
        _context.Articles.Add(article);
        await _context.SaveChangesAsync();
        return article;
    }

    public async Task<bool> UpdateAsync(Article article)
    {
        var existingArticle = await _context.Articles
            .Include(a => a.ArticleTags)
            .FirstOrDefaultAsync(a => a.Id == article.Id && !a.IsDeleted);

        if (existingArticle == null)
            return false;

        existingArticle.Title = article.Title;
        existingArticle.Slug = article.Slug;
        existingArticle.Content = article.Content;
        existingArticle.Excerpt = article.Excerpt;
        existingArticle.CategoryId = article.CategoryId;
        existingArticle.FeaturedImageUrl = article.FeaturedImageUrl;
        existingArticle.IsPublished = article.IsPublished;
        existingArticle.PublishedAt = article.PublishedAt;

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

    public async Task<bool> DeleteAsync(int id)
    {
        var article = await _context.Articles.FindAsync(id);
        if (article == null)
            return false;

        _context.Articles.Remove(article);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> IsSlugUniqueAsync(string slug, int? excludeId = null)
    {
        if (excludeId.HasValue)
        {
            return !await _context.Articles
                .AnyAsync(a => a.Slug == slug && a.Id != excludeId.Value && !a.IsDeleted);
        }
        
        return !await _context.Articles.AnyAsync(a => a.Slug == slug && !a.IsDeleted);
    }
}
