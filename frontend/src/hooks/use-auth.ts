import { useState } from 'react';
import { loginWithMicrosoft, logoutMicrosoft } from '@/src/services/auth/microsoft-auth.service';
import { postApiAuthLogin } from '@/src/api/database/sdk.gen';
import type { LoginWithMicrosoftResponse, LoginResponse } from '@/src/api/database/types.gen';

interface UseAuthReturn {
  handleMicrosoftLogin: () => Promise<LoginWithMicrosoftResponse | null>;
  handleEmailPasswordLogin: (emailOrUsername: string, password: string) => Promise<LoginResponse | null>;
  handleLogout: () => Promise<void>;
  loading: boolean;
  error: string | null;
}

export function useAuth(): UseAuthReturn {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleMicrosoftLogin = async (): Promise<LoginWithMicrosoftResponse | null> => {
    setLoading(true);
    setError(null);
    
    try {
      const result = await loginWithMicrosoft();
      return result;
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Đăng nhập thất bại';
      setError(errorMessage);
      throw err;
    } finally {
      setLoading(false);
    }
  };

  const handleEmailPasswordLogin = async (
    emailOrUsername: string,
    password: string
  ): Promise<LoginResponse | null> => {
    setLoading(true);
    setError(null);
    
    try {
      const response = await postApiAuthLogin({
        body: {
          emailOrUsername,
          password,
        },
      });

      if (response.data) {
        // Lưu tokens vào localStorage
        if (response.data.accessToken) {
          localStorage.setItem('access_token', response.data.accessToken);
        }
        
        if (response.data.refreshToken) {
          localStorage.setItem('refresh_token', response.data.refreshToken);
        }

        return response.data;
      }

      return null;
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Đăng nhập thất bại';
      setError(errorMessage);
      throw err;
    } finally {
      setLoading(false);
    }
  };

  const handleLogout = async (): Promise<void> => {
    setLoading(true);
    setError(null);
    
    try {
      await logoutMicrosoft();
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Đăng xuất thất bại';
      setError(errorMessage);
      throw err;
    } finally {
      setLoading(false);
    }
  };

  return {
    handleMicrosoftLogin,
    handleEmailPasswordLogin,
    handleLogout,
    loading,
    error,
  };
}

