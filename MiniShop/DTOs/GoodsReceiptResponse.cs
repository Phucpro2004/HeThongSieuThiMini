using System;
using System.Collections.Generic;

namespace MiniShop.DTOs
{
    public class GoodsReceiptResponse
    {
        public int Id { get; set; }
        public string ReceiptCode { get; set; } = string.Empty;
        public string? SupplierName { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Note { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? UserId { get; set; }
        public string? UserName { get; set; }

        public List<GoodsReceiptDetailResponse> Details { get; set; } = new List<GoodsReceiptDetailResponse>();
    }

    public class GoodsReceiptDetailResponse
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal SubTotal { get; set; }
    }
}
