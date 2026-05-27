using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MiniShop.Models
{
    public class Customer
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string FullName { get; set; } = string.Empty;
        
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public int Points { get; set; } = 0;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
