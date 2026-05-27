using MiniShop.DTOs;
using System;
using System.Threading.Tasks;

namespace MiniShop.Interfaces
{
    public interface IReportService
    {
        Task<RevenueReportResponse> GetRevenueReportAsync(DateTime startDate, DateTime endDate);
    }
}
