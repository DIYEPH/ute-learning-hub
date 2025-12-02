import { client } from '@/src/api/database/client.gen';

const ACCESS_TOKEN_KEY = 'access_token';
const AUTH_CHANGED_EVENT = 'auth-changed';

export function getAccessToken(): string | undefined {
  if (typeof window === 'undefined') return undefined;
  return localStorage.getItem(ACCESS_TOKEN_KEY) || undefined;
}

export function setAccessToken(token?: string) {
  if (typeof window === 'undefined') return;

  if (token) {
    localStorage.setItem(ACCESS_TOKEN_KEY, token);
  } else {
    localStorage.removeItem(ACCESS_TOKEN_KEY);
  }

  // Phát sự kiện auth-changed để các hook (useAuthState, useUserProfile, ...) cập nhật
  window.dispatchEvent(new Event(AUTH_CHANGED_EVENT));
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

  client.instance.interceptors.response.use(
    (response) => response,
    (error) => {
      const status = error?.response?.status;

      // Nếu server trả 401 thì xóa token, bắn sự kiện auth-changed và chuyển về trang chủ
      if (status === 401) {
        setAccessToken(undefined);

        try {
          if (typeof window !== 'undefined') {
            // Đánh dấu để mở modal login sau khi reload
            window.localStorage.setItem('auth_show_login', '1');

            // Nếu không ở trang chủ thì điều hướng về "/"
            if (window.location.pathname !== '/') {
              window.location.href = '/';
            }
          }
        } catch {
          // ignore
        }
      }

      return Promise.reject(error);
    }
  );
}

export function isAuthenticated(): boolean {
  return !!getAccessToken();
}

export const authEvents = {
  AUTH_CHANGED_EVENT,
};