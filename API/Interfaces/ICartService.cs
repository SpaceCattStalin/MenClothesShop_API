using Repositories.Models;
using static API.Features.CartEndpoints;

namespace API.Interfaces
{
    public interface ICartService
    {
        Task<List<GetCartItemDTO>> GetCartItemsAsync(int userId);
        Task<bool> AddToCartAsync(int userId, int variantId, int sizeId, int quantity);
        Task<Cart> GetOrCreateCartAsync(int userId);
    }
}
