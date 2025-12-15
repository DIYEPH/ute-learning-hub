"use client";

import { usePathname } from "next/navigation";
import { useTranslations } from 'next-intl';
import type { ReactNode } from "react";

import { AppHeader } from "../layout/app-header";
import { AppSidebar } from "../layout/app-sidebar";
import { AppFooter } from "../layout/app-footer";
import { ADMIN_NAV_CONFIG } from "./admin-nav-config";
import type { AdminNavItem } from "./admin-nav-config";
import { useUserProfile } from "@/src/hooks/use-user-profile";
import { useAdminBadges } from "@/src/hooks/use-admin-badges";

type AdminShellProps = {
  children: ReactNode;
};

// Trust levels hierarchy
const ADMIN_LEVEL = "Master"; // Master is the highest trust level, close to admin
const MODERATOR_LEVELS = ["Moderator", "Master"];

export function AdminShell({ children }: AdminShellProps) {
  const pathname = usePathname();
  const t = useTranslations();
  const { profile } = useUserProfile();
  const { pendingReports, pendingDocumentFiles } = useAdminBadges();

  const isMaster = profile?.trustLevel === ADMIN_LEVEL;
  const isModerator = profile?.trustLevel && MODERATOR_LEVELS.includes(profile.trustLevel);

  // Filter nav items based on user's permission level
  const filteredNavItems = ADMIN_NAV_CONFIG.filter(item => {
    if (isMaster) return true; // Master sees everything
    if (isModerator && item.minLevel === "Moderator") return true; // Moderator sees Moderator-level items
    return false; // Hide everything else
  });

  const navItems: AdminNavItem[] = filteredNavItems.map(item => {
    // Attach badges to specific nav items
    let badge: number | undefined;
    if (item.href === "/admin/reports") {
      badge = pendingReports;
    } else if (item.href === "/admin/documents") {
      badge = pendingDocumentFiles;
    }

    return {
      ...item,
      label: t(item.labelKey as any),
      badge,
    };
  });

  return (
    <div className="h-screen overflow-hidden bg-slate-50 dark:bg-slate-950 flex flex-col">
      <div className="flex-shrink-0">
        <AppHeader navItems={navItems} activePath={pathname} />
      </div>

      <div className="flex-1 min-h-0 flex overflow-hidden">
        {/* Desktop Sidebar */}
        <AppSidebar navItems={navItems} activePath={pathname} />

        {/* Main Content */}
        <main className="flex-1 min-h-0 overflow-y-auto p-3 sm:p-4 md:p-6 bg-background">
          {children}
        </main>
      </div>

      <AppFooter />
    </div>
  );
}
