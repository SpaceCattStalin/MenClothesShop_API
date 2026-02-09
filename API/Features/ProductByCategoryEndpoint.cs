using Common.Commons;
using Microsoft.EntityFrameworkCore;
using Repositories.ApplicationDbContext;
using Repositories.Models;

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
        }
    }
}
