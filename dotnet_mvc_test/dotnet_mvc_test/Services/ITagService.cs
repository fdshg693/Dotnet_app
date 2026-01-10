using dotnet_mvc_test.Models.Entities;

namespace dotnet_mvc_test.Services
{
    public interface ITagService
    {
        Task<IEnumerable<Tag>> GetAllTagsAsync();
        Task<Tag?> GetTagByIdAsync(int id);
        Task<IEnumerable<Tag>> GetTagsByIdsAsync(IEnumerable<int> ids);
        Task<Tag> CreateTagAsync(Tag tag);
        Task<bool> UpdateTagAsync(Tag tag);
        Task<bool> DeleteTagAsync(int id);
    }
}
