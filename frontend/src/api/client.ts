import { client } from '@/src/api/database/client.gen';

const ACCESS_TOKEN_KEY = 'access_token';
const AUTH_CHANGED_EVENT = 'auth-changed';

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

// ============ Axios Configuration ============
client.setConfig({
  baseURL: process.env.NEXT_PUBLIC_API_URL || 'https://localhost:7080',
});

if (typeof window !== 'undefined') {
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

  // Response interceptor: Handle 401
  client.instance.interceptors.response.use(
    (response) => response,
    (error) => {
      if (error?.response?.status === 401) {
        setAccessToken(undefined);

        // Redirect to home if not already there
        if (window.location.pathname !== '/') {
          window.location.href = '/';
        }
      }
      return Promise.reject(error);
    }
  );
}