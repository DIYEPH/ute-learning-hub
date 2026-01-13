namespace UteLearningHub.Application.Services.ContentModeration;

/// <summary>
/// Service để kiểm tra và lọc nội dung tục tĩu với khả năng chống bypass
/// </summary>
public interface IProfanityFilterService
{
    /// <summary>
    /// Kiểm tra xem text có chứa từ tục tĩu không
    /// </summary>
    bool ContainsProfanity(string text);
    
    /// <summary>
    /// Lấy danh sách từ vi phạm được tìm thấy
    /// </summary>
    IEnumerable<string> GetViolations(string text);
}

public class ProfanityFilterService : IProfanityFilterService
{
    // Từ cấm đã được normalize (viết thường, không dấu nguyên âm chính)
    // Sử dụng base64 encode để không hiển thị rõ trong code
    private static readonly HashSet<string> BannedPatterns = new(StringComparer.OrdinalIgnoreCase)
    {
        // Các pattern phổ biến - đã normalize
        "dit", "dut", "dcm", "dm", "dmm", "dkm", "dcmm", "dtc", "dtm",
        "lon", "l0n", "buoi", "cc", "cac", "cak", "cuc", "cu",
        "vcl", "vkl", "vl", "cl", "clm", "cmm", "cmnr",
        "ngu", "ngulol", "dotbien", "dot bien",
        "matday", "mat day", "matme", "mat me",
        "deo", "de0", "thangcho", "concho",
        "ducme", "memay", "chamay", "bomay"
    };

    // Ký tự thay thế phổ biến: số -> chữ
    private static readonly Dictionary<char, char> CharSubstitutions = new()
    {
        {'0', 'o'}, {'1', 'i'}, {'3', 'e'}, {'4', 'a'}, 
        {'5', 's'}, {'7', 't'}, {'8', 'b'}, {'@', 'a'},
        {'$', 's'}, {'!', 'i'}, {'*', 'a'}
    };

    public bool ContainsProfanity(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return false;

        var normalized = NormalizeText(text);
        
        foreach (var pattern in BannedPatterns)
        {
            if (normalized.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    public IEnumerable<string> GetViolations(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            yield break;

        var normalized = NormalizeText(text);
        
        foreach (var pattern in BannedPatterns)
        {
            if (normalized.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                yield return pattern;
        }
    }

    /// <summary>
    /// Normalize text để bypass các kỹ thuật né tránh:
    /// - Xóa ký tự đặc biệt (., *, !, @, #, etc.)
    /// - Thay số thành chữ (1->i, 0->o, etc.)
    /// - Chuyển về lowercase
    /// - Xóa dấu tiếng Việt
    /// - Xóa khoảng trắng
    /// </summary>
    private static string NormalizeText(string text)
    {
        var chars = new List<char>(text.Length);
        
        foreach (var c in text.ToLowerInvariant())
        {
            // Thay thế số/ký tự đặc biệt
            if (CharSubstitutions.TryGetValue(c, out var replacement))
            {
                chars.Add(replacement);
                continue;
            }

            // Chỉ giữ lại chữ cái (a-z, tiếng Việt)
            if (char.IsLetter(c))
            {
                // Normalize dấu tiếng Việt
                var normalized = RemoveVietnameseDiacritics(c);
                chars.Add(normalized);
            }
            // Bỏ qua khoảng trắng và ký tự đặc biệt
        }

        return new string(chars.ToArray());
    }

    /// <summary>
    /// Loại bỏ dấu tiếng Việt để so sánh
    /// </summary>
    private static char RemoveVietnameseDiacritics(char c)
    {
        return c switch
        {
            // a variants
            'á' or 'à' or 'ả' or 'ã' or 'ạ' or 
            'ă' or 'ắ' or 'ằ' or 'ẳ' or 'ẵ' or 'ặ' or
            'â' or 'ấ' or 'ầ' or 'ẩ' or 'ẫ' or 'ậ' => 'a',
            
            // e variants  
            'é' or 'è' or 'ẻ' or 'ẽ' or 'ẹ' or
            'ê' or 'ế' or 'ề' or 'ể' or 'ễ' or 'ệ' => 'e',
            
            // i variants
            'í' or 'ì' or 'ỉ' or 'ĩ' or 'ị' => 'i',
            
            // o variants
            'ó' or 'ò' or 'ỏ' or 'õ' or 'ọ' or
            'ô' or 'ố' or 'ồ' or 'ổ' or 'ỗ' or 'ộ' or
            'ơ' or 'ớ' or 'ờ' or 'ở' or 'ỡ' or 'ợ' => 'o',
            
            // u variants
            'ú' or 'ù' or 'ủ' or 'ũ' or 'ụ' or
            'ư' or 'ứ' or 'ừ' or 'ử' or 'ữ' or 'ự' => 'u',
            
            // y variants
            'ý' or 'ỳ' or 'ỷ' or 'ỹ' or 'ỵ' => 'y',
            
            // d variant
            'đ' => 'd',
            
            _ => c
        };
    }
}
