using dotnet_mvc_test.Data;
using dotnet_mvc_test.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace dotnet_mvc_test.Repositories;

/// <summary>
/// タグデータアクセス用リポジトリ実装
/// </summary>
public class TagRepository : ITagRepository
{
    private readonly ApplicationDbContext _context;

    public TagRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Tag>> GetAllAsync()
    {
        return await _context.Tags
            .OrderBy(t => t.Name)
            .ToListAsync();
    }

    public async Task<Tag?> GetByIdAsync(int id)
    {
        return await _context.Tags.FindAsync(id);
    }

    public async Task<IEnumerable<Tag>> GetByIdsAsync(IEnumerable<int> ids)
    {
        return await _context.Tags
            .Where(t => ids.Contains(t.Id))
            .ToListAsync();
    }

    public async Task<Tag> AddAsync(Tag tag)
    {
        _context.Tags.Add(tag);
        await _context.SaveChangesAsync();
        return tag;
    }

    public async Task<bool> UpdateAsync(Tag tag)
    {
        var existingTag = await _context.Tags.FindAsync(tag.Id);
        if (existingTag == null)
            return false;

        existingTag.Name = tag.Name;
        existingTag.Slug = tag.Slug;

        _context.Tags.Update(existingTag);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var tag = await _context.Tags.FindAsync(id);
        if (tag == null)
            return false;

        _context.Tags.Remove(tag);
        await _context.SaveChangesAsync();
        return true;
    }
}
