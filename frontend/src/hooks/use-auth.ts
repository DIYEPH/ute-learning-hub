'use client';

import { useState } from 'react';
import { login, loginWithMicrosoft, logout } from '@/src/services/auth/auth.service';
import type { LoginResponse, LoginWithMicrosoftResponse } from '@/src/api/database/types.gen';

interface UseAuthReturn {
  handleLogin: (emailOrUsername: string, password: string) => Promise<LoginResponse>;
  handleMicrosoftLogin: () => Promise<LoginWithMicrosoftResponse>;
  handleLogout: () => Promise<void>;
  loading: boolean;
}

export function useAuth(): UseAuthReturn {
  const [loading, setLoading] = useState(false);

  const handleLogin = async (emailOrUsername: string, password: string): Promise<LoginResponse> => {
    setLoading(true);
    try {
      return await login(emailOrUsername, password);
    } finally {
      setLoading(false);
    }
  };

  const handleMicrosoftLogin = async (): Promise<LoginWithMicrosoftResponse> => {
    setLoading(true);
    try {
      return await loginWithMicrosoft();
    } finally {
      setLoading(false);
    }
  };

  const handleLogout = async (): Promise<void> => {
    setLoading(true);
    try {
      await logout();
    } finally {
      setLoading(false);
    }
  };

  return {
    handleLogin,
    handleMicrosoftLogin,
    handleLogout,
    loading,
  };
}

