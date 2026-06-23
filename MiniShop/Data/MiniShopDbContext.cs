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
        public DbSet<GoodsReceipt> GoodsReceipts { get; set; }
        public DbSet<GoodsReceiptDetail> GoodsReceiptDetails { get; set; }

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

            modelBuilder.Entity<GoodsReceipt>()
                .HasOne(gr => gr.User)
                .WithMany()
                .HasForeignKey(gr => gr.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<GoodsReceiptDetail>()
                .HasOne(grd => grd.GoodsReceipt)
                .WithMany(gr => gr.GoodsReceiptDetails)
                .HasForeignKey(grd => grd.GoodsReceiptId);

            modelBuilder.Entity<GoodsReceiptDetail>()
                .HasOne(grd => grd.Product)
                .WithMany()
                .HasForeignKey(grd => grd.ProductId);

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
                typeof(Payment).GetProperty("ChangeAmount"),
                typeof(GoodsReceipt).GetProperty("TotalAmount"),
                typeof(GoodsReceiptDetail).GetProperty("UnitPrice"),
                typeof(GoodsReceiptDetail).GetProperty("SubTotal")
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

            // 1. Seed Users (Admin & Cashiers)
            var adminUser = Users.FirstOrDefault(u => u.Email == "admin@minishop.com");
            if (adminUser == null)
            {
                Users.Add(new User { FullName = "Admin", Email = "admin@minishop.com", PasswordHash = passHash, Role = "Admin", IsActive = true });
            }
            else
            {
                adminUser.PasswordHash = passHash;
                adminUser.IsActive = true;
            }
            if (!Users.Any(u => u.Email == "CN01@minishop.com"))
            {
                Users.Add(new User { FullName = "Thu Ngân 01", Email = "CN01@minishop.com", PasswordHash = passHash, Role = "Cashier", IsActive = true });
            }
            if (!Users.Any(u => u.Email == "CN02@minishop.com"))
            {
                Users.Add(new User { FullName = "Thu Ngân 02", Email = "CN02@minishop.com", PasswordHash = passHash, Role = "Cashier", IsActive = true });
            }
            SaveChanges();

            // 2. Seed Customers
            if (!Customers.Any())
            {
                Customers.AddRange(
                    new Customer { FullName = "Khách Lẻ", Phone = "0000000000", Email = "", Address = "", Points = 0 },
                    new Customer { FullName = "Nguyễn Văn A", Phone = "0901234567", Email = "nva@gmail.com", Address = "123 Lê Lợi, Q1, TP.HCM", Points = 150 },
                    new Customer { FullName = "Trần Thị B", Phone = "0912345678", Email = "ttb@gmail.com", Address = "456 Nguyễn Trãi, Q5, TP.HCM", Points = 50 }
                );
                SaveChanges();
            }

            // 3. Seed Categories
            if (!Categories.Any())
            {
                Categories.AddRange(
                    new Category { Name = "Thực phẩm tươi sống", Description = "Thịt, cá, rau củ quả tươi" },
                    new Category { Name = "Đồ uống", Description = "Nước giải khát, bia, sữa" },
                    new Category { Name = "Đồ ăn vặt", Description = "Bánh kẹo, snack" },
                    new Category { Name = "Gia vị", Description = "Nước mắm, xì dầu, muối, đường" },
                    new Category { Name = "Hóa mỹ phẩm", Description = "Bột giặt, dầu gội, sữa tắm" }
                );
                SaveChanges();
            }

            // 4. Seed Products
            if (!Products.Any())
            {
                var catThucPham = Categories.First(c => c.Name == "Thực phẩm tươi sống").Id;
                var catDoUong = Categories.First(c => c.Name == "Đồ uống").Id;
                var catDoAnVat = Categories.First(c => c.Name == "Đồ ăn vặt").Id;
                var catGiaVi = Categories.First(c => c.Name == "Gia vị").Id;
                var catHoaMyPham = Categories.First(c => c.Name == "Hóa mỹ phẩm").Id;

                Products.AddRange(
                    // Đồ uống
                    new Product { CategoryId = catDoUong, Name = "Coca Cola 320ml", Barcode = "8935049500544", Price = 10000, OriginalPrice = 8000, StockQuantity = 100, Unit = "Lon", ImageUrl = "https://images.unsplash.com/photo-1622483767028-3f66f32aef97?w=500&q=80" },
                    new Product { CategoryId = catDoUong, Name = "Pepsi 320ml", Barcode = "8935049501541", Price = 10000, OriginalPrice = 8000, StockQuantity = 100, Unit = "Lon", ImageUrl = "https://images.unsplash.com/photo-1629203851122-3726ecdf080e?w=500&q=80" },
                    new Product { CategoryId = catDoUong, Name = "Sữa tươi TH True Milk", Barcode = "8935049502123", Price = 35000, OriginalPrice = 30000, StockQuantity = 50, Unit = "Lốc", ImageUrl = "https://images.unsplash.com/photo-1550583724-b2692b85b150?w=500&q=80" },
                    new Product { CategoryId = catDoUong, Name = "Nước suối Aquafina 500ml", Barcode = "8935049503456", Price = 5000, OriginalPrice = 3500, StockQuantity = 200, Unit = "Chai", ImageUrl = "https://images.unsplash.com/photo-1548839140-29a749e1bc4e?w=500&q=80" },
                    
                    // Thực phẩm tươi sống
                    new Product { CategoryId = catThucPham, Name = "Thịt ba rọi heo (500g)", Barcode = "8935049511111", Price = 75000, OriginalPrice = 60000, StockQuantity = 20, Unit = "Khay", ImageUrl = "https://images.unsplash.com/photo-1603048297172-c92544798d5e?w=500&q=80" },
                    new Product { CategoryId = catThucPham, Name = "Táo Mỹ (1kg)", Barcode = "8935049512222", Price = 80000, OriginalPrice = 65000, StockQuantity = 30, Unit = "Kg", ImageUrl = "https://images.unsplash.com/photo-1560806887-1e4cd0b6faa6?w=500&q=80" },
                    new Product { CategoryId = catThucPham, Name = "Rau cải thìa (500g)", Barcode = "8935049513333", Price = 15000, OriginalPrice = 10000, StockQuantity = 40, Unit = "Bó", ImageUrl = "https://images.unsplash.com/photo-1598514982205-f36b96d1e8d4?w=500&q=80" },

                    // Đồ ăn vặt
                    new Product { CategoryId = catDoAnVat, Name = "Bánh Oreo", Barcode = "8935049521111", Price = 18000, OriginalPrice = 14000, StockQuantity = 60, Unit = "Cây", ImageUrl = "https://images.unsplash.com/photo-1558961363-fa8fdf82db35?w=500&q=80" },
                    new Product { CategoryId = catDoAnVat, Name = "Snack Khoai Tây Lay's", Barcode = "8935049522222", Price = 12000, OriginalPrice = 9000, StockQuantity = 80, Unit = "Gói", ImageUrl = "https://images.unsplash.com/photo-1566478989037-e924e50b0a1c?w=500&q=80" },
                    new Product { CategoryId = catDoAnVat, Name = "Mì Hảo Hảo chua cay", Barcode = "8935049523333", Price = 4500, OriginalPrice = 3500, StockQuantity = 500, Unit = "Gói", ImageUrl = "https://plus.unsplash.com/premium_photo-1664472637341-3ec829d1f4df?w=500&q=80" },

                    // Gia vị
                    new Product { CategoryId = catGiaVi, Name = "Nước mắm Nam Ngư", Barcode = "8935049531111", Price = 45000, OriginalPrice = 38000, StockQuantity = 40, Unit = "Chai", ImageUrl = "https://images.unsplash.com/photo-1599557456720-91a133d59648?w=500&q=80" },
                    new Product { CategoryId = catGiaVi, Name = "Đường tinh luyện Biên Hòa (1kg)", Barcode = "8935049532222", Price = 28000, OriginalPrice = 24000, StockQuantity = 100, Unit = "Gói", ImageUrl = "https://images.unsplash.com/photo-1626814026160-2237a95fc5a0?w=500&q=80" },
                    new Product { CategoryId = catGiaVi, Name = "Muối i-ốt (500g)", Barcode = "8935049533333", Price = 6000, OriginalPrice = 4500, StockQuantity = 150, Unit = "Gói", ImageUrl = "https://images.unsplash.com/photo-1624467004652-32b0f4dc7bc3?w=500&q=80" },

                    // Hóa mỹ phẩm
                    new Product { CategoryId = catHoaMyPham, Name = "Bột giặt OMO (1.2kg)", Barcode = "8935049541111", Price = 65000, OriginalPrice = 55000, StockQuantity = 30, Unit = "Túi", ImageUrl = "https://images.unsplash.com/photo-1584824388143-69020473ce17?w=500&q=80" },
                    new Product { CategoryId = catHoaMyPham, Name = "Dầu gội Clear (630g)", Barcode = "8935049542222", Price = 135000, OriginalPrice = 115000, StockQuantity = 25, Unit = "Chai", ImageUrl = "https://images.unsplash.com/photo-1526947425960-945c6e72858f?w=500&q=80" },
                    new Product { CategoryId = catHoaMyPham, Name = "Kem đánh răng P/S", Barcode = "8935049543333", Price = 38000, OriginalPrice = 30000, StockQuantity = 45, Unit = "Tuýp", ImageUrl = "https://images.unsplash.com/photo-1559598467-f8b76c8155d0?w=500&q=80" }
                );
                SaveChanges();
            }
        }
    }
}
