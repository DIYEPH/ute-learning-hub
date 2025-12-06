import { Users, FileText, BookOpen, Building, GraduationCap, Flag, Bell, MessageCircle, BarChart3, Tag, Hash } from "lucide-react";

export type AdminNavItem = {
  label: string;
  href: string;
  icon: React.ComponentType<{ size?: number }>;
};

export const ADMIN_NAV_CONFIG = [
  { labelKey: "admin.nav.statistics", href: "/admin/statistics", icon: BarChart3 },
  { labelKey: "admin.nav.users", href: "/admin/users", icon: Users },
  { labelKey: "admin.nav.documents", href: "/admin/documents", icon: FileText },
  { labelKey: "admin.nav.subjects", href: "/admin/subjects", icon: BookOpen },
  { labelKey: "admin.nav.faculties", href: "/admin/faculties", icon: Building },
  { labelKey: "admin.nav.majors", href: "/admin/majors", icon: GraduationCap },
  { labelKey: "admin.nav.types", href: "/admin/types", icon: Tag },
  { labelKey: "admin.nav.tags", href: "/admin/tags", icon: Hash },
  { labelKey: "admin.nav.notifications", href: "/admin/notifications", icon: Bell },
  { labelKey: "admin.nav.conversations", href: "/admin/conversations", icon: MessageCircle },
  { labelKey: "admin.nav.reports", href: "/admin/reports", icon: Flag },
] as const;

