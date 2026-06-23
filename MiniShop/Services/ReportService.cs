using Microsoft.EntityFrameworkCore;
using MiniShop.Data;
using MiniShop.DTOs;
using MiniShop.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MiniShop.Services
{
    public class ReportService : IReportService
    {
        private readonly MiniShopDbContext _context;

        public ReportService(MiniShopDbContext context)
        {
            _context = context;
        }

        public async Task<RevenueReportResponse> GetRevenueReportAsync(DateTime startDate, DateTime endDate)
        {
            var orders = await _context.Orders
                .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate)
                .ToListAsync();

            var totalOrders = orders.Count;
            var totalRevenue = orders.Sum(o => o.TotalAmount);

            return new RevenueReportResponse
            {
                StartDate = startDate,
                EndDate = endDate,
                TotalOrders = totalOrders,
                TotalRevenue = totalRevenue
            };
        }

        public async Task<System.Collections.Generic.IEnumerable<ProductResponse>> GetLowStockProductsAsync()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.StockQuantity < 10)
                .Select(p => new ProductResponse
                {
                    Id = p.Id,
                    Name = p.Name,
                    Barcode = p.Barcode,
                    Price = p.Price,
                    OriginalPrice = p.OriginalPrice,
                    StockQuantity = p.StockQuantity,
                    Description = p.Description,
                    ImageUrl = p.ImageUrl,
                    Unit = p.Unit,
                    IsActive = p.IsActive,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category != null ? p.Category.Name : null
                }).ToListAsync();
            return products;
        }
    }
}
