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

        //phuc da them sua 
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
            var passHash = BCrypt.Net.BCrypt.HashPassword("ToilaADMIN123");

            // 1. Delete existing admin to be totally fresh
            var oldAdmin = Users.FirstOrDefault(u => u.Email == "admin@minishop.com");
            if (oldAdmin != null)
            {
                Users.Remove(oldAdmin);
                SaveChanges();
            }

            // 2. Add new Admin
            Users.Add(new User 
            { 
                FullName = "Admin", 
                Email = "admin@minishop.com", 
                PasswordHash = passHash, 
                Role = "Admin", 
                IsActive = true 
            });

            // 3. Add CN01
            var c1 = Users.FirstOrDefault(u => u.Email == "CN01@minishop.com");
            if (c1 == null)
            {
                Users.Add(new User { FullName = "Thu Ngân 01", Email = "CN01@minishop.com", PasswordHash = passHash, Role = "Cashier", IsActive = true });
            }

            // 4. Add CN02
            var c2 = Users.FirstOrDefault(u => u.Email == "CN02@minishop.com");
            if (c2 == null)
            {
                Users.Add(new User { FullName = "Thu Ngân 02", Email = "CN02@minishop.com", PasswordHash = passHash, Role = "Cashier", IsActive = true });
            }

            SaveChanges();
        }
    }
}
