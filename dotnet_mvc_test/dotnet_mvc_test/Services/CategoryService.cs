using dotnet_mvc_test.Models.Entities;
using dotnet_mvc_test.Repositories;

namespace dotnet_mvc_test.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _repository;

        public CategoryService(ICategoryRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<Category?> GetCategoryByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<Category> CreateCategoryAsync(Category category)
        {
            return await _repository.AddAsync(category);
        }

        public async Task<bool> UpdateCategoryAsync(Category category)
        {
            return await _repository.UpdateAsync(category);
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            return await _repository.DeleteAsync(id);
        }
    }
}
