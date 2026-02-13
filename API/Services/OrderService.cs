using API.Interfaces;
using Common.Commons;
using Microsoft.EntityFrameworkCore;
using Repositories.ApplicationDbContext;
using Repositories.Models;
using System.Collections.ObjectModel;

namespace API.Services
{
    public class OrderService : IOrderService
    {
        private readonly AppDbContext _context;
        public OrderService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Order> CreateOrderFromCartAsync(int userId)
        {
            try
            {
                var cart = await _context.Cart
                    .Include(c => c.Items)
                    .FirstOrDefaultAsync(c => c.UserId.Equals(userId));

                var now = DateTime.UtcNow;
                var vietnamTime = Utils.UtcToLocalTimeZone(now);

                var order = new Order
                {
                    UserId = userId,
                    Total = cart.Items.Aggregate(0m, (total, item) => total + item.UnitPrice * item.Quantity),
                    Created = vietnamTime,
                    OrderItems = new Collection<OrderItem>()
                };

                foreach (var item in cart.Items)
                {
                    order.OrderItems.Add(new OrderItem
                    {
                        Quantity = item.Quantity,
                        ProductVariantId = item.ProductVariantId,
                        SizeId = item.SizeId,
                        UnitPrice = item.UnitPrice,
                        Total = item.UnitPrice * item.Quantity
                    });
                }

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                return order;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
