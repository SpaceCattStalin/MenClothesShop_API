using Common.Commons;
using Microsoft.EntityFrameworkCore;
using Repositories.ApplicationDbContext;
using Repositories.Models;

namespace API.Features
{
    public static class AdminEndpoints
    {
        public record UserDto(int UserId, string UserName, string Address, string Role);
        public record PaymentDto(DateTime Created, string status);
        public record OrderDto(int Id, int UserId, string UserName, decimal Total, DateTime Created, string To, PaymentDto payment);

        public static void MapEndpoint(IEndpointRouteBuilder builder)
        {
            builder.MapGet("admin/users", async (AppDbContext context) =>
            {
                try
                {
                    var list = await context.Users
                        .Select(u => new UserDto(u.UserId, u.UserName, u.Address ?? "", u.Role))
                        .ToListAsync();
                    list.RemoveAt(0);
                    return ApiResponse<List<UserDto>>.SuccessResult(list);
                }
                catch (Exception)
                {
                    return ApiResponse.ErrorResult("Failed to fetch users", HttpStatusCode.InternalServerError, ErrorCode.InternalServerError);
                }
            });

            builder.MapGet("admin/orders", async (AppDbContext context) =>
            {
                try
                {
                    var list = await context.Orders
                        .Include(o => o.User)
                        .Include(o => o.Payment)
                        .OrderByDescending(o => o.Created)
                        .Select(o => new OrderDto(o.Id, o.UserId, o.User.UserName, o.Total, o.Created, o.To ?? "", new PaymentDto(o.Payment.Created, o.Payment.PaymentType.ToString())))
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
