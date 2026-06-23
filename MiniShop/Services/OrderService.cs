using Microsoft.EntityFrameworkCore;
using MiniShop.Data;
using MiniShop.DTOs;
using MiniShop.Interfaces;
using MiniShop.Models;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MiniShop.Services
{
    public class OrderService : IOrderService
    {
        private readonly MiniShopDbContext _context;

        public OrderService(MiniShopDbContext context)
        {
            _context = context;
        }

        public async Task<Order> CheckoutAsync(CheckoutRequest request, int cashierId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var productIds = request.CartItems.Select(ci => ci.ProductId).ToList();
                var products = await _context.Products.Where(p => productIds.Contains(p.Id)).ToDictionaryAsync(p => p.Id);

                decimal subTotal = 0;

                // Step 1 & 2: Loop CartItems, check stock, and calculate subtotal
                foreach (var item in request.CartItems)
                {
                    if (!products.TryGetValue(item.ProductId, out var product))
                    {
                        throw new Exceptions.NotFoundException($"Product with ID {item.ProductId} not found.");
                    }

                    if (product.StockQuantity < item.Quantity)
                    {
                        throw new Exceptions.BadRequestException($"Sản phẩm '{product.Name}' không đủ tồn kho.");
                    }

                    subTotal += product.Price * item.Quantity;
                }

                decimal totalAmount = subTotal - request.Discount;

                decimal changeAmount = request.AmountReceived - totalAmount;
                if (request.AmountReceived < totalAmount)
                {
                    throw new Exceptions.BadRequestException("Khách đưa thiếu tiền.");
                }

                // Step 3: Create Order
                var order = new Order
                {
                    OrderCode = "ORD" + DateTime.UtcNow.Ticks.ToString().Substring(10),
                    SubTotal = subTotal,
                    Discount = request.Discount,
                    TotalAmount = totalAmount,
                    Status = "Paid",
                    CustomerId = request.CustomerId,
                    CashierId = cashierId,
                    OrderDetails = new List<OrderDetail>()
                };

                // Add points to customer if CustomerId is provided
                if (request.CustomerId.HasValue)
                {
                    var customer = await _context.Customers.FindAsync(request.CustomerId.Value);
                    if (customer != null)
                    {
                        int earnedPoints = (int)(totalAmount / 10000);
                        customer.Points += earnedPoints;
                    }
                }

                // Step 4 & 5: Create OrderDetails and Update Stock
                foreach (var item in request.CartItems)
                {
                    var product = products[item.ProductId];
                    
                    var orderDetail = new OrderDetail
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = product.Price,
                        SubTotal = product.Price * item.Quantity
                    };
                    
                    order.OrderDetails.Add(orderDetail);
                    
                    // Deduct stock
                    product.StockQuantity -= item.Quantity;
                }

                _context.Orders.Add(order);

                // Step 6: Create Payment
                var payment = new Payment
                {
                    Order = order,
                    Amount = totalAmount,
                    AmountReceived = request.AmountReceived,
                    ChangeAmount = changeAmount,
                    Method = request.PaymentMethod,
                    Status = "Paid"
                };

                _context.Payments.Add(payment);

                // Step 7: Save and Commit
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return order;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<IEnumerable<OrderResponse>> GetAllOrdersAsync()
        {
            var orders = await _context.Orders
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => new OrderResponse
                {
                    Id = o.Id,
                    OrderCode = o.OrderCode,
                    SubTotal = o.SubTotal,
                    Discount = o.Discount,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status,
                    Note = o.Note,
                    CreatedAt = o.CreatedAt,
                    CustomerId = o.CustomerId,
                    CashierId = o.CashierId
                }).ToListAsync();
            return orders;
        }

        public async Task<OrderResponse?> GetOrderByIdAsync(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return null;

            return new OrderResponse
            {
                Id = order.Id,
                OrderCode = order.OrderCode,
                SubTotal = order.SubTotal,
                Discount = order.Discount,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                Note = order.Note,
                CreatedAt = order.CreatedAt,
                CustomerId = order.CustomerId,
                CashierId = order.CashierId,
                OrderDetails = order.OrderDetails.Select(od => new OrderDetailResponse
                {
                    Id = od.Id,
                    ProductId = od.ProductId,
                    ProductName = od.Product?.Name,
                    Quantity = od.Quantity,
                    UnitPrice = od.UnitPrice,
                    SubTotal = od.SubTotal
                }).ToList()
            };
        }
    }
}
