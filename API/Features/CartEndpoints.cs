using API.Interfaces;
using Common.Commons;
using Microsoft.EntityFrameworkCore;
using Repositories.ApplicationDbContext;
using Repositories.Models;

namespace API.Features
{
    public static class CartEndpoints
    {
        public record AddToCartRequest(int variantId, string sizeName, int quantity);
        public record GetCartItemDTO(int prodVarId, string prodVarName, int quantity, decimal total, string imgUrl);

        public static void MapEndpoint(IEndpointRouteBuilder builder)
        {
            builder.MapGet("cart/{userId}", async (ICartService cartService, int userId) =>
            {
                try
                {
                    var items = await cartService.GetCartItemsAsync(userId);
                    return ApiResponse<List<GetCartItemDTO>>.SuccessResult(items);
                }
                catch (Exception e)
                {
                    return ApiResponse.ErrorResult(
                        $"Failed to fetch cart items: {e.Message}",
                        HttpStatusCode.InternalServerError,
                        ErrorCode.InternalServerError);
                }
            });

            builder.MapPost("cart/add", async (
                ICartService cartService,
                IInventoryService inventoryService,
                ISizeService sizeService,
                AddToCartRequest request,
                int userId) =>
            {
                try
                {
                    var size = await sizeService.GetSizeByName(request.sizeName);
                    if (size != null)
                    {
                        // Check stock
                        var availableStock = await inventoryService.GetStockAsync(request.variantId, size.Id);
                        if (availableStock < request.quantity)
                        {
                            return ApiResponse.ErrorResult(
                                $"Only {availableStock} items available",
                                HttpStatusCode.BadRequest,
                                ErrorCode.InsufficientStock);
                        }

                        // Reserve stock
                        var stockReserved = await inventoryService.ReserveStockAsync(
                            request.variantId,
                            size.Id,
                            request.quantity);

                        if (!stockReserved)
                        {
                            return ApiResponse.ErrorResult(
                                "Failed to reserve stock",
                                HttpStatusCode.InternalServerError,
                                ErrorCode.InternalServerError);
                        }

                        // Add to cart
                        var added = await cartService.AddToCartAsync(
                            userId,
                            request.variantId,
                            size.Id,
                            request.quantity);

                        if (!added)
                        {
                            await inventoryService.ReleaseStockAsync(
                                request.variantId,
                                size.Id,
                                request.quantity);

                            return ApiResponse.ErrorResult(
                                "Failed to add to cart",
                                HttpStatusCode.InternalServerError,
                                ErrorCode.InternalServerError);
                        }
                    }

                    return ApiResponse.SuccessResult(HttpStatusCode.Ok, "Added to cart successfully");
                }
                catch (Exception ex)
                {
                    return ApiResponse.ErrorResult(
                        $"Failed to add to cart: {ex.Message}",
                        HttpStatusCode.InternalServerError,
                        ErrorCode.InternalServerError);
                }
            });
        }
    }
}
