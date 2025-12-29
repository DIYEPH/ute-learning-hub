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

// Trust level enum values: Moderator = 4, Master = 5
const MODERATOR_MIN_LEVEL = 4;

export function AdminShell({ children }: AdminShellProps) {
  const pathname = usePathname();
  const t = useTranslations();
  const { profile } = useUserProfile();
  const { pendingReports, pendingDocumentFiles } = useAdminBadges();

  // Check if user has Admin role (not just trust level)
  const hasAdminRole = profile?.roles?.some(role => role === 'Admin') === true;
  // Check if user has Moderator-level trust
  const isModerator = typeof profile?.trustLevel === 'number' && profile.trustLevel >= MODERATOR_MIN_LEVEL;

  // Filter nav items based on user's permission level
  const filteredNavItems = ADMIN_NAV_CONFIG.filter(item => {
    if (item.minLevel === "Admin") {
      // Admin-level pages require actual Admin role
      return hasAdminRole;
    }
    if (item.minLevel === "Moderator") {
      // Moderator-level pages require Admin role OR Moderator+ trust level
      return hasAdminRole || isModerator;
    }
    return false;
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
    <div className="h-screen overflow-hidden bg-background flex flex-col">
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
