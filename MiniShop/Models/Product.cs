using System;
using System.ComponentModel.DataAnnotations;

namespace MiniShop.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; } = string.Empty;
        
        public string? Barcode { get; set; }
        public string? Description { get; set; }
        
        public decimal Price { get; set; }
        public decimal OriginalPrice { get; set; }
        public int StockQuantity { get; set; }
        public string? ImageUrl { get; set; }
        public string? Unit { get; set; }
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public int CategoryId { get; set; }
        public Category? Category { get; set; }
    }
}
