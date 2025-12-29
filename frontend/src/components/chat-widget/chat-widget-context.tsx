"use client";

import { createContext, useContext, useState, useCallback, type ReactNode } from "react";
import type { ConversationDto } from "@/src/api/database/types.gen";

export interface ChatWidget {
  id: string;
  conversation: ConversationDto;
  isMinimized: boolean;
  hasUnread: boolean;
}

interface ChatWidgetContextType {
  widgets: ChatWidget[];
  openChat: (conversation: ConversationDto) => void;
  closeChat: (conversationId: string) => void;
  toggleMinimize: (conversationId: string) => void;
  minimizeAll: () => void;
  markAsRead: (conversationId: string) => void;
  markAsUnread: (conversationId: string) => void;
  bringToFront: (conversationId: string) => void;
}

const ChatWidgetContext = createContext<ChatWidgetContextType | undefined>(undefined);

const MAX_WIDGETS = 3; // Maximum number of open chat widgets

export function ChatWidgetProvider({ children }: { children: ReactNode }) {
  const [widgets, setWidgets] = useState<ChatWidget[]>([]);

  const openChat = useCallback((conversation: ConversationDto) => {
    if (!conversation.id) return;

    const conversationId = conversation.id;

    setWidgets((prev) => {
      // If already open, just bring to front and unminimize
      const existing = prev.find((w) => w.id === conversationId);
      if (existing) {
        return prev
          .filter((w) => w.id !== conversationId)
          .concat({ ...existing, isMinimized: false });
      }

      // If at max, close the oldest (first) one
      let newWidgets = [...prev];
      if (newWidgets.length >= MAX_WIDGETS) {
        newWidgets = newWidgets.slice(1);
      }

      // Add new widget
      const newWidget: ChatWidget = {
        id: conversationId,
        conversation,
        isMinimized: false,
        hasUnread: false,
      };

      return [...newWidgets, newWidget];
    });
  }, []);

  const closeChat = useCallback((conversationId: string) => {
    setWidgets((prev) => prev.filter((w) => w.id !== conversationId));
  }, []);

  const toggleMinimize = useCallback((conversationId: string) => {
    setWidgets((prev) =>
      prev.map((w) =>
        w.id === conversationId ? { ...w, isMinimized: !w.isMinimized } : w
      )
    );
  }, []);

  const minimizeAll = useCallback(() => {
    setWidgets((prev) => prev.map((w) => ({ ...w, isMinimized: true })));
  }, []);

  const markAsRead = useCallback((conversationId: string) => {
    setWidgets((prev) =>
      prev.map((w) =>
        w.id === conversationId ? { ...w, hasUnread: false } : w
      )
    );
  }, []);

  const markAsUnread = useCallback((conversationId: string) => {
    setWidgets((prev) =>
      prev.map((w) =>
        w.id === conversationId ? { ...w, hasUnread: true } : w
      )
    );
  }, []);

  const bringToFront = useCallback((conversationId: string) => {
    setWidgets((prev) => {
      const widget = prev.find((w) => w.id === conversationId);
      if (!widget) return prev;
      return [...prev.filter((w) => w.id !== conversationId), widget];
    });
  }, []);

  return (
    <ChatWidgetContext.Provider
      value={{
        widgets,
        openChat,
        closeChat,
        toggleMinimize,
        minimizeAll,
        markAsRead,
        markAsUnread,
        bringToFront,
      }}
    >
      {children}
    </ChatWidgetContext.Provider>
  );
}

export function useChatWidget() {
  const context = useContext(ChatWidgetContext);
  if (!context) {
    throw new Error("useChatWidget must be used within a ChatWidgetProvider");
  }
  return context;
}
