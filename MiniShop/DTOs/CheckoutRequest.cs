using System.Collections.Generic;

namespace MiniShop.DTOs
{
    public class CheckoutRequest
    {
        public int? CustomerId { get; set; }
        public decimal Discount { get; set; }
        public decimal AmountReceived { get; set; }
        public string PaymentMethod { get; set; } = "Cash";
        
        public List<CartItemDto> CartItems { get; set; } = new List<CartItemDto>();
    }
}
