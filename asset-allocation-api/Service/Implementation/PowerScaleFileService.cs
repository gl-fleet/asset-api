using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Confluent.Kafka;
using Microsoft.IdentityModel.Tokens;
using asset_allocation_api.Config;
using asset_allocation_api.Model.Output_Model;
using asset_allocation_api.Models;
using asset_allocation_api.Util;


namespace asset_allocation_api.Service.Implementation
{
    public class PowerScaleFileService
    {
        private readonly ILogger<PowerScaleFileService> _logger;
        private string AccessKey = AssetAllocationConfig.S3AccessKey;
        private string SecretKey = AssetAllocationConfig.S3SecretKey;
        private string BucketName = AssetAllocationConfig.S3BucketName;
        private string Endpoint = AssetAllocationConfig.S3Uri;
        private readonly HttpClient client = new();

        public PowerScaleFileService(ILogger<PowerScaleFileService> logger)
        {
            _logger = logger;
        }

        public async Task<FileUploadResult> UploadImage(IFormFile imageFile)
        {
            _logger.LogInformation("File Upload Is Starting...");
            _logger.LogInformation("S3 URL: {}", Endpoint);
            using var memoryStream = new MemoryStream();
            await imageFile.CopyToAsync(memoryStream);
            Response<object?> resp = new();
            try
            {
                var key = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
                var putObjectRequest = new PutObjectRequest()
                {
                    BucketName = BucketName,
                    Key = key,
                    InputStream = memoryStream,
                    UseChunkEncoding = false,
                    ContentType = imageFile.ContentType
                };
                var s3client = new AmazonS3Client(
                    new BasicAWSCredentials(AccessKey, SecretKey), new AmazonS3Config
                    {
                        ServiceURL = Endpoint,
                        ForcePathStyle = true
                    });

                await s3client.PutObjectAsync(putObjectRequest);
                _logger.LogInformation("File uploaded to S3 with key: {}", key);

                FileUploadResult resultobj = new()
                {
                    Key = key,
                    FileName = imageFile.FileName
                };
                return resultobj;
            }
            catch (AmazonS3Exception ex)
            {
                if (ex.ErrorCode != null &&
                    (ex.ErrorCode.Equals("InvalidAccessKeyId") || ex.ErrorCode.Equals("InvalidSecurity")))
                {
                    _logger.LogError("Check the provided AWS Credentials.");
                }
                else
                {
                    _logger.LogError("Error occurred: {}", ex.Message);
                }

                _logger.LogError("Error uploading file to S3: {}", ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError("An exception occurred. Message: {}", ex.Message);
                throw;
            }
        }

        public async Task<byte[]> GetImage(string objectKey)
        {
            try
            {
                var s3client = new AmazonS3Client(
                    new BasicAWSCredentials(AccessKey, SecretKey), new AmazonS3Config
                    {
                        ServiceURL = Endpoint,
                        ForcePathStyle = true
                    });

                var request = new GetPreSignedUrlRequest
                {
                    BucketName = BucketName,
                    Key = objectKey,
                    Expires = DateTime.Now.AddHours(1) // Set expiration time for the URL if needed
                };

                var url = s3client.GetPreSignedURL(request);
                _logger.LogInformation("Image URL: {}", url);
                var request1 = new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                };
                request1.RequestUri = new Uri($"{url.Replace("https://", "http://")}");

                var result = await client.SendAsync(request1);

                return await result.Content.ReadAsByteArrayAsync();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

    }
}
