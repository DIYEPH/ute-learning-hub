"use client";

import { ChatBubble } from "./chat-bubble";
import { ChatWindow } from "./chat-window";
import { useChatWidget } from "./chat-widget-context";
import { cn } from "@/lib/utils";

export function ChatWidgetContainer() {
  const { widgets, closeChat, toggleMinimize, markAsRead } = useChatWidget();

  if (widgets.length === 0) return null;

  // Separate minimized and expanded widgets
  const minimizedWidgets = widgets.filter((w) => w.isMinimized);
  const expandedWidgets = widgets.filter((w) => !w.isMinimized);

  return (
    <>
      {/* Chat Windows - Fixed at bottom right */}
      <div className="fixed bottom-0 right-20 z-50 flex items-end gap-2 pointer-events-none">
        {expandedWidgets.map((widget, index) => (
          <div
            key={widget.id}
            className="pointer-events-auto"
            style={{
              width: "328px",
              height: "455px",
              marginRight: index < expandedWidgets.length - 1 ? "8px" : "0",
            }}
          >
            <ChatWindow
              conversationId={widget.id}
              onClose={() => closeChat(widget.id)}
              onMinimize={() => toggleMinimize(widget.id)}
              onMarkAsRead={() => markAsRead(widget.id)}
            />
          </div>
        ))}
      </div>

      {/* Minimized Bubbles - Fixed at bottom right corner */}
      {minimizedWidgets.length > 0 && (
        <div className="fixed bottom-4 right-4 z-50 flex flex-col-reverse gap-2">
          {minimizedWidgets.map((widget) => (
            <ChatBubble
              key={widget.id}
              conversation={widget.conversation}
              hasUnread={widget.hasUnread}
              onClick={() => toggleMinimize(widget.id)}
              onClose={() => closeChat(widget.id)}
            />
          ))}
        </div>
      )}
    </>
  );
}
