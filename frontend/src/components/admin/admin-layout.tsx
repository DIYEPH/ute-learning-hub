"use client";

import { ReactNode, useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { useAdmin } from "@/src/hooks/use-admin";
import { useAuthState } from "@/src/hooks/use-auth-state";
import { AdminShell } from "./admin-shell";
import { useTranslations } from 'next-intl';

type AdminLayoutProps = {
  children: ReactNode;
};

export function AdminLayout({ children }: AdminLayoutProps) {
  const { isAdmin, isLoading } = useAdmin();
  const isAuthenticated = useAuthState();
  const router = useRouter();
  const t = useTranslations('common');
  const [isChecking, setIsChecking] = useState(true);

  useEffect(() => {
    const timer = setTimeout(() => {
      setIsChecking(false);
    }, 100);

    return () => clearTimeout(timer);
  }, []);

  useEffect(() => {
    if (isChecking) return;

    if (!isAuthenticated || !isLoading && !isAdmin) {
      router.push('/');
      return;
    }
    
  }, [isAuthenticated, isAdmin, isLoading, router, isChecking]);

  if (isChecking || isLoading) {
    return (
      <AdminShell>
        <div className="flex items-center justify-center min-h-screen">
          <p className="text-slate-600 dark:text-slate-400">{t('loading') || 'Đang tải...'}</p>
        </div>
      </AdminShell>
    );
  }

  if (!isAuthenticated || (!isLoading && !isAdmin)) {
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

