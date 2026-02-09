using API.Interfaces;
using Microsoft.EntityFrameworkCore;
using Repositories.ApplicationDbContext;
using Repositories.Models;
using static API.Features.CartEndpoints;

namespace API.Services
{
    public class CartService : ICartService
    {
        private readonly AppDbContext _context;

        public CartService(AppDbContext context)
        {
            _context = context;
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
                    Quantity = quantity
                };
                _context.CartItem.Add(newItem);
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}