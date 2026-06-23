using MiniShop.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MiniShop.Interfaces
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryResponse>> GetAllAsync();
        Task<CategoryResponse?> GetByIdAsync(int id);
        Task<CategoryResponse> CreateAsync(CategoryCreateRequest request);
        Task<bool> UpdateAsync(int id, CategoryCreateRequest request);
        Task<bool> DeleteAsync(int id);
    }
}
