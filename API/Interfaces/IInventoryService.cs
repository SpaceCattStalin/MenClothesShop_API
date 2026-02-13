using static API.Features.CartEndpoints;

namespace API.Interfaces
{
    public interface IInventoryService
    {
        Task<int> GetStockAsync(int variantId, int sizeId);
        Task<bool> ReserveStockAsync(int variantId, int sizeId, int quantity);
        Task ReleaseStockAsync(int variantId, int sizeId, int quantity);
        Task<List<ValidationIssue>> ValidateCartItemsAsync(int userId);

    }
}
