using Amazon.S3;
using Common.Commons;
using Microsoft.EntityFrameworkCore;
using MySqlX.XDevAPI;
using Repositories.ApplicationDbContext;
using Repositories.Models;
using Services;
using System.Threading.Tasks;

namespace API.Features
{
    public static class CategoryEndpoint
    {
        public record GetAllResponse(int Id, String name, String imgUri);
        public static void MapEndpoint(IEndpointRouteBuilder builder)
        {
            builder.MapGet("category", async (AppDbContext context) =>
            {
                try
                {
                    var res = context.Category.Select(c => new GetAllResponse(c.Id, c.Name, c.ImgUri)).ToList();
                    return ApiResponse<List<GetAllResponse>>.SuccessResult(res);
                }
                catch (Exception ex)
                {
                    return ApiResponse.ErrorResult("Fail to fetch all category", HttpStatusCode.InternalServerError, ErrorCode.InternalServerError);
                }
            });

            //builder.MapGet("test", async (IAmazonS3 client, ILogger<ImageService> logger) =>
            //{
            //    ImageService service = new ImageService(client, logger);
            //    return await service.ListImagesAsync();
            //});
            //builder.MapGet("test", async (ImageService service) =>
            //{
            //    return await service.ListImagesAsync();
            //});
        }
    }
}
