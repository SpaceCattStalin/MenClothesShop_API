using API.Interfaces;
using Common.Commons;

namespace API.Features
{
    public static class MapEndpoints
    {
        public static void MapEndpoint(IEndpointRouteBuilder builder)
        {
            builder.MapGet("map", async (IGeocodingService service, string fullAddress) =>
            {
                try
                {
                    var res = await service.GeocodeAddressAsync(fullAddress);
                    return Results.Json(res);
                }
                catch (Exception e)
                {
                    return Results.Json(ApiResponse.ErrorResult(
                        $"{e.Message}",
                        HttpStatusCode.InternalServerError,
                        ErrorCode.InternalServerError));
                }
            });
        }
    }
}
