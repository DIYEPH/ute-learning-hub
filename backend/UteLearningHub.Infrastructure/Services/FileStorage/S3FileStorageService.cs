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

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, string? category = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var sanitizedFileName = SanitizeFileName(fileName);
            var uniqueFileName = $"{Guid.NewGuid()}_{sanitizedFileName}";
            var folder = GetFolderFromCategory(category);

            var key = string.IsNullOrEmpty(folder) ? uniqueFileName : $"{folder}/{uniqueFileName}";

            var request = new PutObjectRequest
            {
                BucketName = _options.S3BucketName,
                Key = key,
                InputStream = fileStream,
                ContentType = contentType
                // Removed S3CannedACL.PublicRead - files are now private and accessed via pre-signed URLs
            };

            await _s3Client.PutObjectAsync(request, cancellationToken);

            var fileUrl = $"{_options.S3BaseUrl}/{Uri.EscapeDataString(key).Replace("%2F", "/")}";
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

    /// <summary>
    /// Sanitize filename by removing Vietnamese diacritics and replacing invalid characters
    /// </summary>
    private static string SanitizeFileName(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
            return "file";

        // Get extension
        var extension = Path.GetExtension(fileName);
        var nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);

        // Remove Vietnamese diacritics
        nameWithoutExt = RemoveVietnameseDiacritics(nameWithoutExt);

        // Replace spaces and invalid characters with underscore
        var invalidChars = Path.GetInvalidFileNameChars().Concat(new[] { ' ', '#', '%', '&', '{', '}', '\\', '<', '>', '*', '?', '/', '$', '!', '\'', '"', ':', '@', '+', '`', '|', '=', '(', ')' }).ToArray();
        foreach (var c in invalidChars)
        {
            nameWithoutExt = nameWithoutExt.Replace(c, '_');
        }

        // Remove consecutive underscores
        while (nameWithoutExt.Contains("__"))
        {
            nameWithoutExt = nameWithoutExt.Replace("__", "_");
        }

        // Trim underscores from start and end
        nameWithoutExt = nameWithoutExt.Trim('_');

        // If name is empty after sanitization, use a default
        if (string.IsNullOrEmpty(nameWithoutExt))
            nameWithoutExt = "file";

        return nameWithoutExt + extension;
    }

    /// <summary>
    /// Remove Vietnamese diacritics from a string
    /// </summary>
    private static string RemoveVietnameseDiacritics(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        // Vietnamese character mappings
        var vietnameseChars = new Dictionary<string, string>
        {
            { "àáạảãâầấậẩẫăằắặẳẵ", "a" },
            { "èéẹẻẽêềếệểễ", "e" },
            { "ìíịỉĩ", "i" },
            { "òóọỏõôồốộổỗơờớợởỡ", "o" },
            { "ùúụủũưừứựửữ", "u" },
            { "ỳýỵỷỹ", "y" },
            { "đ", "d" },
            { "ÀÁẠẢÃÂẦẤẬẨẪĂẰẮẶẲẴ", "A" },
            { "ÈÉẸẺẼÊỀẾỆỂỄ", "E" },
            { "ÌÍỊỈĨ", "I" },
            { "ÒÓỌỎÕÔỒỐỘỔỖƠỜỚỢỞỠ", "O" },
            { "ÙÚỤỦŨƯỪỨỰỬỮ", "U" },
            { "ỲÝỴỶỸ", "Y" },
            { "Đ", "D" }
        };

        foreach (var kvp in vietnameseChars)
        {
            foreach (char c in kvp.Key)
            {
                text = text.Replace(c.ToString(), kvp.Value);
            }
        }

        return text;
    }
    private static string GetFolderFromCategory(string? category)
    {
        return category?.ToLowerInvariant() switch
        {
            "avataruser" => "avatars/users",
            "avatarconversation" => "avatars/conversations",
            "documentcover" => "documents/covers",
            "documentfilecover" => "documents/file-covers",
            "documentfile" => "documents/files",
            "message" => "messages",
            _ => "" 
        };
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

    public string? GetPresignedUrl(string fileUrl, int expirationMinutes = 15)
    {
        try
        {
            var key = ExtractKeyFromUrl(fileUrl);
            if (string.IsNullOrEmpty(key))
                return null;

            var request = new GetPreSignedUrlRequest
            {
                BucketName = _options.S3BucketName,
                Key = key,
                Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),
                Verb = HttpVerb.GET
            };

            return _s3Client.GetPreSignedURL(request);
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

