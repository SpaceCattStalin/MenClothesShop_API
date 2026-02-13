using Common.Commons;
using Microsoft.EntityFrameworkCore;
using Repositories.ApplicationDbContext;

namespace API.Features
{
    public static class ProductDetailEndpoint
    {
        public record SizeStockDTO(string size, int quantity);
        public record ProductVariantDTO(
            int variantId,
            string colorHex,
            decimal price,
            List<SizeStockDTO> inStock,
            string imgUrl
        );
        //public record GetProductDetailResponse(List<ProductVariantDTO> variants);

        public static void MapEndpoint(IEndpointRouteBuilder routeBuilder)
        {
            routeBuilder.MapGet("products/{productId}", async (AppDbContext context, int productId) =>
            {
                try
                {
                    var res = await context.ProductVariants
                    .Where(v => v.MainProductId.Equals(productId))
                    .Select(v =>
                        new ProductVariantDTO(v.Id, v.Color.HexCode,
                        v.MainProduct.Price,
                        v.Sizes.Select(vs => new SizeStockDTO(vs.Size.Name, vs.Quantity)).ToList(),
                        v.Images.FirstOrDefault().Url
                    ))
                    .ToListAsync();

                    return ApiResponse<List<ProductVariantDTO>>.SuccessResult(res);
                }
                catch (Exception ex)
                {
                    return ApiResponse.ErrorResult($"Fail to fetch product variant of product with id {productId}", HttpStatusCode.InternalServerError, ErrorCode.InternalServerError);
                }
            });
        }
    }
}
