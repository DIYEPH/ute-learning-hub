"use client";

import Link from "next/link";
import Image from "next/image";
import { Button } from "../ui/button";
import { InputWithIcon } from "../ui/input-with-icon";
import { useTranslations } from 'next-intl';
import { LanguageSwitcher } from "./language-switcher";
import { ThemeSwitcher } from "./theme-switcher";
import type { NavItem } from "./nav-config";
import MobileSidebar from "./app-mobile-sidebar";
import { useState } from "react";
import LoginDialog from "../auth/login-dialog";
import { Search } from "lucide-react";
import { useAuthState } from "@/src/hooks/use-auth-state";
import { UserMenu } from "./user-menu";
import { NotificationMenu } from "./notification-menu";
import { EventMenu } from "./event-menu";

type HeaderProps = {
  navItems: NavItem[];
  activePath?: string;
};

export function AppHeader({ navItems, activePath }: HeaderProps) {
  const t = useTranslations('common');
  const tCommon = useTranslations('common');
  const [open, setOpen] = useState(false);
  const isAuthenticated = useAuthState();

  return (
    <header className="h-16 border-b bg-white dark:bg-slate-900 border-slate-200 dark:border-slate-800 flex items-center px-4 gap-4">
      {/* Mobile: nút menu mở sidebar */}
      <div className="md:hidden">
        <MobileSidebar navItems={navItems} activePath={activePath} />
      </div>

      {/* Logo */}
      <Link href="/" className="flex items-center gap-2 font-semibold text-lg text-foreground">
        <div className="relative h-8 w-8 shrink-0">
          <Image
            src="/images/ute_logo.png"
            alt="UTE Logo"
            fill
            className="object-contain"
            priority
          />
        </div>
        <span className="hidden sm:inline">UTE Learning Hub</span>
      </Link>

      {/* Search */}
      <div className="flex-1 max-w-xl mx-auto w-full">
        <InputWithIcon
          prefixIcon={Search}
          placeholder={tCommon('searchPlaceholder')}
          className="rounded-full"
          onPrefixClick={() => {
            // Handle search action
            console.log('Search clicked');
          }}
        />
      </div>

      {/* Auth buttons & Language switcher */}
      <div className="flex items-center gap-1">
        {/* Event/Calendar icon */}
        {isAuthenticated && (
          <div className="hidden sm:block">
            <EventMenu />
          </div>
        )}

        {/* Notification icon */}
        {isAuthenticated && (
          <div className="hidden sm:block">
            <NotificationMenu />
          </div>
        )}

        {/* Theme switcher */}
        <div className="hidden sm:block">
          <ThemeSwitcher />
        </div>

        {/* Language switcher */}
        <div className="hidden sm:block">
          <LanguageSwitcher />
        </div>

        {/* Login/User Avatar */}
        {isAuthenticated ? (
          <UserMenu />
        ) : (
          <>
            <Button
              className="rounded-full bg-green-500 hover:bg-green-600 text-white px-4"
              onClick={() => setOpen(true)}
            >
              {t('login')}
        </Button>
            <LoginDialog open={open} onOpenChange={setOpen} />
          </>
        )}
      </div>
    </header>
  );
}