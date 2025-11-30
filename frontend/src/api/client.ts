import { client } from '@/src/api/database/client.gen';

/**
 * Lấy access token từ localStorage
 * @returns Token string hoặc undefined nếu không có
 */
export function getAccessToken(): string | undefined {
  if (typeof window === 'undefined') return undefined;
  return localStorage.getItem('access_token') || undefined;
}

/**
 * Lấy Bearer token để dùng trong API requests
 * @returns "Bearer {token}" hoặc undefined nếu không có token
 */
export function getBearerToken(): string | undefined {
  const token = getAccessToken();
  return token ? `Bearer ${token}` : undefined;
}

// Cấu hình client
client.setConfig({
  baseURL: process.env.NEXT_PUBLIC_API_URL || 'https://localhost:7080',
});

// Thêm axios interceptor để tự động thêm Authorization header vào mọi request
if (typeof window !== 'undefined') {
  client.instance.interceptors.request.use(
    (config) => {
      // Chỉ thêm token nếu chưa có Authorization header
      if (!config.headers.Authorization) {
        const token = getBearerToken();
        if (token) {
          config.headers.Authorization = token;
        }
      }
      return config;
    },
    (error) => {
      return Promise.reject(error);
    }
  );
}

// Helper function to check authentication state
export function isAuthenticated(): boolean {
  return !!getAccessToken();
}