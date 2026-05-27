using System;
using System.ComponentModel.DataAnnotations;

namespace MiniShop.Models
{
    public class Payment
    {
        [Key]
        public int Id { get; set; }
        
        public decimal Amount { get; set; }
        public decimal AmountReceived { get; set; }
        public decimal ChangeAmount { get; set; }
        public string Method { get; set; } = "Cash";
        public string Status { get; set; } = "Paid";
        public string? TransactionRef { get; set; }
        public string? Note { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? PaidAt { get; set; } = DateTime.UtcNow;

        public int OrderId { get; set; }
        public Order? Order { get; set; }
    }
}
