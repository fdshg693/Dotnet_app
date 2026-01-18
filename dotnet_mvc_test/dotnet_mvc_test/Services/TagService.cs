using dotnet_mvc_test.Models.Entities;
using dotnet_mvc_test.Repositories;

namespace dotnet_mvc_test.Services
{
    public class TagService : ITagService
    {
        private readonly ITagRepository _repository;

        public TagService(ITagRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<Tag>> GetAllTagsAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<Tag?> GetTagByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Tag>> GetTagsByIdsAsync(IEnumerable<int> ids)
        {
            return await _repository.GetByIdsAsync(ids);
        }

        public async Task<Tag> CreateTagAsync(Tag tag)
        {
            return await _repository.AddAsync(tag);
        }

        public async Task<bool> UpdateTagAsync(Tag tag)
        {
            return await _repository.UpdateAsync(tag);
        }

        public async Task<bool> DeleteTagAsync(int id)
        {
            return await _repository.DeleteAsync(id);
        }
    }
}
