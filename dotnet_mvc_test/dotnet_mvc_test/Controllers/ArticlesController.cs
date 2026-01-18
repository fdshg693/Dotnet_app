using dotnet_mvc_test.Models.ViewModels;
using dotnet_mvc_test.Services;
using Microsoft.AspNetCore.Mvc;

namespace dotnet_mvc_test.Controllers
{
    /// <summary>
    /// 一般ユーザー向けの記事表示コントローラー
    /// </summary>
    public class ArticlesController : Controller
    {
        private readonly IArticleService _articleService;
        private readonly IMarkdownService _markdownService;

        public ArticlesController(
            IArticleService articleService,
            IMarkdownService markdownService)
        {
            _articleService = articleService;
            _markdownService = markdownService;
        }

        /// <summary>
        /// 公開記事一覧を表示
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var articles = await _articleService.GetPublishedArticlesAsync();

            // 公開日降順でソート
            var viewModels = articles
                .OrderByDescending(a => a.PublishedAt)
                .Select(a => new ArticleListViewModel
                {
                    Id = a.Id,
                    Title = a.Title,
                    Slug = a.Slug,
                    Excerpt = a.Excerpt,
                    FeaturedImageUrl = a.FeaturedImageUrl,
                    CategoryName = a.Category?.Name,
                    AuthorName = a.Author?.UserName ?? "Unknown",
                    PublishedAt = a.PublishedAt ?? DateTime.UtcNow
                })
                .ToList();

            return View(viewModels);
        }

        /// <summary>
        /// 記事詳細をスラッグで表示
        /// </summary>
        /// <param name="slug">記事のスラッグ</param>
        [HttpGet]
        public async Task<IActionResult> Details(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
            {
                return NotFound();
            }

            var article = await _articleService.GetArticleBySlugAsync(slug);

            if (article == null || !article.IsPublished)
            {
                return NotFound();
            }

            // MarkdownをHTMLに変換
            var renderedContent = _markdownService.ToHtml(article.Content);

            var viewModel = new ArticleDetailViewModel
            {
                Id = article.Id,
                Title = article.Title,
                Slug = article.Slug,
                RenderedContent = renderedContent,
                CategoryName = article.Category?.Name,
                TagNames = article.ArticleTags?
                    .Select(at => at.Tag.Name)
                    .ToList() ?? new List<string>(),
                AuthorName = article.Author?.UserName ?? "Unknown",
                PublishedAt = article.PublishedAt ?? DateTime.UtcNow
            };

            return View(viewModel);
        }
    }
}
