using dotnet_mvc_test.Data;
using dotnet_mvc_test.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace dotnet_mvc_test.Repositories;

/// <summary>
/// カテゴリデータアクセス用リポジトリ実装
/// </summary>
public class CategoryRepository : ICategoryRepository
{
    private readonly ApplicationDbContext _context;

    public CategoryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Category>> GetAllAsync()
    {
        return await _context.Categories
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<Category?> GetByIdAsync(int id)
    {
        return await _context.Categories.FindAsync(id);
    }

    public async Task<Category> AddAsync(Category category)
    {
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
        return category;
    }

    public async Task<bool> UpdateAsync(Category category)
    {
        var existingCategory = await _context.Categories.FindAsync(category.Id);
        if (existingCategory == null)
            return false;

        existingCategory.Name = category.Name;
        existingCategory.Slug = category.Slug;
        existingCategory.Description = category.Description;

        _context.Categories.Update(existingCategory);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null)
            return false;

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
        return true;
    }
}
