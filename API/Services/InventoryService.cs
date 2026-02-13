using API.Interfaces;
using Common.Commons;
using Microsoft.EntityFrameworkCore;
using Repositories.ApplicationDbContext;
using static API.Features.CartEndpoints;

namespace API.Services
{
    namespace API.Services
    {
        public class InventoryService : IInventoryService
        {
            private readonly AppDbContext _context;
            private readonly ILogger<InventoryService> _logger;

            public InventoryService(AppDbContext context, ILogger<InventoryService> logger)
            {
                _context = context;
                _logger = logger;
            }

            //public async Task<int> GetStockAsync(int variantId, int sizeId)
            //{
            //    var stock = await _context.ProductSizes
            //        .Where(ps => ps.ProductVariantId == variantId && ps.SizeId == sizeId)
            //        .Select(ps => ps.Quantity)
            //        .FirstOrDefaultAsync();

            //    return stock;
            //}
            public async Task<int> GetStockAsync(int variantId, int sizeId)
            {
                var productSize = await _context.ProductSizes
                    .Where(ps => ps.ProductVariantId == variantId && ps.SizeId == sizeId)
                    .FirstOrDefaultAsync();

                Console.WriteLine($"[GetStock] Looking for VariantId={variantId}, SizeId={sizeId}");
                Console.WriteLine($"[GetStock] ProductSize found: {productSize != null}");

                if (productSize != null)
                {
                    Console.WriteLine($"[GetStock] Quantity: {productSize.Quantity}");
                    return productSize.Quantity;
                }

                Console.WriteLine($"[GetStock] No ProductSize found, returning 0");
                return 0;
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
            //public async Task<List<ValidationIssue>> ValidateCartItemsAsync(int userId)
            //{
            //    try
            //    {
            //        List<ValidationIssue> errors = new List<ValidationIssue>();

            //        //var items = await _context.CartItem
            //        //    .Where(ci => ci.Cart.UserId.Equals(userId))
            //        //    .ToListAsync();
            //        var items = await _context.CartItem
            //             .Include(ci => ci.ProductVariant)
            //                 .ThenInclude(pv => pv.MainProduct)
            //             .Include(ci => ci.ProductVariant)
            //                 .ThenInclude(pv => pv.Color)
            //             .Include(ci => ci.Size)
            //             .Include(ci => ci.Cart)
            //             .Where(ci => ci.Cart.UserId.Equals(userId))
            //             .ToListAsync();

            //        foreach (var item in items)
            //        {
            //            int stock = await GetStockAsync(item.ProductVariantId, item.SizeId);
            //            if (item.Quantity > stock)
            //            {
            //                errors.Add(new ValidationIssue(
            //                    item.ProductVariantId,
            //                    item.ProductVariant.MainProduct.Name + " " + item.ProductVariant.Color.Name + " " + item.Size.Name,
            //                    item.Quantity,
            //                    stock,
            //                    ErrorCode.InsufficientStock.ToString())
            //                );
            //            }
            //        }

            //        return errors;
            //    }
            //    catch (Exception ex)
            //    {
            //        throw;
            //    }
            //}
            public async Task<List<ValidationIssue>> ValidateCartItemsAsync(int userId)
            {
                List<ValidationIssue> errors = new List<ValidationIssue>();
                var items = await _context.CartItem
                    .Include(ci => ci.ProductVariant)
                        .ThenInclude(pv => pv.MainProduct)
                    .Include(ci => ci.ProductVariant)
                        .ThenInclude(pv => pv.Color)
                    .Include(ci => ci.Size)
                    .Include(ci => ci.Cart)
                    .Where(ci => ci.Cart.UserId.Equals(userId))
                    .ToListAsync();

                Console.WriteLine($"[ValidateCart] Found {items.Count} cart items for user {userId}");

                foreach (var item in items)
                {
                    Console.WriteLine($"[ValidateCart] Processing item: VariantId={item.ProductVariantId}, SizeId={item.SizeId}, Quantity={item.Quantity}");

                    int stock = await GetStockAsync(item.ProductVariantId, item.SizeId);

                    Console.WriteLine($"[ValidateCart] Stock for variant {item.ProductVariantId}, size {item.SizeId}: {stock}");
                    Console.WriteLine($"[ValidateCart] Comparison: {item.Quantity} > {stock} = {item.Quantity > stock}");

                    if (item.Quantity > stock)
                    {
                        errors.Add(new ValidationIssue(
                            item.ProductVariantId,
                            item.ProductVariant.MainProduct.Name + " " + item.ProductVariant.Color.Name + " " + item.Size.Name,
                            item.Quantity,
                            stock,
                            ErrorCode.InsufficientStock.ToString())
                        );
                    }
                }

                Console.WriteLine($"[ValidateCart] Total errors: {errors.Count}");
                return errors;
            }
        }
    }
}
