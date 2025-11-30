"use client";

import { usePathname } from "next/navigation";
import { useTranslations } from 'next-intl';
import type { ReactNode } from "react";

import { AppHeader } from "../layout/app-header";
import { AppSidebar } from "../layout/app-sidebar";
import { AppFooter } from "../layout/app-footer";
import { ADMIN_NAV_CONFIG } from "./admin-nav-config";
import type { AdminNavItem } from "./admin-nav-config";

type AdminShellProps = {
  children: ReactNode;
};

export function AdminShell({ children }: AdminShellProps) {
  const pathname = usePathname();
  const t = useTranslations();

  const navItems: AdminNavItem[] = ADMIN_NAV_CONFIG.map(item => ({
    ...item,
    label: t(item.labelKey as any)
  }));

  const mainNavItems: any[] = [];

  return (
    <div className="min-h-screen bg-slate-50 dark:bg-slate-950 flex flex-col">
      <AppHeader navItems={mainNavItems} activePath={pathname} />

      <div className="flex-1 flex">
        <AppSidebar navItems={navItems} activePath={pathname} />

        <main className="flex-1 p-4 md:p-6 bg-background">{children}</main>
      </div>

      <AppFooter />
    </div>
  );
}

