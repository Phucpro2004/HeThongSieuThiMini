using MiniShop.DTOs;
using MiniShop.Models;
using System.Threading.Tasks;

namespace MiniShop.Interfaces
{
    public interface IOrderService
    {
        Task<Order> CheckoutAsync(CheckoutRequest request, int cashierId);
        Task<IEnumerable<OrderResponse>> GetAllOrdersAsync();
        Task<OrderResponse?> GetOrderByIdAsync(int id);
    }
}
