using Microsoft.Extensions.Options;
using UteLearningHub.Application.Services.FileStorage;
using UteLearningHub.Infrastructure.ConfigurationOptions;

namespace UteLearningHub.Infrastructure.Services.FileStorage;

public class FileStorageService : IFileStorageService
{
    private readonly string _basePath;
    private readonly string _baseUrl;

    public FileStorageService(IOptions<FileStorageOptions> options)
    {
        var fileStorageOptions = options.Value;
        _basePath = !string.IsNullOrEmpty(fileStorageOptions.BasePath)
            ? fileStorageOptions.BasePath
            : Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        _baseUrl = fileStorageOptions.BaseUrl;

        if (!Directory.Exists(_basePath))
        {
            Directory.CreateDirectory(_basePath);
        }
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, string? category = null, CancellationToken cancellationToken = default)
    {
        var sanitizedFileName = SanitizeFileName(fileName);
        var uniqueFileName = $"{Guid.NewGuid()}_{sanitizedFileName}";
        var folder = GetFolderFromCategory(category);
        
        string filePath;
        string fileUrl;
        
        if (string.IsNullOrEmpty(folder))
        {
            // No folder - save at root
            filePath = Path.Combine(_basePath, uniqueFileName);
            fileUrl = $"{_baseUrl}/{Uri.EscapeDataString(uniqueFileName)}";
        }
        else
        {
            // Create subfolder if needed
            var folderPath = Path.Combine(_basePath, folder);
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            
            filePath = Path.Combine(folderPath, uniqueFileName);
            fileUrl = $"{_baseUrl}/{folder}/{Uri.EscapeDataString(uniqueFileName)}";
        }

        using (var fileStreamWriter = new FileStream(filePath, FileMode.Create))
        {
            await fileStream.CopyToAsync(fileStreamWriter, cancellationToken);
        }

        return fileUrl;
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
        var invalidChars = Path.GetInvalidFileNameChars().Concat(new[] { ' ', '#', '%', '&', '{', '}', '\\', '<', '>', '*', '?', '/', '$', '!', '\'', '"', ':', '@', '+', '`', '|', '=' }).ToArray();
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
            _ => "" // Default folder
        };
    }




    public Task<bool> DeleteFileAsync(string fileUrl, CancellationToken cancellationToken = default)
    {
        try
        {
            var fileName = Path.GetFileName(fileUrl);
            var filePath = Path.Combine(_basePath, fileName);

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    public Task<Stream?> GetFileAsync(string fileUrl, CancellationToken cancellationToken = default)
    {
        try
        {
            var fileName = Path.GetFileName(fileUrl);
            var filePath = Path.Combine(_basePath, fileName);

            if (System.IO.File.Exists(filePath))
            {
                return Task.FromResult<Stream?>(new FileStream(filePath, FileMode.Open, FileAccess.Read));
            }

            return Task.FromResult<Stream?>(null);
        }
        catch
        {
            return Task.FromResult<Stream?>(null);
        }
    }

    public string? GetPresignedUrl(string fileUrl, int expirationMinutes = 15)
    {
        return null;
    }
}
