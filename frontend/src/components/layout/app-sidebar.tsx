"use client";

import Link from "next/link";
import { useTranslations } from 'next-intl';
import { ScrollArea} from "../ui/scroll-area"
import { Button } from "../ui/button";
import { Avatar, AvatarFallback } from "../ui/avatar"
import type { NavItem } from "./nav-config";
import type { AdminNavItem } from "../admin/admin-nav-config";

type SidebarProps = {
  navItems: (NavItem | AdminNavItem)[];
  activePath?: string;
};

export function AppSidebar({ navItems, activePath }: SidebarProps) {
  const t = useTranslations('common');

  return (
    <aside className="hidden md:flex w-64 border-r bg-white dark:bg-slate-900 border-slate-200 dark:border-slate-800">
      <ScrollArea className="w-full">
        <div className="p-4 space-y-4">
          {/* User box */}
          {/* <div className="flex items-center gap-3">
            <Avatar>
              <AvatarFallback>GU</AvatarFallback>
            </Avatar>
            <div className="text-sm">
              <div className="font-semibold">{t('guestUser')}</div>
              <button className="text-xs text-sky-600 dark:text-sky-400">
                {t('addUniversity')}
              </button>
            </div>
          </div>*/}

          <Link href="/documents/upload">
            <Button className="w-full rounded-full mt-2">{t('new-document')}</Button>
          </Link> 

          {/* Nav */}
          <nav className="mt-4 space-y-1 text-sm">
            {navItems.map((item) => {
              const Icon = item.icon;
              const isActive = activePath === item.href;
              return (
                <Link key={item.href} href={item.href}>
                  <button
                    className={`flex w-full items-center gap-3 rounded-lg px-3 py-2 text-left transition-colors ${
                      isActive
                        ? "bg-slate-100 dark:bg-slate-800 text-slate-900 dark:text-slate-100 font-medium"
                        : "text-slate-600 dark:text-slate-400 hover:bg-slate-100 dark:hover:bg-slate-800"
                    }`}
                  >
                    <span className="shrink-0">
                      <Icon size={18} />
                    </span>
                    <span>{item.label}</span>
                  </button>
                </Link>
              );
            })}
          </nav>
        </div>
      </ScrollArea>
    </aside>
  );
}
