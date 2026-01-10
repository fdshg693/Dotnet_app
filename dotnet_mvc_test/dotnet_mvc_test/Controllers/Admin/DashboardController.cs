using dotnet_mvc_test.Data;
using dotnet_mvc_test.Filters;
using dotnet_mvc_test.Models.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace dotnet_mvc_test.Controllers.Admin
{
    [Authorize(Roles = "Administrator")]
    [RequireAdminTwoFactor]
    [Area("Admin")]
    [Route("admin")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("")]
        [HttpGet("dashboard")]
        public async Task<IActionResult> Index()
        {
            var viewModel = new DashboardViewModel
            {
                TotalArticles = await _context.Articles.CountAsync(),
                PublishedArticles = await _context.Articles.CountAsync(a => a.IsPublished),
                DraftArticles = await _context.Articles.CountAsync(a => !a.IsPublished),
                TotalComments = await _context.Comments.CountAsync(),
                TotalCategories = await _context.Categories.CountAsync(),
                TotalTags = await _context.Tags.CountAsync(),
                RecentArticles = await _context.Articles
                    .Include(a => a.Author)
                    .Include(a => a.Category)
                    .OrderByDescending(a => a.CreatedAt)
                    .Take(5)
                    .Select(a => new DashboardArticleViewModel
                    {
                        Id = a.Id,
                        Title = a.Title,
                        AuthorName = a.Author!.UserName ?? "Unknown",
                        CategoryName = a.Category!.Name,
                        IsPublished = a.IsPublished,
                        CreatedAt = a.CreatedAt
                    })
                    .ToListAsync(),
                RecentComments = await _context.Comments
                    .Include(c => c.Article)
                    .OrderByDescending(c => c.CreatedAt)
                    .Take(5)
                    .Select(c => new DashboardCommentViewModel
                    {
                        Id = c.Id,
                        Content = c.Content.Length > 100 ? c.Content.Substring(0, 100) + "..." : c.Content,
                        ArticleTitle = c.Article!.Title,
                        AuthorName = c.AuthorName,
                        CreatedAt = c.CreatedAt
                    })
                    .ToListAsync()
            };

            return View(viewModel);
        }
    }
}
