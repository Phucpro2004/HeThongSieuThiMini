using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniShop.Interfaces;
using System;
using System.Threading.Tasks;

namespace MiniShop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportsController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet("daily-revenue")]
        public async Task<IActionResult> GetRevenueReport([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            if (startDate > endDate)
            {
                return BadRequest(new { message = "Start date cannot be greater than end date." });
            }

            var report = await _reportService.GetRevenueReportAsync(startDate, endDate);
            return Ok(report);
        }

        [HttpGet("low-stock")]
        public async Task<IActionResult> GetLowStock()
        {
            var products = await _reportService.GetLowStockProductsAsync();
            return Ok(products);
        }
    }
}
