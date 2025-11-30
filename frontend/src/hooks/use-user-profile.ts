"use client";

import { useEffect, useState } from 'react';
import { getApiAccountProfile } from '@/src/api/database/sdk.gen';
import type { GetApiAccountProfileResponse } from '@/src/api/database/types.gen';
import { useAuthState } from './use-auth-state';

export function useUserProfile() {
  const [profile, setProfile] = useState<GetApiAccountProfileResponse | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const isAuthenticated = useAuthState();

  useEffect(() => {
    if (!isAuthenticated) {
      setProfile(null);
      setLoading(false);
      return;
    }

    const fetchProfile = async () => {
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
    };

    fetchProfile();
  }, [isAuthenticated]);

  return { profile, loading, error };
}

