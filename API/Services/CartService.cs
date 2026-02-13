using API.Interfaces;
using Common.Commons;
using Microsoft.EntityFrameworkCore;
using Repositories.ApplicationDbContext;
using Repositories.Models;
using static API.Features.CartEndpoints;

namespace API.Services
{
    public class CartService : ICartService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CartService> _logger;

        public CartService(AppDbContext context, ILogger<CartService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<GetCartItemDTO>> GetCartItemsAsync(int userId)
        {
            return await _context.CartItem
                .Where(ci => ci.Cart.UserId == userId)
                .Select(ci => new GetCartItemDTO(
                    ci.ProductVariantId,
                    ci.ProductVariant.MainProduct.Name + " " + ci.ProductVariant.Color.Name + " " + ci.Size.Name,
                    ci.Quantity,
                    ci.ProductVariant.MainProduct.Price * ci.Quantity,
                    ci.UnitPrice,
                    ci.ProductVariant.Images.FirstOrDefault().Url
                ))
                .ToListAsync();
        }

        public async Task<Cart> GetOrCreateCartAsync(int userId)
        {
            var cart = await _context.Cart.FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.Cart.Add(cart);
                await _context.SaveChangesAsync();
            }

            return cart;
        }

        public async Task<bool> AddToCartAsync(int userId, int variantId, int sizeId, int quantity)
        {
            var cart = await GetOrCreateCartAsync(userId);

            var existingItem = await _context.CartItem
                .FirstOrDefaultAsync(ci => ci.CartId == cart.Id
                                        && ci.ProductVariantId == variantId
                                        && ci.SizeId == sizeId);

            var variant = await _context.ProductVariants.Include(v => v.MainProduct).FirstOrDefaultAsync(v => v.Id.Equals(variantId));

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                var newItem = new CartItem
                {
                    CartId = cart.Id,
                    ProductVariantId = variantId,
                    SizeId = sizeId,
                    Quantity = quantity,
                    UnitPrice = variant.MainProduct.Price
                };
                _context.CartItem.Add(newItem);
            }

            var now = DateTime.UtcNow;
            var vietnamTime = Utils.UtcToLocalTimeZone(now);

            cart.UpdatedAt = vietnamTime;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task ClearCart(int userId)
        {
            var items = await _context.CartItem
                .Include(i => i.Cart)
                .Where(ci => ci.Cart.UserId.Equals(userId))
                .ToListAsync();

            if (items.Any())
            {
                _context.CartItem.RemoveRange(items);
                await _context.SaveChangesAsync();
            }
        }
    }
}