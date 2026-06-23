using MiniShop.DTOs;
using MiniShop.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MiniShop.Interfaces
{
    public interface IGoodsReceiptService
    {
        Task<GoodsReceipt> CreateReceiptAsync(GoodsReceiptCreateRequest request, int userId);
        Task<IEnumerable<GoodsReceiptResponse>> GetAllReceiptsAsync();
        Task<GoodsReceiptResponse?> GetReceiptByIdAsync(int id);
    }
}
