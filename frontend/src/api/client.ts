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
let refreshSubscribers: ((token: string) => void)[] = [];

function subscribeToRefresh(callback: (token: string) => void) {
  refreshSubscribers.push(callback);
}

function onRefreshComplete(newToken: string) {
  refreshSubscribers.forEach((callback) => callback(newToken));
  refreshSubscribers = [];
}

async function tryRefreshToken(): Promise<string | null> {
  try {
    // No body needed - backend reads refresh token from httpOnly cookie
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

const apiBaseUrl = process.env.NEXT_PUBLIC_API_URL !== undefined
  ? process.env.NEXT_PUBLIC_API_URL
  : 'https://localhost:7080';

client.setConfig({
  baseURL: apiBaseUrl,
});

if (typeof window !== 'undefined') {
  // Enable sending cookies with requests (for httpOnly refresh token)
  client.instance.defaults.withCredentials = true;

  // Request interceptor: Add Authorization header
  client.instance.interceptors.request.use(
    (config) => {
      if (!config.headers.Authorization) {
        const token = getAccessToken();
        if (token) {
          config.headers.Authorization = `Bearer ${token}`;
        }
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

      // Skip refresh for auth endpoints to avoid infinite loops
      if (originalRequest?.url?.includes('/api/auth/')) {
        if (error?.response?.status === 401) {
          clearTokens();
        }
        return Promise.reject(error);
      }

      // Handle 401 - try to refresh token
      if (error?.response?.status === 401 && !originalRequest._retry) {
        originalRequest._retry = true;

        // If already refreshing, wait for it to complete
        if (isRefreshing) {
          return new Promise((resolve) => {
            subscribeToRefresh((newToken) => {
              originalRequest.headers.Authorization = `Bearer ${newToken}`;
              resolve(client.instance(originalRequest));
            });
          });
        }

        isRefreshing = true;

        try {
          const newToken = await tryRefreshToken();

          if (newToken) {
            onRefreshComplete(newToken);
            originalRequest.headers.Authorization = `Bearer ${newToken}`;
            return client.instance(originalRequest);
          } else {
            // Refresh failed - logout
            clearTokens();
            if (window.location.pathname !== '/') {
              window.location.href = '/';
            }
          }
        } finally {
          isRefreshing = false;
        }
      }

      return Promise.reject(error);
    }
  );
}