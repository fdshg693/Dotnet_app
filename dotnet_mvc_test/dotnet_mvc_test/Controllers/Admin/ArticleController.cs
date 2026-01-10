using dotnet_mvc_test.Filters;
using dotnet_mvc_test.Models.Entities;
using dotnet_mvc_test.Models.ViewModels.Admin;
using dotnet_mvc_test.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace dotnet_mvc_test.Controllers.Admin
{
    [Authorize(Roles = "Administrator")]
    [RequireAdminTwoFactor]
    [Area("Admin")]
    [Route("admin/articles")]
    public class ArticleController : Controller
    {
        private readonly IArticleService _articleService;
        private readonly ICategoryService _categoryService;
        private readonly ITagService _tagService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ArticleController(
            IArticleService articleService,
            ICategoryService categoryService,
            ITagService tagService,
            UserManager<ApplicationUser> userManager)
        {
            _articleService = articleService;
            _categoryService = categoryService;
            _tagService = tagService;
            _userManager = userManager;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var articles = await _articleService.GetAllArticlesAsync();
            
            var viewModel = new ArticleListViewModel
            {
                Articles = articles.Select(a => new ArticleListItemViewModel
                {
                    Id = a.Id,
                    Title = a.Title,
                    Slug = a.Slug,
                    CategoryName = a.Category?.Name,
                    AuthorName = a.Author?.UserName ?? "Unknown",
                    IsPublished = a.IsPublished,
                    CreatedAt = a.CreatedAt,
                    PublishedAt = a.PublishedAt,
                    CommentCount = a.Comments?.Count ?? 0,
                    Tags = a.ArticleTags?.Select(at => at.Tag.Name).ToList() ?? new List<string>()
                })
            };

            return View(viewModel);
        }

        [HttpGet("create")]
        public async Task<IActionResult> Create()
        {
            await LoadSelectListsAsync();
            return View(new ArticleCreateViewModel());
        }

        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ArticleCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                // スラッグの一意性確認
                if (!await _articleService.IsSlugUniqueAsync(model.Slug))
                {
                    ModelState.AddModelError("Slug", "このスラッグは既に使用されています");
                    await LoadSelectListsAsync();
                    return View(model);
                }

                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized();
                }

                var article = new Article
                {
                    Title = model.Title,
                    Slug = model.Slug,
                    Content = model.Content,
                    Excerpt = model.Excerpt,
                    CategoryId = model.CategoryId,
                    FeaturedImageUrl = model.FeaturedImageUrl,
                    IsPublished = model.IsPublished,
                    AuthorId = user.Id,
                    ArticleTags = new List<ArticleTag>()
                };

                // タグの追加
                if (model.SelectedTagIds?.Any() == true)
                {
                    foreach (var tagId in model.SelectedTagIds)
                    {
                        article.ArticleTags.Add(new ArticleTag
                        {
                            TagId = tagId,
                            Article = article
                        });
                    }
                }

                await _articleService.CreateArticleAsync(article);
                TempData["SuccessMessage"] = "記事を作成しました";
                return RedirectToAction(nameof(Index));
            }

            await LoadSelectListsAsync();
            return View(model);
        }

        [HttpGet("edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var article = await _articleService.GetArticleByIdAsync(id);
            if (article == null)
            {
                return NotFound();
            }

            var model = new ArticleEditViewModel
            {
                Id = article.Id,
                Title = article.Title,
                Slug = article.Slug,
                Content = article.Content,
                Excerpt = article.Excerpt,
                CategoryId = article.CategoryId,
                FeaturedImageUrl = article.FeaturedImageUrl,
                IsPublished = article.IsPublished,
                SelectedTagIds = article.ArticleTags?.Select(at => at.TagId).ToList() ?? new List<int>(),
                CreatedAt = article.CreatedAt,
                PublishedAt = article.PublishedAt
            };

            await LoadSelectListsAsync();
            return View(model);
        }

        [HttpPost("edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ArticleEditViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // スラッグの一意性確認
                if (!await _articleService.IsSlugUniqueAsync(model.Slug, model.Id))
                {
                    ModelState.AddModelError("Slug", "このスラッグは既に使用されています");
                    await LoadSelectListsAsync();
                    return View(model);
                }

                var article = await _articleService.GetArticleByIdAsync(id);
                if (article == null)
                {
                    return NotFound();
                }

                article.Title = model.Title;
                article.Slug = model.Slug;
                article.Content = model.Content;
                article.Excerpt = model.Excerpt;
                article.CategoryId = model.CategoryId;
                article.FeaturedImageUrl = model.FeaturedImageUrl;
                article.IsPublished = model.IsPublished;

                // タグの更新
                article.ArticleTags = new List<ArticleTag>();
                if (model.SelectedTagIds?.Any() == true)
                {
                    foreach (var tagId in model.SelectedTagIds)
                    {
                        article.ArticleTags.Add(new ArticleTag
                        {
                            ArticleId = article.Id,
                            TagId = tagId
                        });
                    }
                }

                var success = await _articleService.UpdateArticleAsync(article);
                if (success)
                {
                    TempData["SuccessMessage"] = "記事を更新しました";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError("", "記事の更新に失敗しました");
                }
            }

            await LoadSelectListsAsync();
            return View(model);
        }

        [HttpGet("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var article = await _articleService.GetArticleByIdAsync(id);
            if (article == null)
            {
                return NotFound();
            }

            var model = new ArticleDetailViewModel
            {
                Id = article.Id,
                Title = article.Title,
                Slug = article.Slug,
                Content = article.Content,
                Excerpt = article.Excerpt,
                CategoryName = article.Category?.Name,
                FeaturedImageUrl = article.FeaturedImageUrl,
                AuthorName = article.Author?.UserName ?? "Unknown",
                IsPublished = article.IsPublished,
                CreatedAt = article.CreatedAt,
                UpdatedAt = article.UpdatedAt,
                PublishedAt = article.PublishedAt,
                Tags = article.ArticleTags?.Select(at => at.Tag.Name).ToList() ?? new List<string>(),
                CommentCount = article.Comments?.Count ?? 0
            };

            return View(model);
        }

        [HttpPost("delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var success = await _articleService.DeleteArticleAsync(id);
            if (success)
            {
                TempData["SuccessMessage"] = "記事を削除しました";
            }
            else
            {
                TempData["ErrorMessage"] = "記事の削除に失敗しました";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet("detail/{id}")]
        public async Task<IActionResult> Detail(int id)
        {
            var article = await _articleService.GetArticleByIdAsync(id);
            if (article == null)
            {
                return NotFound();
            }

            var model = new ArticleDetailViewModel
            {
                Id = article.Id,
                Title = article.Title,
                Slug = article.Slug,
                Content = article.Content,
                Excerpt = article.Excerpt,
                CategoryName = article.Category?.Name,
                FeaturedImageUrl = article.FeaturedImageUrl,
                AuthorName = article.Author?.UserName ?? "Unknown",
                IsPublished = article.IsPublished,
                CreatedAt = article.CreatedAt,
                UpdatedAt = article.UpdatedAt,
                PublishedAt = article.PublishedAt,
                Tags = article.ArticleTags?.Select(at => at.Tag.Name).ToList() ?? new List<string>(),
                CommentCount = article.Comments?.Count ?? 0
            };

            return View(model);
        }

        private async Task LoadSelectListsAsync()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            var tags = await _tagService.GetAllTagsAsync();

            ViewBag.Categories = new SelectList(categories, "Id", "Name");
            ViewBag.Tags = new MultiSelectList(tags, "Id", "Name");
        }
    }
}
