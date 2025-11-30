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

  return (
    <div className="min-h-screen bg-slate-50 dark:bg-slate-950 flex flex-col">
      <AppHeader navItems={navItems} activePath={pathname} />

      <div className="flex-1 flex flex-col md:flex-row overflow-hidden">
        {/* Desktop Sidebar */}
        <AppSidebar navItems={navItems} activePath={pathname} />

        {/* Main Content */}
        <main className="flex-1 overflow-y-auto p-3 sm:p-4 md:p-6 bg-background min-w-0 w-full">
          {children}
        </main>
      </div>

      <AppFooter />
    </div>
  );
}

