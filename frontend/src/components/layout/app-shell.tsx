"use client";

import { usePathname } from "next/navigation";
import type { ReactNode } from "react";

import { AppHeader } from "./app-header";
import { AppSidebar } from "./app-sidebar";
import { AppFooter } from "./app-footer";
import { MAIN_NAV } from "./nav-config";

type AppShellProps = {
  children: ReactNode;
};

export function AppShell({ children }: AppShellProps) {
  const pathname = usePathname();

  return (
    <div className="min-h-screen bg-slate-50 flex flex-col">
      {/* Header trên cùng */}
      <AppHeader navItems={MAIN_NAV} activePath={pathname} />

      {/* Sidebar + nội dung */}
      <div className="flex-1 flex">
        <AppSidebar navItems={MAIN_NAV} activePath={pathname} />

        <main className="flex-1 p-4 md:p-6">{children}</main>
      </div>

      {/* Footer cuối trang */}
      <AppFooter />
    </div>
  );
}
