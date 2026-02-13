
using API.Interfaces;
using Microsoft.EntityFrameworkCore;
using Repositories.ApplicationDbContext;

namespace API.Background_Tasks
{
    public sealed class CartExpirationService : BackgroundService
    {
        private IServiceProvider _serviceProvider;
        private ILogger<CartExpirationService> _logger;

        public CartExpirationService(IServiceProvider serviceProvider, ILogger<CartExpirationService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Cart Expiration Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ExpireOldCartsAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error expiring carts");
                }

                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }

        async Task ExpireOldCartsAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var inventoryService = scope.ServiceProvider.GetRequiredService<IInventoryService>();

            var expirationTime = DateTime.UtcNow.AddMinutes(120);

            var expiredCartItems = await dbContext.CartItem.Where(ci => ci.Cart.UpdatedAt < expirationTime).ToListAsync();

            foreach (var item in expiredCartItems)
            {
                await inventoryService.ReleaseStockAsync(item.ProductVariantId, item.SizeId, item.Quantity);
                dbContext.CartItem.Remove(item);
            }

            await dbContext.SaveChangesAsync();
        }

    }
}
