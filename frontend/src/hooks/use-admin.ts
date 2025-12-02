"use client";

import { useUserProfile } from './use-user-profile';
import { useAuthState } from './use-auth-state';

export function useAdmin() {
  const { authenticated: isAuthenticated, ready: authReady } = useAuthState();
  const { profile, loading } = useUserProfile();

  const isAdmin = isAuthenticated && profile?.roles?.some(role => role === 'Admin') === true;

  return {
    isAdmin,
    isLoading: (!authReady) || (isAuthenticated && (loading || !profile)),
  };
}

