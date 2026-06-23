using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MiniShop.DTOs
{
    /// <summary>
    /// Request model for creating a goods receipt.
    /// Modified by Phuc.
    /// </summary>
    public class GoodsReceiptCreateRequest
    {
        [Required]
        public string SupplierName { get; set; } = string.Empty;

        public string? Note { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "Phải có ít nhất 1 sản phẩm để nhập kho.")]
        public List<GoodsReceiptItemDto> Items { get; set; } = new List<GoodsReceiptItemDto>();
    }

    public class GoodsReceiptItemDto
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng nhập phải lớn hơn 0.")]
        public int Quantity { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Đơn giá nhập không hợp lệ.")]
        public decimal UnitPrice { get; set; }
    }
}
