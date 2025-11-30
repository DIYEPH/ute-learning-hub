"use client";

import { useUserProfile } from './use-user-profile';
import { useAuthState } from './use-auth-state';

export function useAdmin() {
  const isAuthenticated = useAuthState();
  const { profile, loading } = useUserProfile();

  const isAdmin = isAuthenticated && profile?.roles?.some(role => role === 'Admin') === true;

  return {
    isAdmin,
    isLoading: isAuthenticated && (loading || !profile),
  };
}

