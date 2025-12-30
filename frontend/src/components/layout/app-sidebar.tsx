"use client";

import Link from "next/link";
import { useTranslations } from 'next-intl';
import { Info, Shield, Home } from "lucide-react";
import { ScrollArea } from "../ui/scroll-area"
import { Button } from "../ui/button";
import type { NavItem } from "./nav-config";
import type { AdminNavItem } from "../admin/admin-nav-config";
import { useUserProfile } from "@/src/hooks/use-user-profile";

type SidebarProps = {
  navItems: (NavItem | AdminNavItem)[];
  activePath?: string;
};

// Trust levels that can access admin features (Moderator = 4, Master = 5)
const MODERATOR_MIN_LEVEL = 4;

export function AppSidebar({ navItems, activePath }: SidebarProps) {
  const t = useTranslations('common');
  const tNav = useTranslations('nav');
  const { profile } = useUserProfile();

  const canAccessAdmin = typeof profile?.trustLevel === 'number' && profile.trustLevel >= MODERATOR_MIN_LEVEL;
  const isInAdminPanel = activePath?.startsWith("/admin");

  return (
    <aside className="hidden md:flex w-72 shrink-0 border-r bg-card border-border h-full overflow-hidden flex-col">
      <ScrollArea className="w-full flex-1">
        <div className="p-4 space-y-4">
          {/* Show upload button only in user pages */}
          {!isInAdminPanel && (
            <Link href="/documents/upload">
              <Button className="w-full rounded-full mt-2">{t('new-document')}</Button>
            </Link>
          )}

          {/* Nav */}
          <nav className="mt-4 space-y-1 text-sm">
            {navItems.map((item) => {
              const Icon = item.icon;
              const isActive = activePath === item.href;
              const badge = 'badge' in item ? item.badge : undefined;
              return (
                <Link key={item.href} href={item.href}>
                  <button
                    className={`flex w-full items-center gap-3 px-3 py-2 text-left transition-colors ${isActive
                      ? "bg-muted text-foreground font-medium"
                      : "text-muted-foreground hover:bg-muted"
                      }`}
                  >
                    <span className="shrink-0">
                      <Icon size={18} />
                    </span>
                    <span className="flex-1">{item.label}</span>
                    {badge !== undefined && badge > 0 && (
                      <span className="shrink-0 min-w-5 h-5 px-1.5 text-xs font-medium bg-red-500 text-white rounded-full flex items-center justify-center">
                        {badge > 99 ? "99+" : badge}
                      </span>
                    )}
                  </button>
                </Link>
              );
            })}
          </nav>

          {/* Admin/User toggle */}
          {canAccessAdmin && (
            <div className="pt-4 border-t">
              {isInAdminPanel ? (
                <Link href="/">
                  <button className="flex w-full items-center gap-3 px-3 py-2 text-left transition-colors text-muted-foreground hover:bg-muted">
                    <Home size={18} />
                    <span>Trang chủ</span>
                  </button>
                </Link>
              ) : (
                <Link href="/admin">
                  <button className="flex w-full items-center gap-3 px-3 py-2 text-left transition-colors text-muted-foreground hover:bg-muted">
                    <Shield size={18} />
                    <span>Quản trị</span>
                  </button>
                </Link>
              )}
            </div>
          )}
        </div>
      </ScrollArea>

      {/* Bottom link */}
      <div className="p-3">
        <Link href="/about">
          <button
            className={`flex w-full items-center gap-3 px-3 py-2 text-sm text-left transition-colors ${activePath === "/about"
              ? "bg-muted text-foreground font-medium"
              : "text-muted-foreground hover:bg-muted"
              }`}
          >
            <Info size={18} />
            <span>{tNav('about')}</span>
          </button>
        </Link>
      </div>
    </aside>
  );
}
