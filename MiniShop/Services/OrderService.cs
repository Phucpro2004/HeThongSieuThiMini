using Microsoft.EntityFrameworkCore;
using MiniShop.Data;
using MiniShop.DTOs;
using MiniShop.Interfaces;
using MiniShop.Models;
using System;
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
                decimal subTotal = 0;

                // Step 1 & 2: Loop CartItems, check stock, and calculate subtotal
                foreach (var item in request.CartItems)
                {
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product == null)
                    {
                        throw new Exception($"Product with ID {item.ProductId} not found.");
                    }

                    if (product.StockQuantity < item.Quantity)
                    {
                        throw new Exception($"Sản phẩm '{product.Name}' không đủ tồn kho.");
                    }

                    subTotal += product.Price * item.Quantity;
                }

                decimal totalAmount = subTotal - request.Discount;

                // Step 3: Create Order
                var order = new Order
                {
                    OrderCode = "ORD" + DateTime.UtcNow.Ticks.ToString().Substring(10),
                    SubTotal = subTotal,
                    Discount = request.Discount,
                    TotalAmount = totalAmount,
                    Status = "Paid",
                    CustomerId = request.CustomerId,
                    CashierId = cashierId
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync(); // save to get OrderId

                // Step 4 & 5: Create OrderDetails and Update Stock
                foreach (var item in request.CartItems)
                {
                    var product = await _context.Products.FindAsync(item.ProductId);
                    
                    var orderDetail = new OrderDetail
                    {
                        OrderId = order.Id,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = product!.Price,
                        SubTotal = product.Price * item.Quantity
                    };
                    
                    _context.OrderDetails.Add(orderDetail);
                    
                    // Deduct stock
                    product.StockQuantity -= item.Quantity;
                }

                // Step 6: Create Payment
                decimal changeAmount = request.AmountReceived - totalAmount;
                if (request.AmountReceived < totalAmount)
                {
                    throw new Exception("Khách đưa thiếu tiền.");
                }

                var payment = new Payment
                {
                    OrderId = order.Id,
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
