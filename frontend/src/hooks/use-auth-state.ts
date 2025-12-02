"use client";

import { useEffect, useState } from 'react';
import { authEvents, isAuthenticated } from '@/src/api/client';

export function useAuthState() {
  const [state, setState] = useState<{ ready: boolean; authenticated: boolean }>({
    ready: false,
    authenticated: false,
  });

  useEffect(() => {
    const checkAuth = () => {
      setState({
        ready: true,
        authenticated: isAuthenticated(),
      });
    };

    // Kiểm tra ngay khi mount trên client
    checkAuth();

    // Lắng nghe thay đổi token từ các tab khác (storage)
    window.addEventListener('storage', checkAuth);
    // Và từ chính app (auth-changed)
    window.addEventListener(authEvents.AUTH_CHANGED_EVENT, checkAuth);

    return () => {
      window.removeEventListener('storage', checkAuth);
      window.removeEventListener(authEvents.AUTH_CHANGED_EVENT, checkAuth);
    };
  }, []);

  return state;
}
