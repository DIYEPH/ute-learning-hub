"use client";

import { ReactNode, useEffect, useState } from "react";
import { useRouter, usePathname } from "next/navigation";
import { useAuthState } from "@/src/hooks/use-auth-state";
import { useUserProfile } from "@/src/hooks/use-user-profile";
import { AdminShell } from "./admin-shell";
import { useTranslations } from 'next-intl';

type AdminLayoutProps = {
  children: ReactNode;
};

// Trust levels that can access admin panel
const ADMIN_TRUST_LEVELS = ["Moderator", "Master"];
// Pages Moderator can access
const MODERATOR_ALLOWED_PATHS = ["/admin/reports", "/admin/documents"];

export function AdminLayout({ children }: AdminLayoutProps) {
  const { authenticated: isAuthenticated, ready: authReady } = useAuthState();
  const { profile, loading: profileLoading } = useUserProfile();
  const router = useRouter();
  const pathname = usePathname();
  const t = useTranslations('common');
  const isModerator = profile?.trustLevel === "Moderator";
  const canAccessAdmin = profile?.trustLevel && ADMIN_TRUST_LEVELS.includes(profile.trustLevel);

  useEffect(() => {
    if (!authReady || profileLoading) return;

    // Not authenticated or no admin access -> redirect home
    if (!isAuthenticated || !canAccessAdmin) {
      router.push('/');
      return;
    }

    // Moderator accessing Master-only page -> redirect to reports
    if (isModerator && pathname) {
      const isAllowedPath = MODERATOR_ALLOWED_PATHS.some(p => pathname.startsWith(p));
      if (!isAllowedPath) {
        router.push('/admin/reports');
        return;
      }
    }

  }, [isAuthenticated, canAccessAdmin, isModerator, pathname, profileLoading, router, authReady]);

  if (!authReady || profileLoading) {
    return (
      <AdminShell>
        <div className="flex items-center justify-center min-h-screen">
          <p className="text-slate-600 dark:text-slate-400">{t('loading') || 'Đang tải...'}</p>
        </div>
      </AdminShell>
    );
  }

  if (!isAuthenticated || !canAccessAdmin) {
    return (
      <AdminShell>
        <div className="flex items-center justify-center min-h-screen">
          <p className="text-slate-600 dark:text-slate-400">{t('loading') || 'Đang tải...'}</p>
        </div>
      </AdminShell>
    );
  }

  return <AdminShell>{children}</AdminShell>;
}
