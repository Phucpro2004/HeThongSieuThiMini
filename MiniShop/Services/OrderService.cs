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
    }
}
