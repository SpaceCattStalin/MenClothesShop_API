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
        public record GetCartItemDTO(int prodVarId, string prodVarName, int quantity, decimal total, decimal unitPrice, string imgUrl);
        public record ValidationIssue(int prodVarId, string prodVarName,
                        int cartQn,
                        int stockQn,
                        string issueType);
        public record ValidateCartResponse(
            bool isValid,
            List<ValidationIssue> issues
        );

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

            builder.MapPost("checkout", async (
                int userId,
                ICartService cartService,
                IOrderService orderService,
                IInventoryService inventoryService,
                PaymentService paymentService
            ) =>
            {
                try
                {
                    var order = await orderService.CreateOrderFromCartAsync(userId);
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
