"use client";

import { useTranslations } from 'next-intl';
import Link from "next/link";
import { useRouter } from "next/navigation";
import { User, LogOut } from "lucide-react";
import { Avatar, AvatarImage, AvatarFallback } from "../ui/avatar";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "../ui/dropdown-menu";
import { Button } from "../ui/button";
import { useUserProfile } from "@/src/hooks/use-user-profile";
import { useAuth } from "@/src/hooks/use-auth";
import { useDropdown } from "@/src/hooks/use-dropdown";

export function UserMenu() {
  const t = useTranslations('common');
  const tNav = useTranslations('nav');
  const { profile } = useUserProfile();
  const { handleLogout, loading } = useAuth();
  const router = useRouter();
  const dropdown = useDropdown();

  const handleLogoutClick = async () => {
    try {
      await handleLogout();
      dropdown.closeDropdown();
      // Full page reload và redirect về homepage để clear all cached data
      window.location.href = '/';
    } catch (error) {
      console.error('Logout error:', error);
    }
  };

  const getInitials = (name: string) => {
    return name
      .split(' ')
      .map((n) => n[0])
      .join('')
      .toUpperCase()
      .slice(0, 2);
  };

  return (
    <DropdownMenu open={dropdown.open} onOpenChange={dropdown.setOpen}>
      <DropdownMenuTrigger asChild>
        <Button
          variant="ghost"
          size="sm"
          className="h-9 w-9 rounded-full p-0 hover:bg-muted"
          aria-label="User menu"
        >
          <Avatar className="h-9 w-9">
            <AvatarImage
              src={profile?.avatarUrl || undefined}
              alt={profile?.fullName || 'User'}
            />
            <AvatarFallback className="bg-muted text-muted-foreground">
              {profile?.fullName ? (
                getInitials(profile.fullName)
              ) : (
                <User size={18} />
              )}
            </AvatarFallback>
          </Avatar>
        </Button>
      </DropdownMenuTrigger>
      <DropdownMenuContent align="end" className="w-56">
        <div className="px-2 py-1.5">
          <p className="text-sm font-medium">{profile?.fullName || 'User'}</p>
          <p className="text-xs text-muted-foreground truncate">
            {profile?.email}
          </p>
        </div>
        <DropdownMenuSeparator />
        <DropdownMenuItem asChild>
          <Link href="/profile" className="cursor-pointer">
            <User size={16} className="mr-2" />
            {tNav('profile')}
          </Link>
        </DropdownMenuItem>
        <DropdownMenuSeparator />
        <DropdownMenuItem
          onClick={handleLogoutClick}
          disabled={loading}
          className="text-red-600 dark:text-red-400 cursor-pointer"
        >
          <LogOut size={16} className="mr-2" />
          {loading ? (t('loggingOut') || 'Đang đăng xuất...') : t('logout')}
        </DropdownMenuItem>
      </DropdownMenuContent>
    </DropdownMenu>
  );
}


