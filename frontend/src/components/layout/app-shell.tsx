"use client";

import { usePathname } from "next/navigation";
import { useTranslations } from 'next-intl';
import type { ReactNode } from "react";

import { AppHeader } from "./app-header";
import { AppSidebar } from "./app-sidebar";
import { MAIN_NAV_CONFIG } from "./nav-config";
import type { NavItem } from "./nav-config";
import { useAuthState } from "@/src/hooks/use-auth-state";
import { ChatWidgetProvider, ChatWidgetContainer, ChatFloatingButton } from "@/src/components/chat-widget";

type AppShellProps = {
  children: ReactNode;
};

export function AppShell({ children }: AppShellProps) {
  const pathname = usePathname();
  const t = useTranslations();
  const { authenticated: isAuthenticated } = useAuthState();

  const navItems: NavItem[] = MAIN_NAV_CONFIG
    .filter(item => !item.requiresAuth || isAuthenticated)
    .map(item => ({
      ...item,
      label: t(item.labelKey as any)
    }));

  // Check if current page is document file viewer (fullscreen mode)
  const isDocumentFileViewer = pathname?.match(/^\/documents\/[^/]+\/files\/[^/]+$/);
  const isChatPage = pathname?.startsWith('/chat');
  const isFullscreenPage = isDocumentFileViewer || isChatPage;

  return (
    <ChatWidgetProvider>
      <div className="h-screen overflow-hidden flex flex-col">
        {/* Header trên cùng */}
        <div className="shrink-0">
          <AppHeader navItems={navItems} activePath={pathname} />
        </div>

        {/* Sidebar + nội dung */}
        <div className="flex-1 min-h-0 flex overflow-hidden">
          <AppSidebar navItems={navItems} activePath={pathname} />

          <main
            className={`flex-1 min-h-0 overflow-hidden ${isFullscreenPage ? '' : 'pt-4 px-4 md:pt-6 md:px-6 overflow-y-auto'}`}
            style={isFullscreenPage ? undefined : {
              backgroundImage: 'url(/images/cover_page.png)',
              backgroundSize: 'cover',
              backgroundPosition: 'center',
              backgroundAttachment: 'fixed'
            }}
          >
            {children}
          </main>
        </div>
      </div>

      {/* Chat Widget*/}
      {isAuthenticated && !isChatPage && (
        <>
          <ChatFloatingButton />
          <ChatWidgetContainer />
        </>
      )}
    </ChatWidgetProvider>
  );
}


