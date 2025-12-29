"use client";

import { Menu, Info, Plus, Bell, Calendar, Star, Sun, Moon, Globe, Shield } from "lucide-react";
import { Button } from "../ui/button";
import { Sheet, SheetContent, SheetHeader, SheetTitle, SheetTrigger } from "../ui/sheet";
import { NavItem } from "./nav-config";
import { ScrollArea } from "../ui/scroll-area";
import Link from "next/link";
import { useTranslations } from "next-intl";
import { useAuthState } from "@/src/hooks/use-auth-state";
import { useTheme } from "../providers/theme-provider";
import { useRouter, usePathname } from "next/navigation";
import { useUserProfile } from "@/src/hooks/use-user-profile";
import { useState } from "react";

// Trust levels that can access admin features (Moderator = 4, Master = 5)
const MODERATOR_MIN_LEVEL = 4;

export default function MobileSidebar({
  navItems,
  activePath,
}: {
  navItems: NavItem[];
  activePath?: string;
}) {
  const t = useTranslations("nav");
  const tCommon = useTranslations("common");
  const { authenticated: isAuthenticated } = useAuthState();
  const { profile } = useUserProfile();
  const { theme, setTheme } = useTheme();
  const router = useRouter();
  const pathname = usePathname();
  const [open, setOpen] = useState(false);

  const currentLocale = pathname?.startsWith('/vi') ? 'vi' : 'en';

  const toggleTheme = () => {
    setTheme(theme === 'dark' ? 'light' : 'dark');
  };

  const toggleLanguage = () => {
    const newLocale = currentLocale === 'vi' ? 'en' : 'vi';
    const newPath = pathname?.replace(/^\/(vi|en)/, `/${newLocale}`) || `/${newLocale}`;
    router.push(newPath);
  };

  const handleLinkClick = () => {
    setOpen(false);
  };

  return (
    <Sheet open={open} onOpenChange={setOpen}>
      <SheetTrigger asChild>
        <Button variant="ghost" size="icon">
          <Menu />
        </Button>
      </SheetTrigger>
      <SheetContent side="left" className="p-0 w-72 flex flex-col">
        <SheetHeader className="p-4 border-b">
          <SheetTitle>UTE Learning Hub</SheetTitle>
        </SheetHeader>
        <ScrollArea className="flex-1">
          <div className="p-4 space-y-4">
            {/* Create Document Button */}
            {isAuthenticated && (
              <Link href="/documents/upload" onClick={handleLinkClick}>
                <Button className="w-full rounded-full">
                  <Plus className="h-4 w-4 mr-2" />
                  {tCommon('new-document')}
                </Button>
              </Link>
            )}

            {/* Trust Score */}
            {/* {isAuthenticated && profile && (
              <div className="flex items-center gap-2 px-3 py-2 text-sm text-slate-600 dark:text-slate-400">
                <Star className="h-4 w-4 text-amber-500" />
                <span>{profile.trustScore || 0} điểm uy tín</span>
              </div>
            )} */}

            {/* Quick Actions for authenticated users */}
            {isAuthenticated && (
              <div className="flex gap-2 pt-2">
                <Link href="/notifications" className="flex-1" onClick={handleLinkClick}>
                  <Button variant="outline" size="sm" className="w-full">
                    <Bell className="h-4 w-4 mr-1" />
                    Thông báo
                  </Button>
                </Link>
                <Link href="/events" className="flex-1" onClick={handleLinkClick}>
                  <Button variant="outline" size="sm" className="w-full">
                    <Calendar className="h-4 w-4 mr-1" />
                    Sự kiện
                  </Button>
                </Link>
              </div>
            )}

            {/* Navigation */}
            <nav className="space-y-1 text-sm">
              {navItems.map((item) => {
                const Icon = item.icon;
                const isActive = activePath === item.href;
                return (
                  <Link key={item.href} href={item.href} onClick={handleLinkClick}>
                    <button
                      className={`flex w-full items-center gap-3 px-3 py-2 rounded-lg text-left transition-colors ${isActive
                        ? "bg-muted text-foreground font-medium"
                        : "text-muted-foreground hover:bg-muted"
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

              {/* About link */}
              <Link href="/about" onClick={handleLinkClick}>
                <button
                  className={`flex w-full items-center gap-3 px-3 py-2 rounded-lg text-left transition-colors ${activePath === "/about"
                    ? "bg-muted text-foreground font-medium"
                    : "text-muted-foreground hover:bg-muted"
                    }`}
                >
                  <span className="shrink-0">
                    <Info size={18} />
                  </span>
                  <span>{t("about")}</span>
                </button>
              </Link>

              {/* Admin link for Moderator+ */}
              {typeof profile?.trustLevel === 'number' && profile.trustLevel >= MODERATOR_MIN_LEVEL && (
                <Link href="/admin" onClick={handleLinkClick}>
                  <button
                    className={`flex w-full items-center gap-3 px-3 py-2 rounded-lg text-left transition-colors ${activePath?.startsWith("/admin")
                      ? "bg-amber-100 dark:bg-amber-900/30 text-amber-700 dark:text-amber-300 font-medium"
                      : "text-amber-600 dark:text-amber-400 hover:bg-amber-50 dark:hover:bg-amber-900/20"
                      }`}
                  >
                    <span className="shrink-0">
                      <Shield size={18} />
                    </span>
                    <span>Quản trị</span>
                  </button>
                </Link>
              )}
            </nav>

            {/* Settings Section */}
            <div className="pt-4 border-t space-y-2">
              <p className="text-xs text-muted-foreground uppercase tracking-wider px-3">Cài đặt</p>

              {/* Theme Toggle */}
              <button
                onClick={toggleTheme}
                className="flex w-full items-center gap-3 px-3 py-2 rounded-lg text-left text-muted-foreground hover:bg-muted transition-colors"
              >
                {theme === 'dark' ? <Sun size={18} /> : <Moon size={18} />}
                <span className="text-sm">{theme === 'dark' ? 'Sáng' : 'Tối'}</span>
              </button>

              {/* Language Toggle */}
              <button
                onClick={toggleLanguage}
                className="flex w-full items-center gap-3 px-3 py-2 rounded-lg text-left text-muted-foreground hover:bg-muted transition-colors"
              >
                <Globe size={18} />
                <span className="text-sm">{currentLocale === 'vi' ? 'GB' : 'VN'}</span>
              </button>
            </div>
          </div>
        </ScrollArea>
      </SheetContent>
    </Sheet>
  );
}
