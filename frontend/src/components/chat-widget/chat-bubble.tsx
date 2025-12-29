"use client";

import { MessageCircle } from "lucide-react";
import { cn } from "@/lib/utils";
import { Avatar, AvatarFallback, AvatarImage } from "@/src/components/ui/avatar";
import type { ConversationDto } from "@/src/api/database/types.gen";

interface ChatBubbleProps {
  conversation: ConversationDto;
  hasUnread?: boolean;
  onClick: () => void;
  onClose: () => void;
}

export function ChatBubble({
  conversation,
  hasUnread = false,
  onClick,
  onClose,
}: ChatBubbleProps) {
  const handleClose = (e: React.MouseEvent) => {
    e.stopPropagation();
    onClose();
  };

  return (
    <div className="relative group">
      {/* Main bubble */}
      <button
        onClick={onClick}
        className={cn(
          "relative flex h-12 w-12 items-center justify-center rounded-full shadow-lg transition-all",
          "bg-primary hover:bg-primary/90 hover:scale-110",
          "focus:outline-none focus:ring-2 focus:ring-primary focus:ring-offset-2"
        )}
        title={conversation.conversationName || "Chat"}
      >
        <Avatar className="h-10 w-10">
          <AvatarImage
            src={conversation.avatarUrl || undefined}
            alt={conversation.conversationName || "Avatar"}
          />
          <AvatarFallback className="bg-primary-foreground/20 text-primary-foreground">
            <MessageCircle className="h-5 w-5" />
          </AvatarFallback>
        </Avatar>

        {/* Unread indicator */}
        {hasUnread && (
          <span className="absolute -top-1 -right-1 flex h-4 w-4 items-center justify-center rounded-full bg-red-500 text-[10px] font-bold text-white">
            !
          </span>
        )}
      </button>

      {/* Close button (visible on hover) */}
      <button
        onClick={handleClose}
        className={cn(
          "absolute -top-1 -right-1 flex h-5 w-5 items-center justify-center rounded-full",
          "bg-muted-foreground/80 text-background text-xs font-bold",
          "opacity-0 group-hover:opacity-100 transition-opacity",
          "hover:bg-destructive"
        )}
      >
        ×
      </button>

      {/* Tooltip */}
      <div className="absolute bottom-full left-1/2 -translate-x-1/2 mb-2 px-2 py-1 bg-foreground text-background text-xs rounded whitespace-nowrap opacity-0 group-hover:opacity-100 transition-opacity pointer-events-none">
        {conversation.conversationName || "Chat"}
      </div>
    </div>
  );
}
