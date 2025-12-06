'use client';

import { useState } from 'react';
import { login, loginWithMicrosoft, logout } from '@/src/services/auth/auth.service';
import type { LoginResponse, LoginWithMicrosoftResponse } from '@/src/api/database/types.gen';

interface UseAuthReturn {
  handleLogin: (emailOrUsername: string, password: string) => Promise<LoginResponse>;
  handleMicrosoftLogin: () => Promise<LoginWithMicrosoftResponse>;
  handleLogout: () => Promise<void>;
  loading: boolean;
  error: string | null;
}

export function useAuth(): UseAuthReturn {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleLogin = async (emailOrUsername: string, password: string): Promise<LoginResponse> => {
    setLoading(true);
    setError(null);
    try {
      return await login(emailOrUsername, password);
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Login failed';
      setError(message);
      throw err;
    } finally {
      setLoading(false);
    }
  };

  const handleMicrosoftLogin = async (): Promise<LoginWithMicrosoftResponse> => {
    setLoading(true);
    setError(null);
    try {
      return await loginWithMicrosoft();
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Microsoft login failed';
      setError(message);
      throw err;
    } finally {
      setLoading(false);
    }
  };

  const handleLogout = async (): Promise<void> => {
    setLoading(true);
    setError(null);
    try {
      await logout();
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Logout failed';
      setError(message);
      throw err;
    } finally {
      setLoading(false);
    }
  };

  return {
    handleLogin,
    handleMicrosoftLogin,
    handleLogout,
    loading,
    error,
  };
}
