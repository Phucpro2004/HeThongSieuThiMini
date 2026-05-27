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
    }
}
