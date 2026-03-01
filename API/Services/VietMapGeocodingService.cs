using API.Interfaces;
using System.Text.Json;

namespace API.Services
{
    public record GeocodeResult(double Lat, double Lng);
    public class VietMapGeocodingService : IGeocodingService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly ILogger<VietMapGeocodingService> _logger;

        public VietMapGeocodingService(
            HttpClient httpClient,
            ILogger<VietMapGeocodingService> logger)
        {
            _httpClient = httpClient;
            // Best practice: Fallback to config if Env var is missing
            _apiKey = Environment.GetEnvironmentVariable("VIETMAP_API_KEY");
            _logger = logger;
        }

        public async Task<GeocodeResult?> GeocodeAddressAsync(string fullAddress)
        {
            try
            {
                // STEP 1: Search for the address to get a ref_id
                var searchUrl = $"https://maps.vietmap.vn/api/search/v4?apikey={_apiKey}&text={Uri.EscapeDataString(fullAddress)}";
                var searchResponse = await _httpClient.GetAsync(searchUrl);

                if (!searchResponse.IsSuccessStatusCode) return null;

                var searchJson = await searchResponse.Content.ReadAsStringAsync();
                using var searchDoc = JsonDocument.Parse(searchJson);

                var firstResult = searchDoc.RootElement.EnumerateArray().FirstOrDefault();

                if (firstResult.ValueKind == JsonValueKind.Undefined) return null;

                string refId = firstResult.GetProperty("ref_id").GetString();

                var placeUrl = $"https://maps.vietmap.vn/api/place/v4?apikey={_apiKey}&refid={refId}";
                var placeResponse = await _httpClient.GetAsync(placeUrl);

                if (!placeResponse.IsSuccessStatusCode) return null;

                var placeJson = await placeResponse.Content.ReadAsStringAsync();
                using var placeDoc = JsonDocument.Parse(placeJson);
                var root = placeDoc.RootElement;

                // Extract lat and lng
                double lat = root.GetProperty("lat").GetDouble();
                double lng = root.GetProperty("lng").GetDouble();

                return new GeocodeResult(lat, lng);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "VietMap Geocoding failed for address: {Address}", fullAddress);
                return null;
            }
        }
    }
}
