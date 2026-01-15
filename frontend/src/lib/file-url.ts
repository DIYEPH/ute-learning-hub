function getToken(): string | undefined {
  if (typeof window === "undefined") return undefined;
  return localStorage.getItem("access_token") || undefined;
}

export function getFileUrlById(fileIdOrUrl: string | undefined | null): string {
  if (!fileIdOrUrl) return "";
  if (fileIdOrUrl.startsWith("http://") || fileIdOrUrl.startsWith("https://")) return fileIdOrUrl;
  const apiBaseUrl = process.env.NEXT_PUBLIC_API_URL ?? "https://localhost:7080";
  const baseUrl = `${apiBaseUrl}/api/File/${fileIdOrUrl}`;
  const token = getToken();
  if (token) return `${baseUrl}?access_token=${encodeURIComponent(token)}`;
  return baseUrl;
}
