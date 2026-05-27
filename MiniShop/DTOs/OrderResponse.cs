using System;
using System.Collections.Generic;

namespace MiniShop.DTOs
{
    public class OrderResponse
    {
        public int Id { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public decimal SubTotal { get; set; }
        public decimal Discount { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Note { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? CustomerId { get; set; }
        public int CashierId { get; set; }
        public List<OrderDetailResponse> OrderDetails { get; set; } = new List<OrderDetailResponse>();
    }
}
