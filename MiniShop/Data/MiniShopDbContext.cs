using Microsoft.EntityFrameworkCore;
using MiniShop.Models;
using System.Linq;

namespace MiniShop.Data
{
    public class MiniShopDbContext : DbContext
    {
        public MiniShopDbContext(DbContextOptions<MiniShopDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Payment> Payments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Relationships
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Customer)
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.CustomerId)
                .IsRequired(false);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Cashier)
                .WithMany()
                .HasForeignKey(o => o.CashierId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.Order)
                .WithMany(o => o.OrderDetails)
                .HasForeignKey(od => od.OrderId);

            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.Product)
                .WithMany()
                .HasForeignKey(od => od.ProductId);

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Order)
                .WithMany(o => o.Payments)
                .HasForeignKey(p => p.OrderId);

            // Precision for decimals
            var decimalProps = new[] {
                typeof(Product).GetProperty("Price"),
                typeof(Product).GetProperty("OriginalPrice"),
                typeof(Order).GetProperty("SubTotal"),
                typeof(Order).GetProperty("Discount"),
                typeof(Order).GetProperty("TotalAmount"),
                typeof(OrderDetail).GetProperty("UnitPrice"),
                typeof(OrderDetail).GetProperty("SubTotal"),
                typeof(Payment).GetProperty("Amount"),
                typeof(Payment).GetProperty("AmountReceived"),
                typeof(Payment).GetProperty("ChangeAmount")
            };

            foreach (var prop in decimalProps)
            {
                if (prop != null && prop.DeclaringType != null)
                {
                    modelBuilder.Entity(prop.DeclaringType)
                        .Property(prop.Name)
                        .HasColumnType("decimal(18,2)");
                }
            }
        }

        public void Seed()
        {
            if (!Users.Any())
            {
                var admin = new User
                {
                    FullName = "Administrator",
                    Email = "admin@minishop.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("123"),
                    Role = "Admin"
                };
                Users.Add(admin);
                SaveChanges();
            }
        }
    }
}
