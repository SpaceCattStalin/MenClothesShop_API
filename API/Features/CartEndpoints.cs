using API.Interfaces;
using API.Services;
using API.Services.API.Services;
using Common.Commons;
using MenClothesShop_API;
using Microsoft.EntityFrameworkCore;
using PayOS.Models.V2.PaymentRequests;
using PayOS.Models.Webhooks;
using Repositories.ApplicationDbContext;
using Repositories.Models;
using static API.Features.CartEndpoints;

namespace API.Features
{
    public static class CartEndpoints
    {
        public record AddToCartRequest(int variantId, string sizeName, int quantity);
        public record GetCartItemDTO(int id, int prodVarId, string prodVarName, int quantity, decimal total, decimal unitPrice, string imgUrl);
        public record UpdateCartItemRequest(int quantity);
        public record ValidationIssue(int prodVarId, string prodVarName,
                        int cartQn,
                        int stockQn,
                        string issueType);
        public record ValidateCartResponse(
            bool isValid,
            List<ValidationIssue> issues
        );

        /** Optional shipping address; if omitted, backend uses the user's saved address. */
        public record CheckoutRequest(string? ShippingAddress);

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

                    return ApiResponse.SuccessResult(HttpStatusCode.Created, "Added to cart successfully");
                }
                catch (Exception ex)
                {
                    return ApiResponse.ErrorResult(
                        $"Failed to add to cart: {ex.Message}",
                        HttpStatusCode.InternalServerError,
                        ErrorCode.InternalServerError);
                }
            });

            builder.MapPut("cart/item/{cartItemId}", async (AppDbContext db, int userId, int cartItemId, UpdateCartItemRequest request) =>
            {
                try
                {
                    if (request.quantity < 1)
                    {
                        return ApiResponse.ErrorResult("Quantity must be at least 1", HttpStatusCode.BadRequest, ErrorCode.ValidationError);
                    }

                    var cart = await db.Cart.FirstOrDefaultAsync(c => c.UserId == userId);
                    if (cart == null)
                    {
                        return ApiResponse.ErrorResult("Cart not found", HttpStatusCode.NotFound, ErrorCode.ResourceNotFound);
                    }

                    var item = await db.CartItem
                        .FirstOrDefaultAsync(ci => ci.Id == cartItemId && ci.CartId == cart.Id);
                    if (item == null)
                    {
                        return ApiResponse.ErrorResult("Cart item not found", HttpStatusCode.NotFound, ErrorCode.ResourceNotFound);
                    }

                    item.Quantity = request.quantity;
                    cart.UpdatedAt = DateTime.UtcNow;
                    await db.SaveChangesAsync();

                    return ApiResponse.SuccessResult(HttpStatusCode.Ok, "Cart item quantity updated");
                }
                catch (Exception ex)
                {
                    return ApiResponse.ErrorResult(
                        $"Failed to update cart item: {ex.Message}",
                        HttpStatusCode.InternalServerError,
                        ErrorCode.InternalServerError);
                }
            });

            builder.MapDelete("cart/item/{cartItemId}", async (AppDbContext db, int userId, int cartItemId) =>
            {
                try
                {
                    var cart = await db.Cart.FirstOrDefaultAsync(c => c.UserId == userId);
                    if (cart == null)
                    {
                        return ApiResponse.ErrorResult("Cart not found", HttpStatusCode.NotFound, ErrorCode.ResourceNotFound);
                    }

                    var item = await db.CartItem
                        .FirstOrDefaultAsync(ci => ci.Id == cartItemId && ci.CartId == cart.Id);
                    if (item == null)
                    {
                        return ApiResponse.ErrorResult("Cart item not found", HttpStatusCode.NotFound, ErrorCode.ResourceNotFound);
                    }

                    db.CartItem.Remove(item);
                    cart.UpdatedAt = DateTime.UtcNow;
                    await db.SaveChangesAsync();

                    return ApiResponse.SuccessResult(HttpStatusCode.Ok, "Cart item removed");
                }
                catch (Exception ex)
                {
                    return ApiResponse.ErrorResult(
                        $"Failed to remove cart item: {ex.Message}",
                        HttpStatusCode.InternalServerError,
                        ErrorCode.InternalServerError);
                }
            });

            builder.MapPost("checkout", async (
                int userId,
                CheckoutRequest request,
                AppDbContext db,
                ICartService cartService,
                IOrderService orderService,
                IInventoryService inventoryService,
                PaymentService paymentService
            ) =>
            {
                try
                {
                    var shippingAddress = !string.IsNullOrWhiteSpace(request?.ShippingAddress)
                        ? request.ShippingAddress.Trim()
                        : null;
                    if (string.IsNullOrEmpty(shippingAddress))
                    {
                        var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == userId);
                        shippingAddress = !string.IsNullOrWhiteSpace(user?.Address) ? user.Address.Trim() : null;
                    }
                    if (string.IsNullOrEmpty(shippingAddress))
                    {
                        return Results.Json(
                            ApiResponse.ErrorResult(
                                "Shipping address is required. Please set it in your profile or provide it at checkout.",
                                HttpStatusCode.BadRequest,
                                ErrorCode.ValidationError),
                            statusCode: 400);
                    }

                    var order = await orderService.CreateOrderFromCartAsync(userId, shippingAddress);
                    var paymentLink = await paymentService.CreatePaymentRequest(order);

                    return Results.Ok(ApiResponse<CreatePaymentLinkResponse>.SuccessResult(paymentLink));
                }
                catch (Exception ex)
                {
                    return Results.Json(ApiResponse.ErrorResult(
                         $"Failed to checkout: {ex.Message}",
                         HttpStatusCode.InternalServerError,
                         ErrorCode.InternalServerError));
                }
            });

            builder.MapPost("validate-cart", async (
                int userId,
                IInventoryService inventoryService,
                ILogger<Program> logger
            ) =>
            {
                try
                {
                    List<ValidationIssue> errors = await inventoryService.ValidateCartItemsAsync(userId);

                    if (errors.Count == 0)
                    {
                        return Results.Ok(ApiResponse.SuccessResult());
                    }
                    else
                    {
                        foreach (var error in errors)
                        {
                            logger.LogInformation("Cart validation error: {@Error}", error);
                        }
                        return Results.Conflict(
                            ApiResponse<List<ValidationIssue>>.SuccessResult(
                                errors,
                                HttpStatusCode.Conflict,
                                "Validation issues"
                            )
                        );
                    }
                }
                catch (Exception ex)
                {
                    return Results.Json(
                    ApiResponse.ErrorResult(
                         $"Failed to validate cart: {ex.Message}",
                         HttpStatusCode.InternalServerError,
                         ErrorCode.InternalServerError
                    ),
                    statusCode: 500);
                }
            });

            builder.MapPost("payos/webhook", async (PaymentService paymentService, HttpRequest request) =>
            {
                try
                {
                    using var reader = new StreamReader(request.Body);
                    var body = await reader.ReadToEndAsync();
                    var webhook = System.Text.Json.JsonSerializer.Deserialize<Webhook>(body);

                    Console.WriteLine(webhook);
                    var res = await paymentService.VerifyWebhook(webhook);

                    return Results.Ok(res);
                }
                catch (Exception ex)
                {
                    return Results.Json(
                    ApiResponse.ErrorResult(
                         $"{ex.Message}",
                         HttpStatusCode.InternalServerError,
                         ErrorCode.InternalServerError
                    ),
                    statusCode: 500);
                }
            });

            //builder.MapPost("payos/register-webhook", async (PaymentService paymentService) =>
            //{
            //    try
            //    {
            //        var res = await paymentService.RegisterWebhook();
            //        return Results.Ok(res);
            //    }
            //    catch (Exception ex)
            //    {
            //        return Results.Json(
            //        ApiResponse.ErrorResult(
            //             $"{ex.Message}",
            //             HttpStatusCode.InternalServerError,
            //             ErrorCode.InternalServerError
            //        ),
            //        statusCode: 500);
            //    }
            //});
        }
    }
}
