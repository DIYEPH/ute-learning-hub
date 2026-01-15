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
      setState({ ready: true, authenticated: isAuthenticated() });
    };
    checkAuth();
    window.addEventListener('storage', checkAuth);
    window.addEventListener(authEvents.AUTH_CHANGED_EVENT, checkAuth);
    return () => {
      window.removeEventListener('storage', checkAuth);
      window.removeEventListener(authEvents.AUTH_CHANGED_EVENT, checkAuth);
    };
  }, []);

  return state;
}