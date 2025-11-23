using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using UteLearningHub.Application.Services.FileStorage;
using UteLearningHub.Infrastructure.ConfigurationOptions;

namespace UteLearningHub.Infrastructure.Services.FileStorage;

public class S3FileStorageService : IFileStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly AmazonS3Options _options;

    public S3FileStorageService(IAmazonS3 s3Client, IOptions<AmazonS3Options> options)
    {
        _s3Client = s3Client;
        _options = options.Value;
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default)
    {
        try
        {
            var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
            var key = $"documents/{uniqueFileName}";

            var request = new PutObjectRequest
            {
                BucketName = _options.S3BucketName,
                Key = key,
                InputStream = fileStream,
                ContentType = contentType,
                CannedACL = S3CannedACL.PublicRead
            };

            await _s3Client.PutObjectAsync(request, cancellationToken);

            var fileUrl = $"{_options.S3BaseUrl}/{key}";
            return fileUrl;
        }
        catch (AmazonS3Exception ex)
        {
            throw new Exception($"Failed to upload file to S3: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new Exception($"An error occurred while uploading file: {ex.Message}", ex);
        }
    }

    public async Task<bool> DeleteFileAsync(string fileUrl, CancellationToken cancellationToken = default)
    {
        try
        {
            // Extract key from URL
            var key = ExtractKeyFromUrl(fileUrl);
            if (string.IsNullOrEmpty(key))
                return false;

            var request = new DeleteObjectRequest
            {
                BucketName = _options.S3BucketName,
                Key = key
            };

            await _s3Client.DeleteObjectAsync(request, cancellationToken);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<Stream?> GetFileAsync(string fileUrl, CancellationToken cancellationToken = default)
    {
        try
        {
            // Extract key from URL
            var key = ExtractKeyFromUrl(fileUrl);
            if (string.IsNullOrEmpty(key))
                return null;

            var request = new GetObjectRequest
            {
                BucketName = _options.S3BucketName,
                Key = key
            };

            var response = await _s3Client.GetObjectAsync(request, cancellationToken);
            return response.ResponseStream;
        }
        catch
        {
            return null;
        }
    }

    private string? ExtractKeyFromUrl(string fileUrl)
    {
        try
        {
            var uri = new Uri(fileUrl);
            // Remove leading slash
            return uri.AbsolutePath.TrimStart('/');
        }
        catch
        {
            return null;
        }
    }
}
