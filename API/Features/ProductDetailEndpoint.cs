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
            List<string> imgUrl
        );
        //public record GetProductDetailResponse(List<ProductVariantDTO> variants);

        public static void MapEndpoint(IEndpointRouteBuilder routeBuilder)
        {
            routeBuilder.MapGet("products/{productVariantId}", async (AppDbContext context, int productVariantId) =>
            {
                try
                {
                    var res = await context.ProductVariants
                    .Where(v => v.MainProductId.Equals(productVariantId))
                    .Select(v =>
                        new ProductVariantDTO(v.Id, v.Color.HexCode,
                        v.MainProduct.Price,
                        v.Sizes.Select(vs => new SizeStockDTO(vs.Size.Name, vs.Quantity)).ToList(),
                        v.Images.Select(i => i.Url).ToList()
                    ))
                    .ToListAsync();

                    return ApiResponse<List<ProductVariantDTO>>.SuccessResult(res);
                }
                catch (Exception ex)
                {
                    return ApiResponse.ErrorResult($"Fail to fetch product variant of product with id {productVariantId}", HttpStatusCode.InternalServerError, ErrorCode.InternalServerError);
                }
            });
        }
    }
}
