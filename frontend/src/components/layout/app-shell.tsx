"use client";

import { usePathname } from "next/navigation";
import { useTranslations } from 'next-intl';
import type { ReactNode } from "react";

import { AppHeader } from "./app-header";
import { AppSidebar } from "./app-sidebar";
import { AppFooter } from "./app-footer";
import { MAIN_NAV_CONFIG } from "./nav-config";
import type { NavItem } from "./nav-config";
import { useAuthState } from "@/src/hooks/use-auth-state";

type AppShellProps = {
  children: ReactNode;
};

export function AppShell({ children }: AppShellProps) {
  const pathname = usePathname();
  const t = useTranslations();
  const { authenticated: isAuthenticated, ready: authReady } = useAuthState();

  // Generate nav items with translations and filter by auth
  const navItems: NavItem[] = MAIN_NAV_CONFIG
    .filter(item => !item.requiresAuth || isAuthenticated)
    .map(item => ({
      ...item,
      label: t(item.labelKey as any)
    }));

  return (
    <div className="h-screen overflow-hidden bg-slate-50 dark:bg-slate-950 flex flex-col">
      {/* Header trên cùng */}
      <div className="flex-shrink-0">
        <AppHeader navItems={navItems} activePath={pathname} />
      </div>

      {/* Sidebar + nội dung */}
      <div className="flex-1 min-h-0 flex overflow-hidden">
        <AppSidebar navItems={navItems} activePath={pathname} />

        <main className={`flex-1 min-h-0 overflow-hidden ${pathname?.startsWith('/chat') ? '' : 'p-4 md:p-6 bg-background overflow-y-auto'}`}>
          {children}
        </main>
      </div>

      {/* Footer cuối trang */}
      {!pathname?.startsWith('/chat') && (
        <div className="flex-shrink-0">
          <AppFooter />
        </div>
      )}
    </div>
  );
}
