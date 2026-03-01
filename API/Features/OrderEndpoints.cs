using Common.Commons;
using Microsoft.EntityFrameworkCore;
using Repositories.ApplicationDbContext;

namespace API.Features
{
    public static class OrderEndpoints
    {
        public record PaymentDto(DateTime Created, string Status);
        public record OrderDto(int Id, int UserId, string UserName, decimal Total, DateTime Created, string To, PaymentDto Payment);

        public static void MapEndpoint(IEndpointRouteBuilder builder)
        {
            builder.MapGet("user/{userId}/orders", async (AppDbContext context, int userId) =>
            {
                try
                {
                    var list = await context.Orders
                        .Include(o => o.User)
                        .Include(o => o.Payment)
                        .Where(o => o.UserId == userId)
                        .OrderByDescending(o => o.Created)
                        .Select(o => new OrderDto(
                            o.Id,
                            o.UserId,
                            o.User.UserName,
                            o.Total,
                            o.Created,
                            o.To ?? "",
                            new PaymentDto(o.Payment.Created, o.Payment.PaymentType.ToString())))
                        .ToListAsync();
                    return ApiResponse<List<OrderDto>>.SuccessResult(list);
                }
                catch (Exception)
                {
                    return ApiResponse.ErrorResult("Failed to fetch orders", HttpStatusCode.InternalServerError, ErrorCode.InternalServerError);
                }
            });
        }
    }
}
