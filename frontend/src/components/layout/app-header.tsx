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
import { useEffect, useState } from "react";
import { useRouter, useSearchParams, usePathname } from "next/navigation";
import LoginDialog from "../auth/login-dialog";
import { Search } from "lucide-react";
import { useAuthState } from "@/src/hooks/use-auth-state";
import { UserMenu } from "./user-menu";
import { NotificationMenu } from "./notification-menu";
import { EventMenu } from "./event-menu";
import { TrustPointsBadge } from "./trust-points-badge";

type HeaderProps = {
  navItems: NavItem[];
  activePath?: string;
};

export function AppHeader({ navItems, activePath }: HeaderProps) {
  const t = useTranslations('common');
  const tCommon = useTranslations('common');
  const [open, setOpen] = useState(false);
  const router = useRouter();
  const searchParams = useSearchParams();
  const pathname = usePathname();

  const [searchValue, setSearchValue] = useState(searchParams.get("q") || "");
  const { authenticated: isAuthenticated, ready: authReady } = useAuthState();

  useEffect(() => {
    if (pathname === "/search") {
      setSearchValue(searchParams.get("q") || "");
    }
  }, [pathname, searchParams]);

  useEffect(() => {
    if (typeof window === 'undefined') return;

    const KEY = 'auth_show_login';
    if (window.localStorage.getItem(KEY) === '1') {
      window.localStorage.removeItem(KEY);
      setOpen(true);
    }
  }, []);

  const handleSearch = () => {
    if (searchValue.trim()) {
      router.push(`/search?q=${encodeURIComponent(searchValue.trim())}`);
    }
  };

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === "Enter") {
      handleSearch();
    }
  };

  return (
    <header className="h-16 border-b bg-card border-border flex items-center px-4 gap-4">
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
            sizes="32px"
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
          value={searchValue}
          onChange={(e) => setSearchValue(e.target.value)}
          onKeyDown={handleKeyDown}
          onPrefixClick={handleSearch}
        />
      </div>

      {/* Auth buttons & Language switcher */}
      <div className="flex items-center gap-1">
        {/* Theme switcher */}
        <div className="hidden sm:block">
          <ThemeSwitcher />
        </div>

        {/* Language switcher */}
        <div className="hidden sm:block">
          <LanguageSwitcher />
        </div>

        {/* Auth-dependent actions */}
        {authReady && (
          <>
            {/* Trust Points Badge */}
            {isAuthenticated && (
              <div className="hidden sm:block">
                <TrustPointsBadge />
              </div>
            )}

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

            {/* Login/User Avatar */}
            {isAuthenticated ? (
              <UserMenu />
            ) : (
              <>
                <Button
                  className="rounded-full bg-primary hover:bg-primary/90 text-primary-foreground px-4"
                  onClick={() => setOpen(true)}
                >
                  {t('login')}
                </Button>
                <LoginDialog open={open} onOpenChange={setOpen} />
              </>
            )}
          </>
        )}
      </div>
    </header>
  );
}
