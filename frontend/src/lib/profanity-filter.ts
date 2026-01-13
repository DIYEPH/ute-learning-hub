/**
 * Profanity filter cho tiếng Việt với khả năng chống bypass
 * - Xử lý số thay chữ (1→i, 0→o, etc.)
 * - Xử lý ký tự đặc biệt (., *, !, @)
 * - Loại bỏ dấu tiếng Việt để so sánh
 */

// Danh sách từ cấm đã normalize (không dấu, viết thường)
const BANNED_PATTERNS = new Set([
    "dit", "dut", "dcm", "dm", "dmm", "dkm", "dcmm", "dtc", "dtm",
    "lon", "l0n", "buoi", "cc", "cac", "cak", "cuc", "cu",
    "vcl", "vkl", "vl", "cl", "clm", "cmm", "cmnr",
    "ngu", "ngulol", "dotbien", "dot bien",
    "matday", "mat day", "matme", "mat me",
    "deo", "de0", "thangcho", "concho",
    "ducme", "memay", "chamay", "bomay"
]);

// Map số/ký tự đặc biệt → chữ
const CHAR_SUBSTITUTIONS: Record<string, string> = {
    "0": "o", "1": "i", "3": "e", "4": "a",
    "5": "s", "7": "t", "8": "b", "@": "a",
    "$": "s", "!": "i", "*": "a"
};

// Map dấu tiếng Việt → không dấu
const VIETNAMESE_MAP: Record<string, string> = {
    "á": "a", "à": "a", "ả": "a", "ã": "a", "ạ": "a",
    "ă": "a", "ắ": "a", "ằ": "a", "ẳ": "a", "ẵ": "a", "ặ": "a",
    "â": "a", "ấ": "a", "ầ": "a", "ẩ": "a", "ẫ": "a", "ậ": "a",
    "é": "e", "è": "e", "ẻ": "e", "ẽ": "e", "ẹ": "e",
    "ê": "e", "ế": "e", "ề": "e", "ể": "e", "ễ": "e", "ệ": "e",
    "í": "i", "ì": "i", "ỉ": "i", "ĩ": "i", "ị": "i",
    "ó": "o", "ò": "o", "ỏ": "o", "õ": "o", "ọ": "o",
    "ô": "o", "ố": "o", "ồ": "o", "ổ": "o", "ỗ": "o", "ộ": "o",
    "ơ": "o", "ớ": "o", "ờ": "o", "ở": "o", "ỡ": "o", "ợ": "o",
    "ú": "u", "ù": "u", "ủ": "u", "ũ": "u", "ụ": "u",
    "ư": "u", "ứ": "u", "ừ": "u", "ử": "u", "ữ": "u", "ự": "u",
    "ý": "y", "ỳ": "y", "ỷ": "y", "ỹ": "y", "ỵ": "y",
    "đ": "d"
};

/**
 * Normalize text để bypass các kỹ thuật né tránh
 */
function normalizeText(text: string): string {
    let result = "";
    for (const char of text.toLowerCase()) {
        // Thay số/ký tự đặc biệt
        if (CHAR_SUBSTITUTIONS[char]) {
            result += CHAR_SUBSTITUTIONS[char];
            continue;
        }
        // Thay dấu tiếng Việt
        if (VIETNAMESE_MAP[char]) {
            result += VIETNAMESE_MAP[char];
            continue;
        }
        // Chỉ giữ chữ cái a-z
        if (/[a-z]/.test(char)) {
            result += char;
        }
    }
    return result;
}

/**
 * Kiểm tra text có chứa từ tục tĩu không
 */
export function containsProfanity(text: string): boolean {
    if (!text?.trim()) return false;

    const normalized = normalizeText(text);

    for (const pattern of BANNED_PATTERNS) {
        if (normalized.includes(pattern)) {
            return true;
        }
    }

    return false;
}

/**
 * Lấy message lỗi cho user
 */
export function getProfanityErrorMessage(): string {
    return "Nội dung chứa từ ngữ không phù hợp. Vui lòng chỉnh sửa lại.";
}
