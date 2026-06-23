using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniShop.DTOs;
using MiniShop.Interfaces;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MiniShop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class GoodsReceiptsController : ControllerBase
    {
        private readonly IGoodsReceiptService _goodsReceiptService;

        public GoodsReceiptsController(IGoodsReceiptService goodsReceiptService)
        {
            _goodsReceiptService = goodsReceiptService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateReceipt([FromBody] GoodsReceiptCreateRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Extract userId from token claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized("Người dùng không hợp lệ.");
            }

            try
            {
                var receipt = await _goodsReceiptService.CreateReceiptAsync(request, userId);
                return CreatedAtAction(nameof(GetById), new { id = receipt.Id }, receipt);
            }
            catch (Exceptions.NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi tạo phiếu nhập: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var receipts = await _goodsReceiptService.GetAllReceiptsAsync();
            return Ok(receipts);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var receipt = await _goodsReceiptService.GetReceiptByIdAsync(id);
            if (receipt == null) return NotFound(new { message = "Phiếu nhập không tồn tại." });
            return Ok(receipt);
        }
    }
}
