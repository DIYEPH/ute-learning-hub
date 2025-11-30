"use client";

import { useEffect, useState } from 'react';
import { isAuthenticated } from '@/src/api/client';

export function useAuthState() {
  const [authenticated, setAuthenticated] = useState(false);
  const [mounted, setMounted] = useState(false);

  useEffect(() => {
    setMounted(true);
    setAuthenticated(isAuthenticated());

    const checkAuth = () => {
      setAuthenticated(isAuthenticated());
    };

    window.addEventListener('storage', checkAuth);
    const interval = setInterval(checkAuth, 1000);
    
    return () => {
      window.removeEventListener('storage', checkAuth);
      clearInterval(interval);
    };
  }, []);

  if (!mounted) {
    return false;
  }

  return authenticated;
}
