using API.Services;

namespace API.Interfaces
{
    public interface IGeocodingService
    {
        Task<GeocodeResult?> GeocodeAddressAsync(string fullAddress);
    }
}
