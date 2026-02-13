using Repositories.Models;

namespace API.Interfaces
{
    public interface IOrderService
    {
        Task<Order> CreateOrderFromCartAsync(int userId);
    }
}
