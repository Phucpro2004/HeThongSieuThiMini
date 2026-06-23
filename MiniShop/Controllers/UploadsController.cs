using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MiniShop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UploadsController : ControllerBase
    {
        [HttpPost("image")]
        [Authorize(Roles = "Admin,Cashier")] // Allow both or just Admin if only admins manage products. Let's allow both for safety.
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "Không tìm thấy file hợp lệ." });
            }

            // Validations (Optional but recommended)
            var extension = Path.GetExtension(file.FileName).ToLower();
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            if (Array.IndexOf(allowedExtensions, extension) < 0)
            {
                return BadRequest(new { message = "Chỉ hỗ trợ file ảnh (jpg, png, gif, webp)." });
            }

            if (file.Length > 5 * 1024 * 1024) // 5MB limit
            {
                return BadRequest(new { message = "File ảnh không được vượt quá 5MB." });
            }

            try
            {
                var folderName = Path.Combine("wwwroot", "uploads", "products");
                var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);

                if (!Directory.Exists(pathToSave))
                {
                    Directory.CreateDirectory(pathToSave);
                }

                var fileName = Guid.NewGuid().ToString() + extension;
                var fullPath = Path.Combine(pathToSave, fileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var dbPath = $"/uploads/products/{fileName}";

                return Ok(new { url = dbPath });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lưu file ảnh: " + ex.Message });
            }
        }
    }
}
