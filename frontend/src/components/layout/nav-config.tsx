import { Home, Library, MessageCircle, Sparkles, Clock } from "lucide-react";

export type NavItem = {
  label: string;
  href: string;
  icon: React.ComponentType<{ size?: number }>;
};

export const MAIN_NAV: NavItem[] = [
  { label: "Home", href: "/", icon: Home },
  { label: "My Library", href: "/library", icon: Library },
  { label: "AI Notes", href: "/ai-notes", icon: Sparkles },
  { label: "Ask AI", href: "/ask-ai", icon: MessageCircle },
  { label: "Recent", href: "/recent", icon: Clock },
];
