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

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] MiniShop.Models.Customer customer)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingCustomer = await _customerService.SearchByPhoneAsync(customer.Phone ?? string.Empty);
            if (existingCustomer != null)
            {
                return BadRequest(new { message = "Khách hàng với số điện thoại này đã tồn tại." });
            }

            customer.CreatedAt = System.DateTime.UtcNow;
            var createdCustomer = await _customerService.CreateAsync(customer);
            return Ok(createdCustomer);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] MiniShop.Models.Customer customer)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Optional: Check if another customer already has this phone
            if (!string.IsNullOrWhiteSpace(customer.Phone))
            {
                var existingCustomer = await _customerService.SearchByPhoneAsync(customer.Phone);
                if (existingCustomer != null && existingCustomer.Id != id)
                {
                    return BadRequest(new { message = "Khách hàng khác với số điện thoại này đã tồn tại." });
                }
            }

            var updatedCustomer = await _customerService.UpdateAsync(id, customer);
            if (updatedCustomer == null) return NotFound(new { message = "Không tìm thấy khách hàng." });

            return Ok(updatedCustomer);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _customerService.DeleteAsync(id);
            if (!result) return NotFound(new { message = "Không tìm thấy khách hàng." });

            return Ok(new { message = "Xóa khách hàng thành công." });
        }
    }
}
