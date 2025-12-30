import { client } from '@/src/api/database/client.gen';
import { postApiAuthRefreshToken } from '@/src/api/database/sdk.gen';

const ACCESS_TOKEN_KEY = 'access_token';
const AUTH_CHANGED_EVENT = 'auth-changed';

// ============ Token Management ============

export function getAccessToken(): string | undefined {
  if (typeof window === 'undefined') return undefined;
  return localStorage.getItem(ACCESS_TOKEN_KEY) || undefined;
}

export function setAccessToken(token?: string): void {
  if (typeof window === 'undefined') return;

  if (token) {
    localStorage.setItem(ACCESS_TOKEN_KEY, token);
  } else {
    localStorage.removeItem(ACCESS_TOKEN_KEY);
  }

  window.dispatchEvent(new Event(AUTH_CHANGED_EVENT));
}

export function clearTokens(): void {
  if (typeof window === 'undefined') return;
  localStorage.removeItem(ACCESS_TOKEN_KEY);
  window.dispatchEvent(new Event(AUTH_CHANGED_EVENT));
}

export function isAuthenticated(): boolean {
  return !!getAccessToken();
}

export function getBearerToken(): string | undefined {
  const token = getAccessToken();
  return token ? `Bearer ${token}` : undefined;
}

export const authEvents = {
  AUTH_CHANGED_EVENT,
};

// ============ Token Refresh Logic ============

let isRefreshing = false;
let refreshPromise: Promise<string | null> | null = null;

async function refreshToken(): Promise<string | null> {
  try {
    const response = await postApiAuthRefreshToken();
    const data = response.data as { accessToken?: string } | undefined;
    if (data?.accessToken) {
      setAccessToken(data.accessToken);
      return data.accessToken;
    }
    return null;
  } catch {
    clearTokens();
    return null;
  }
}

// ============ Axios Configuration ============

const apiBaseUrl = (process.env.NEXT_PUBLIC_API_URL ?? 'https://localhost:7080').replace(/\/+$/, '');

client.setConfig({
  baseURL: apiBaseUrl,
});

if (typeof window !== 'undefined') {
  client.instance.defaults.withCredentials = true;

  // Request interceptor: Add Authorization header
  client.instance.interceptors.request.use(
    (config) => {
      const token = getAccessToken();
      if (token && !config.headers.Authorization) {
        config.headers.Authorization = `Bearer ${token}`;
      }
      return config;
    },
    (error) => Promise.reject(error)
  );

  // Response interceptor: Handle 401 with token refresh
  client.instance.interceptors.response.use(
    (response) => response,
    async (error) => {
      const originalRequest = error.config;
      const isAuthEndpoint = originalRequest?.url?.toLowerCase().includes('/api/auth/');

      // Skip refresh for auth endpoints
      if (isAuthEndpoint) {
        return Promise.reject(error);
      }

      // Handle 401 - try refresh token once
      if (error?.response?.status === 401 && !originalRequest._retry) {
        originalRequest._retry = true;

        // Reuse existing refresh promise to avoid multiple calls
        if (!isRefreshing) {
          isRefreshing = true;
          refreshPromise = refreshToken().finally(() => {
            isRefreshing = false;
            refreshPromise = null;
          });
        }

        const newToken = await refreshPromise;
        if (newToken) {
          originalRequest.headers.Authorization = `Bearer ${newToken}`;
          return client.instance(originalRequest);
        }

        clearTokens();
      }

      return Promise.reject(error);
    }
  );
}