using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniShop.Interfaces;
using System.Threading.Tasks;

namespace MiniShop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        public CustomersController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var customers = await _customerService.GetAllAsync();
            return Ok(customers);
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchByPhone([FromQuery] string phone)
        {
            var customer = await _customerService.SearchByPhoneAsync(phone);
            if (customer == null) return NotFound(new { message = "Không tìm thấy khách hàng với số điện thoại này." });
            return Ok(customer);
        }
    }
}
