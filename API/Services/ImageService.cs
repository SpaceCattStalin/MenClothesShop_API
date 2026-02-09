using Amazon.S3;
using Amazon.S3.Model;

namespace Services
{
    public class ImageService
    {
        private readonly IAmazonS3 _client;
        private readonly ILogger<ImageService> _logger;
        public ImageService(IAmazonS3 client, ILogger<ImageService> logger)
        {
            _client = client;
            _logger = logger;
        }

        public async Task<ListObjectsV2Response> ListImagesAsync()
        {
            try
            {
                var request = new ListObjectsV2Request();
                request.BucketName = "my-bucket";

                var result = await _client.ListObjectsV2Async(request);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return null;
            }
        }

    }
}
