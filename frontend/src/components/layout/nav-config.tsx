import { Home, Library, MessageCircle, Clock, Brain } from "lucide-react";

export type NavItem = {
  label: string;
  href: string;
  icon: React.ComponentType<{ size?: number }>;
};

// Base nav config (without translations)
export const MAIN_NAV_CONFIG = [
  { labelKey: "nav.home", href: "/", icon: Home },
  { labelKey: "nav.myLibrary", href: "/library", icon: Library },
  { labelKey: "nav.chat", href: "/chat", icon: MessageCircle },
  { labelKey: "nav.askAI", href: "/ask-ai", icon: Brain },
  { labelKey: "nav.recent", href: "/recent", icon: Clock },
] as const;
