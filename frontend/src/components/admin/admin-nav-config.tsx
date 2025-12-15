import { Users, FileText, BookOpen, Building, GraduationCap, Flag, Bell, MessageCircle, BarChart3, Tag, Hash } from "lucide-react";

export type AdminNavItem = {
  label: string;
  href: string;
  icon: React.ComponentType<{ size?: number }>;
  minLevel?: "Admin" | "Moderator";
  badge?: number;
};

export const ADMIN_NAV_CONFIG = [
  { labelKey: "admin.nav.statistics", href: "/admin/statistics", icon: BarChart3, minLevel: "Admin" },
  { labelKey: "admin.nav.users", href: "/admin/users", icon: Users, minLevel: "Admin" },
  { labelKey: "admin.nav.documents", href: "/admin/documents", icon: FileText, minLevel: "Moderator" },
  { labelKey: "admin.nav.subjects", href: "/admin/subjects", icon: BookOpen, minLevel: "Admin" },
  { labelKey: "admin.nav.faculties", href: "/admin/faculties", icon: Building, minLevel: "Admin" },
  { labelKey: "admin.nav.majors", href: "/admin/majors", icon: GraduationCap, minLevel: "Admin" },
  { labelKey: "admin.nav.types", href: "/admin/types", icon: Tag, minLevel: "Admin" },
  { labelKey: "admin.nav.tags", href: "/admin/tags", icon: Hash, minLevel: "Admin" },
  { labelKey: "admin.nav.notifications", href: "/admin/notifications", icon: Bell, minLevel: "Admin" },
  { labelKey: "admin.nav.conversations", href: "/admin/conversations", icon: MessageCircle, minLevel: "Admin" },
  { labelKey: "admin.nav.reports", href: "/admin/reports", icon: Flag, minLevel: "Moderator" },
] as const;

