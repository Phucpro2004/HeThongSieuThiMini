using MiniShop.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MiniShop.Interfaces
{
    public interface IProductService
    {
        Task<PagedResponse<ProductResponse>> GetAllAsync(int pageNumber = 1, int pageSize = 10, string? searchKeyword = null);
        Task<ProductResponse?> GetByIdAsync(int id);
        Task<ProductResponse> CreateAsync(ProductCreateRequest request);
        Task<bool> UpdateAsync(int id, ProductCreateRequest request);
        Task<bool> DeleteAsync(int id);
        Task<bool> RestockAsync(int productId, int quantity);
    }
}
