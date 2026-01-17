namespace UteLearningHub.Application.Services.ContentModeration;

public interface IProfanityFilterService
{
    bool ContainsProfanity(string text);
    IEnumerable<string> GetViolations(string text);
}

public class ProfanityFilterService : IProfanityFilterService
{
    private static readonly HashSet<string> BannedPatterns = new(StringComparer.OrdinalIgnoreCase)
    {
        "dit", "dut", "dcm", "dm", "dmm", "dkm", "dcmm", "dtc", "dtm",
        "lon", "l0n", "buoi", "cc", "cac", "cak", "cuc", "cu",
        "vcl", "vkl", "vl", "cl", "clm", "cmm", "cmnr",
        "ngu", "ngulol", "dotbien", "dot bien",
        "matday", "mat day", "matme", "mat me",
        "deo", "de0", "thangcho", "concho",
        "ducme", "memay", "chamay", "bomay"
    };

    // Map ký tự ??
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

    // Normalize: số->chữ, xóa dấu, xóa ký tự đặc biệt
    private static string NormalizeText(string text)
    {
        var chars = new List<char>(text.Length);
        
        foreach (var c in text.ToLowerInvariant())
        {
            if (CharSubstitutions.TryGetValue(c, out var replacement))
            {
                chars.Add(replacement);
                continue;
            }

            if (char.IsLetter(c))
            {
                var normalized = RemoveVietnameseDiacritics(c);
                chars.Add(normalized);
            }
        }

        return new string(chars.ToArray());
    }

    private static char RemoveVietnameseDiacritics(char c)
    {
        return c switch
        {
            'á' or 'à' or 'ả' or 'ã' or 'ạ' or 
            'ă' or 'ắ' or 'ằ' or 'ẳ' or 'ẵ' or 'ặ' or
            'â' or 'ấ' or 'ầ' or 'ẩ' or 'ẫ' or 'ậ' => 'a',
            
            'é' or 'è' or 'ẻ' or 'ẽ' or 'ẹ' or
            'ê' or 'ế' or 'ề' or 'ể' or 'ễ' or 'ệ' => 'e',
            
            'í' or 'ì' or 'ỉ' or 'ĩ' or 'ị' => 'i',
            
            'ó' or 'ò' or 'ỏ' or 'õ' or 'ọ' or
            'ô' or 'ố' or 'ồ' or 'ổ' or 'ỗ' or 'ộ' or
            'ơ' or 'ớ' or 'ờ' or 'ở' or 'ỡ' or 'ợ' => 'o',
            
            'ú' or 'ù' or 'ủ' or 'ũ' or 'ụ' or
            'ư' or 'ứ' or 'ừ' or 'ử' or 'ữ' or 'ự' => 'u',
            
            'ý' or 'ỳ' or 'ỷ' or 'ỹ' or 'ỵ' => 'y',
            
            'đ' => 'd',
            
            _ => c
        };
    }
}
