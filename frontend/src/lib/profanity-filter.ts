const BANNED_PATTERNS = new Set([
    "dit", "dut", "dcm", "dm", "dmm", "dkm", "dcmm", "dtc", "dtm",
    "lon", "l0n", "buoi", "cc", "cac", "cak", "cuc", "cu",
    "vcl", "vkl", "vl", "cl", "clm", "cmm", "cmnr",
    "ngu", "ngulol", "dotbien", "dot bien",
    "matday", "mat day", "matme", "mat me",
    "deo", "de0", "thangcho", "concho",
    "ducme", "memay", "chamay", "bomay"
]);

const CHAR_SUBSTITUTIONS: Record<string, string> = {
    "0": "o", "1": "i", "3": "e", "4": "a", "5": "s", "7": "t", "8": "b", "@": "a", "$": "s", "!": "i", "*": "a"
};

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

function normalizeText(text: string): string {
    let result = "";
    for (const char of text.toLowerCase()) {
        if (CHAR_SUBSTITUTIONS[char]) { result += CHAR_SUBSTITUTIONS[char]; continue; }
        if (VIETNAMESE_MAP[char]) { result += VIETNAMESE_MAP[char]; continue; }
        if (/[a-z]/.test(char)) result += char;
    }
    return result;
}

export function containsProfanity(text: string): boolean {
    if (!text?.trim()) return false;
    const normalized = normalizeText(text);
    for (const pattern of BANNED_PATTERNS) {
        if (normalized.includes(pattern)) return true;
    }
    return false;
}

export function getProfanityErrorMessage(): string {
    return "Nội dung chứa từ ngữ không phù hợp. Vui lòng chỉnh sửa lại.";
}