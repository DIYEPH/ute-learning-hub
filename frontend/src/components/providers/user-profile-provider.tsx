"use client";

import { createContext, useContext, useEffect, useState, useCallback, type ReactNode } from 'react';
import { getApiAccountProfile } from '@/src/api';
import type { GetApiAccountProfileResponse } from '@/src/api/database/types.gen';
import { useAuthState } from '@/src/hooks/use-auth-state';

interface UserProfileContextType {
  profile: GetApiAccountProfileResponse | null;
  loading: boolean;
  error: string | null;
  refetch: () => Promise<void>;
}

const UserProfileContext = createContext<UserProfileContextType | null>(null);

export function UserProfileProvider({ children }: { children: ReactNode }) {
  const [profile, setProfile] = useState<GetApiAccountProfileResponse | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const { authenticated: isAuthenticated, ready: authReady } = useAuthState();

  const fetchProfile = useCallback(async () => {
    if (!authReady || !isAuthenticated) {
      setProfile(null);
      setLoading(false);
      return;
    }

    setLoading(true);
    setError(null);
    try {
      const response = await getApiAccountProfile();
      if (response.data) {
        setProfile(response.data);
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to fetch profile');
      console.error('Error fetching profile:', err);
    } finally {
      setLoading(false);
    }
  }, [isAuthenticated, authReady]);

  useEffect(() => {
    if (!authReady) return;

    if (!isAuthenticated) {
      setProfile(null);
      setLoading(false);
      return;
    }

    fetchProfile();
  }, [isAuthenticated, authReady, fetchProfile]);

  return (
    <UserProfileContext.Provider value={{ profile, loading, error, refetch: fetchProfile }}>
      {children}
    </UserProfileContext.Provider>
  );
}

export function useUserProfileContext() {
  const context = useContext(UserProfileContext);
  if (!context) {
    throw new Error('useUserProfileContext must be used within UserProfileProvider');
  }
  return context;
}
