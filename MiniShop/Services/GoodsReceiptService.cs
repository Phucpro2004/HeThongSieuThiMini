using Microsoft.EntityFrameworkCore;
using MiniShop.Data;
using MiniShop.DTOs;
using MiniShop.Interfaces;
using MiniShop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MiniShop.Services
{
    public class GoodsReceiptService : IGoodsReceiptService
    {
        private readonly MiniShopDbContext _context;

        public GoodsReceiptService(MiniShopDbContext context)
        {
            _context = context;
        }

        public async Task<GoodsReceipt> CreateReceiptAsync(GoodsReceiptCreateRequest request, int userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var productIds = request.Items.Select(i => i.ProductId).ToList();
                var products = await _context.Products.Where(p => productIds.Contains(p.Id)).ToDictionaryAsync(p => p.Id);

                decimal totalAmount = 0;

                var receipt = new GoodsReceipt
                {
                    ReceiptCode = "PN" + DateTime.UtcNow.Ticks.ToString().Substring(10),
                    SupplierName = request.SupplierName,
                    Note = request.Note,
                    UserId = userId,
                    GoodsReceiptDetails = new List<GoodsReceiptDetail>()
                };

                foreach (var item in request.Items)
                {
                    if (!products.TryGetValue(item.ProductId, out var product))
                    {
                        throw new Exceptions.NotFoundException($"Sản phẩm với ID {item.ProductId} không tồn tại.");
                    }

                    var subTotal = item.Quantity * item.UnitPrice;
                    totalAmount += subTotal;

                    var detail = new GoodsReceiptDetail
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        SubTotal = subTotal
                    };

                    receipt.GoodsReceiptDetails.Add(detail);

                    // Cộng dồn tồn kho (Add up inventory)
                    product.StockQuantity += item.Quantity;
                }

                receipt.TotalAmount = totalAmount;

                _context.GoodsReceipts.Add(receipt);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return receipt;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<IEnumerable<GoodsReceiptResponse>> GetAllReceiptsAsync()
        {
            return await _context.GoodsReceipts
                .Include(gr => gr.User)
                .OrderByDescending(gr => gr.CreatedAt)
                .Select(gr => new GoodsReceiptResponse
                {
                    Id = gr.Id,
                    ReceiptCode = gr.ReceiptCode,
                    SupplierName = gr.SupplierName,
                    TotalAmount = gr.TotalAmount,
                    Note = gr.Note,
                    CreatedAt = gr.CreatedAt,
                    UserId = gr.UserId,
                    UserName = gr.User != null ? gr.User.FullName : null
                })
                .ToListAsync();
        }

        public async Task<GoodsReceiptResponse?> GetReceiptByIdAsync(int id)
        {
            var receipt = await _context.GoodsReceipts
                .Include(gr => gr.User)
                .Include(gr => gr.GoodsReceiptDetails)
                    .ThenInclude(d => d.Product)
                .FirstOrDefaultAsync(gr => gr.Id == id);

            if (receipt == null) return null;

            return new GoodsReceiptResponse
            {
                Id = receipt.Id,
                ReceiptCode = receipt.ReceiptCode,
                SupplierName = receipt.SupplierName,
                TotalAmount = receipt.TotalAmount,
                Note = receipt.Note,
                CreatedAt = receipt.CreatedAt,
                UserId = receipt.UserId,
                UserName = receipt.User?.FullName,
                Details = receipt.GoodsReceiptDetails.Select(d => new GoodsReceiptDetailResponse
                {
                    Id = d.Id,
                    ProductId = d.ProductId,
                    ProductName = d.Product?.Name,
                    Quantity = d.Quantity,
                    UnitPrice = d.UnitPrice,
                    SubTotal = d.SubTotal
                }).ToList()
            };
        }
    }
}
