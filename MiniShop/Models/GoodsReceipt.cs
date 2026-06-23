using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MiniShop.Models
{
    public class GoodsReceipt
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string ReceiptCode { get; set; } = string.Empty;

        public string? SupplierName { get; set; }

        public decimal TotalAmount { get; set; }

        public string? Note { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int? UserId { get; set; }
        public User? User { get; set; }

        public ICollection<GoodsReceiptDetail> GoodsReceiptDetails { get; set; } = new List<GoodsReceiptDetail>();
    }
}
