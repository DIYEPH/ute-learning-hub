"use client";

import { useUserProfileContext } from '@/src/components/providers/user-profile-provider';

export function useUserProfile() {
  return useUserProfileContext();
}