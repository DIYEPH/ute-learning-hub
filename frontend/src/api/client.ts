import { client } from '@/src/api/database/client.gen';

export function getAccessToken(): string | undefined {
  if (typeof window === 'undefined') return undefined;
  return localStorage.getItem('access_token') || undefined;
}

export function getBearerToken(): string | undefined {
  const token = getAccessToken();
  return token ? `Bearer ${token}` : undefined;
}

client.setConfig({
  baseURL: process.env.NEXT_PUBLIC_API_URL || 'https://localhost:7080',
});

if (typeof window !== 'undefined') {
  client.instance.interceptors.request.use(
    (config) => {
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

export function isAuthenticated(): boolean {
  return !!getAccessToken();
}