using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniShop.DTOs;
using MiniShop.Interfaces;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MiniShop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost("checkout")]
        [Authorize(Roles = "Cashier,Admin")]
        public async Task<IActionResult> Checkout([FromBody] CheckoutRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int cashierId))
            {
                return Unauthorized(new { message = "Không xác định được CashierId từ Token." });
            }

            var order = await _orderService.CheckoutAsync(request, cashierId);
            
            return Ok(new { message = "Thanh toán thành công.", orderId = order.Id, orderCode = order.OrderCode });
        }
    }
}
