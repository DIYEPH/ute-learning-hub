"use client";

import { ReactNode, useEffect } from "react";
import { useRouter, usePathname } from "next/navigation";
import { useAuthState } from "@/src/hooks/use-auth-state";
import { useUserProfile } from "@/src/hooks/use-user-profile";
import { AdminShell } from "./admin-shell";
import { useTranslations } from "next-intl";

type AdminLayoutProps = { children: ReactNode };
const MODERATOR_MIN_LEVEL = 4;
const MODERATOR_ALLOWED_PATHS = ["/admin/reports", "/admin/documents"];

export function AdminLayout({ children }: AdminLayoutProps) {
  const { authenticated, ready: authReady } = useAuthState();
  const { profile, loading: profileLoading } = useUserProfile();
  const router = useRouter();
  const pathname = usePathname();
  const t = useTranslations("common");

  const trustLevel = profile?.trustLevel;
  const hasAdminRole = profile?.roles?.some(role => role === "Admin") === true;
  const hasModeratorLevel = typeof trustLevel === "number" && trustLevel >= MODERATOR_MIN_LEVEL;
  const canAccessAdmin = hasAdminRole || hasModeratorLevel;

  useEffect(() => {
    if (!authReady || profileLoading) return;
    if (!authenticated || !canAccessAdmin) { router.replace("/"); return; }
    if (!hasAdminRole && hasModeratorLevel && pathname) {
      const allowed = MODERATOR_ALLOWED_PATHS.some(p => pathname.startsWith(p));
      if (!allowed) router.replace("/admin/reports");
    }
  }, [authenticated, canAccessAdmin, hasAdminRole, hasModeratorLevel, pathname, profileLoading, router, authReady]);

  if (!authReady || profileLoading) return <AdminShell><div className="flex items-center justify-center min-h-screen"><p className="text-muted-foreground">{t("loading") || "Đang tải..."}</p></div></AdminShell>;
  if (!authenticated || !canAccessAdmin) return null;

  return <AdminShell>{children}</AdminShell>;
}