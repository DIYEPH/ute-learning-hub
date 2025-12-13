/**
 * Get access token from localStorage (client-side only)
 */
function getToken(): string | undefined {
  if (typeof window === "undefined") return undefined;
  return localStorage.getItem("access_token") || undefined;
}

/**
 * Construct file URL from file ID using the File API endpoint
 * Automatically appends access_token if available for authenticated file access
 * @param fileId - The GUID of the file to retrieve
 * @returns The fully qualified URL to fetch the file, or empty string if fileId is null/undefined
 */
export function getFileUrlById(fileId: string | undefined | null): string {
  if (!fileId) return "";
  const apiBaseUrl = process.env.NEXT_PUBLIC_API_URL ?? "https://localhost:7080";
  const baseUrl = `${apiBaseUrl}/api/File/${fileId}`;

  // Append token if available (for iframe/img that can't use Authorization header)
  const token = getToken();
  if (token) {
    return `${baseUrl}?access_token=${encodeURIComponent(token)}`;
  }

  return baseUrl;
}
