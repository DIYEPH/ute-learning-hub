using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using UteLearningHub.Application.Services.FileStorage;
using UteLearningHub.Infrastructure.ConfigurationOptions;

namespace UteLearningHub.Infrastructure.Services.FileStorage;

public class S3FileStorageService(IAmazonS3 s3Client, IOptions<AmazonS3Options> options) : IFileStorageService
{
    private readonly AmazonS3Options _options = options.Value;

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, string? category = null, CancellationToken ct = default)
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
            };

            await s3Client.PutObjectAsync(request, ct);
            return $"{_options.S3BaseUrl}/{Uri.EscapeDataString(key).Replace("%2F", "/")}";
        }
        catch (AmazonS3Exception ex) { throw new Exception($"Failed to upload file to S3: {ex.Message}", ex); }
        catch (Exception ex) { throw new Exception($"An error occurred while uploading file: {ex.Message}", ex); }
    }

    // Chuẩn hóa tên file: bỏ dấu tiếng Việt, thay ký tự đặc biệt bằng _
    private static string SanitizeFileName(string fileName)
    {
        if (string.IsNullOrEmpty(fileName)) return "file";

        var extension = Path.GetExtension(fileName);
        var nameWithoutExt = RemoveVietnameseDiacritics(Path.GetFileNameWithoutExtension(fileName));

        var invalidChars = Path.GetInvalidFileNameChars()
            .Concat([' ', '#', '%', '&', '{', '}', '\\', '<', '>', '*', '?', '/', '$', '!', '\'', '"', ':', '@', '+', '`', '|', '=', '(', ')'])
            .ToArray();

        foreach (var c in invalidChars)
            nameWithoutExt = nameWithoutExt.Replace(c, '_');

        while (nameWithoutExt.Contains("__"))
            nameWithoutExt = nameWithoutExt.Replace("__", "_");

        nameWithoutExt = nameWithoutExt.Trim('_');
        return string.IsNullOrEmpty(nameWithoutExt) ? $"file{extension}" : nameWithoutExt + extension;
    }

    // Chuyển ký tự có dấu tiếng Việt thành không dấu
    private static string RemoveVietnameseDiacritics(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;

        var vietnameseChars = new Dictionary<string, string>
        {
            { "àáạảãâầấậẩẫăằắặẳẵ", "a" }, { "èéẹẻẽêềếệểễ", "e" },
            { "ìíịỉĩ", "i" }, { "òóọỏõôồốộổỗơờớợởỡ", "o" },
            { "ùúụủũưừứựửữ", "u" }, { "ỳýỵỷỹ", "y" }, { "đ", "d" },
            { "ÀÁẠẢÃÂẦẤẬẨẪĂẰẮẶẲẴ", "A" }, { "ÈÉẸẺẼÊỀẾỆỂỄ", "E" },
            { "ÌÍỊỈĨ", "I" }, { "ÒÓỌỎÕÔỒỐỘỔỖƠỜỚỢỞỠ", "O" },
            { "ÙÚỤỦŨƯỪỨỰỬỮ", "U" }, { "ỲÝỴỶỸ", "Y" }, { "Đ", "D" }
        };

        foreach (var kvp in vietnameseChars)
            foreach (char c in kvp.Key)
                text = text.Replace(c.ToString(), kvp.Value);

        return text;
    }

    private static string GetFolderFromCategory(string? category) => category?.ToLowerInvariant() switch
    {
        "avataruser" => "avatars/users",
        "avatarconversation" => "avatars/conversations",
        "documentcover" => "documents/covers",
        "documentfilecover" => "documents/file-covers",
        "documentfile" => "documents/files",
        "message" => "messages",
        _ => ""
    };

    public async Task<bool> DeleteFileAsync(string fileUrl, CancellationToken ct = default)
    {
        try
        {
            var key = ExtractKeyFromUrl(fileUrl);
            if (string.IsNullOrEmpty(key)) return false;

            await s3Client.DeleteObjectAsync(new DeleteObjectRequest { BucketName = _options.S3BucketName, Key = key }, ct);
            return true;
        }
        catch { return false; }
    }

    public async Task<Stream?> GetFileAsync(string fileUrl, CancellationToken ct = default)
    {
        try
        {
            var key = ExtractKeyFromUrl(fileUrl);
            if (string.IsNullOrEmpty(key)) return null;

            var response = await s3Client.GetObjectAsync(new GetObjectRequest { BucketName = _options.S3BucketName, Key = key }, ct);
            return response.ResponseStream;
        }
        catch { return null; }
    }

    public string? GetPresignedUrl(string fileUrl, int expirationMinutes = 15)
    {
        try
        {
            if (!fileUrl.Contains(_options.S3BucketName))
                return fileUrl.StartsWith("http://") || fileUrl.StartsWith("https://") ? fileUrl : null;

            var key = ExtractKeyFromUrl(fileUrl);
            if (string.IsNullOrEmpty(key)) return null;

            return s3Client.GetPreSignedURL(new GetPreSignedUrlRequest
            {
                BucketName = _options.S3BucketName,
                Key = key,
                Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),
                Verb = HttpVerb.GET
            });
        }
        catch { return null; }
    }

    // Lấy key S3 từ URL (bỏ domain, decode path)
    private static string? ExtractKeyFromUrl(string fileUrl)
    {
        try
        {
            var uri = new Uri(fileUrl);
            return Uri.UnescapeDataString(uri.AbsolutePath.TrimStart('/'));
        }
        catch { return null; }
    }
}
