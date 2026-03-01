using Common.Commons;
using Microsoft.EntityFrameworkCore;
using Repositories.ApplicationDbContext;
using Repositories.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace API.Features
{
    public static class ProductByCategoryEndpoint
    {
        public record GetProductsByCategoryRequest(int id);
        public record ProductPreviewDTO(
            int ProductId,
            string ProductName,
            decimal Price,
            List<string> ColorName,
            string ImageUrl
        );

        //public record GetProductsByCategoryResponse(List<ProductPreviewDTO> );
        public static void MapEndpoints(IEndpointRouteBuilder builder)
        {
            builder.MapGet("category/{categoryId}/products", async (AppDbContext context, int categoryId) =>
            {
                try
                {
                    var res = await context.Products
                    .Where(x => x.CatId.Equals(categoryId))
                    .Select(x =>
                        new ProductPreviewDTO
                        (
                            x.Id,
                            x.Name,
                            x.Price,
                            x.Variants.Select(v => v.Color.HexCode).ToList(),
                            x.Variants.FirstOrDefault().Images.FirstOrDefault().Url
                        ))
                    .ToListAsync();

                    return ApiResponse<List<ProductPreviewDTO>>.SuccessResult(res);
                }
                catch (Exception ex)
                {
                    return ApiResponse.ErrorResult("Fail to fetch products in category", HttpStatusCode.InternalServerError, ErrorCode.InternalServerError);
                }
            });

            builder.MapGet("products", async (AppDbContext context) =>
            {
                try
                {
                    var products = await context.Products
                        .Include(p => p.Category)
                        .Include(p => p.Variants)
                            .ThenInclude(v => v.Images)
                        .Include(p => p.Variants)
                            .ThenInclude(v => v.Sizes)
                        .AsNoTracking()
                        .ToListAsync();

                    var res = products.Select(p => new
                    {
                        p.Id,
                        p.Name,
                        p.Price,
                        p.CatId,
                        CategoryName = p.Category?.Name ?? "",
                        ImageUrl = p.Variants?
                            .SelectMany(v => v.Images)
                            .Select(i => i.Url)
                            .FirstOrDefault(),
                        TotalQuantity = p.Variants?
                            .SelectMany(v => v.Sizes ?? Enumerable.Empty<ProductSize>())
                            .Sum(s => s.Quantity) ?? 0
                    }).ToList();

                    return ApiResponse<object>.SuccessResult(res);
                }
                catch (Exception)
                {
                    return ApiResponse.ErrorResult("Fail to fetch all products", HttpStatusCode.InternalServerError, ErrorCode.InternalServerError);
                }
            });

            builder.MapGet("products/{productId}/variants", async (AppDbContext context, int productId) =>
            {
                try
                {
                    var product = await context.Products
                        .Include(p => p.Variants)
                            .ThenInclude(v => v.Color)
                        .Include(p => p.Variants)
                            .ThenInclude(v => v.Sizes)
                                .ThenInclude(s => s.Size)
                        .AsNoTracking()
                        .FirstOrDefaultAsync(p => p.Id == productId);
                    if (product == null)
                        return ApiResponse.ErrorResult("Product not found", HttpStatusCode.NotFound, ErrorCode.ResourceNotFound);

                    var variants = product.Variants.Select(v => new
                    {
                        VariantId = v.Id,
                        ColorName = v.Color?.Name,
                        ColorHex = v.Color?.HexCode,
                        TotalQuantity = v.Sizes?.Sum(s => s.Quantity),
                        Sizes = (v.Sizes ?? Array.Empty<Repositories.Models.ProductSize>())
                            .Select(s => new { SizeName = s.Size?.Name, Quantity = s.Quantity })
                            .ToList()
                    }).ToList();

                    var payload = new
                    {
                        ProductId = product.Id,
                        ProductName = product.Name ?? "",
                        Variants = variants
                    };
                    return ApiResponse<object>.SuccessResult(payload);
                }
                catch (Exception)
                {
                    return ApiResponse.ErrorResult("Fail to fetch product variants", HttpStatusCode.InternalServerError, ErrorCode.InternalServerError);
                }
            });

            builder.MapGet("search", async (string? q, AppDbContext context) =>
            {
                try
                {
                    var query = context.Products.AsQueryable();

                    if (!string.IsNullOrWhiteSpace(q))
                    {
                        query = query.Where(p => p.Name.ToLower().Contains(q.ToLower()));
                    }

                    var res = await query
                       .Select(x => new ProductPreviewDTO
                       (
                           x.Id,
                           x.Name,
                           x.Price,
                           x.Variants.Select(v => v.Color.HexCode).ToList(),
                           x.Variants.SelectMany(v => v.Images).Select(i => i.Url).FirstOrDefault()
                       ))
                       .ToListAsync();

                    return ApiResponse<List<ProductPreviewDTO>>.SuccessResult(res);
                }
                catch (Exception ex)
                {
                    return ApiResponse.ErrorResult("Fail to search products", HttpStatusCode.InternalServerError, ErrorCode.InternalServerError);
                }
            });
        }
    }
}
