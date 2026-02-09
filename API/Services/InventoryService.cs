using API.Interfaces;
using Microsoft.EntityFrameworkCore;
using Repositories.ApplicationDbContext;

namespace API.Services
{
    namespace API.Services
    {
        public class InventoryService : IInventoryService
        {
            private readonly AppDbContext _context;

            public InventoryService(AppDbContext context)
            {
                _context = context;
            }

            public async Task<int> GetStockAsync(int variantId, int sizeId)
            {
                var stock = await _context.ProductSizes
                    .Where(ps => ps.ProductVariantId == variantId && ps.SizeId == sizeId)
                    .Select(ps => ps.Quantity)
                    .FirstOrDefaultAsync();

                return stock;
            }

            public async Task<bool> ReserveStockAsync(int variantId, int sizeId, int quantity)
            {
                var stockItem = await _context.ProductSizes
                    .FirstOrDefaultAsync(ps => ps.ProductVariantId == variantId && ps.SizeId == sizeId);

                if (stockItem == null || stockItem.Quantity < quantity)
                {
                    return false;
                }

                stockItem.Quantity -= quantity;
                await _context.SaveChangesAsync();
                return true;
            }

            public async Task ReleaseStockAsync(int variantId, int sizeId, int quantity)
            {
                var stockItem = await _context.ProductSizes
                    .FirstOrDefaultAsync(ps => ps.ProductVariantId == variantId && ps.SizeId == sizeId);

                if (stockItem != null)
                {
                    stockItem.Quantity += quantity;
                    await _context.SaveChangesAsync();
                }
            }
        }
    }
}
