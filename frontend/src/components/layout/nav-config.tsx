import { Home, Library, MessageCircle, Clock, Users } from "lucide-react";

export type NavItem = {
  label: string;
  href: string;
  icon: React.ComponentType<{ size?: number }>;
  requiresAuth?: boolean;
};

export const MAIN_NAV_CONFIG = [
  { labelKey: "nav.home", href: "/", icon: Home, requiresAuth: false },
  { labelKey: "nav.myLibrary", href: "/library", icon: Library, requiresAuth: true },
  { labelKey: "nav.chat", href: "/chat", icon: MessageCircle, requiresAuth: true },
  { labelKey: "nav.conversations", href: "/conversations", icon: Users, requiresAuth: true },
  { labelKey: "nav.recent", href: "/recent", icon: Clock, requiresAuth: true },
] as const;

