using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using dotnet_mvc_test.Models.Entities;

namespace dotnet_mvc_test.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        // データベースが作成されていることを確認
        await context.Database.MigrateAsync();

        // ロールの作成
        await CreateRolesAsync(roleManager);

        // 管理者ユーザーの作成
        await CreateAdminUserAsync(userManager);

        // カテゴリの初期データ
        await SeedCategoriesAsync(context);

        // タグの初期データ
        await SeedTagsAsync(context);
    }

    private static async Task CreateRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        string[] roleNames = { "Administrator", "User" };

        foreach (var roleName in roleNames)
        {
            var roleExist = await roleManager.RoleExistsAsync(roleName);
            if (!roleExist)
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }
    }

    private static async Task CreateAdminUserAsync(UserManager<ApplicationUser> userManager)
    {
        var adminEmail = "admin@example.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            var newAdmin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                DisplayName = "管理者",
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(newAdmin, "Admin123!");

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(newAdmin, "Administrator");
            }
        }
    }

    private static async Task SeedCategoriesAsync(ApplicationDbContext context)
    {
        if (!await context.Categories.AnyAsync())
        {
            var categories = new List<Category>
            {
                new Category
                {
                    Name = "技術",
                    Slug = "tech",
                    Description = "プログラミングや技術に関する記事",
                    CreatedAt = DateTime.UtcNow
                },
                new Category
                {
                    Name = "日記",
                    Slug = "diary",
                    Description = "日常の出来事や雑記",
                    CreatedAt = DateTime.UtcNow
                },
                new Category
                {
                    Name = "お知らせ",
                    Slug = "news",
                    Description = "サイトからのお知らせ",
                    CreatedAt = DateTime.UtcNow
                }
            };

            await context.Categories.AddRangeAsync(categories);
            await context.SaveChangesAsync();
        }
    }

    private static async Task SeedTagsAsync(ApplicationDbContext context)
    {
        if (!await context.Tags.AnyAsync())
        {
            var tags = new List<Tag>
            {
                new Tag
                {
                    Name = "C#",
                    Slug = "csharp",
                    CreatedAt = DateTime.UtcNow
                },
                new Tag
                {
                    Name = "ASP.NET",
                    Slug = "aspnet",
                    CreatedAt = DateTime.UtcNow
                },
                new Tag
                {
                    Name = "Entity Framework",
                    Slug = "entity-framework",
                    CreatedAt = DateTime.UtcNow
                },
                new Tag
                {
                    Name = "Web開発",
                    Slug = "web-development",
                    CreatedAt = DateTime.UtcNow
                },
                new Tag
                {
                    Name = "データベース",
                    Slug = "database",
                    CreatedAt = DateTime.UtcNow
                }
            };

            await context.Tags.AddRangeAsync(tags);
            await context.SaveChangesAsync();
        }
    }
}
