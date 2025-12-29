"use client";

import { MessageCircle, ChevronDown, MoreVertical } from "lucide-react";
import { useState } from "react";
import { cn } from "@/lib/utils";
import type { MessageDto2 as MessageDto } from "@/src/api/database/types.gen";
import { Button } from "@/src/components/ui/button";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/src/components/ui/dropdown-menu";

interface PinnedMessagesSectionProps {
  pinnedMessages: MessageDto[];
  conversationId: string;
  onMessageClick?: (messageId: string) => void;
  onUnpin?: (message: MessageDto) => void;
}

export function PinnedMessagesSection({
  pinnedMessages,
  conversationId,
  onMessageClick,
  onUnpin,
}: PinnedMessagesSectionProps) {
  // Bỏ qua tin nhắn hệ thống trong danh sách ghim (phòng trường hợp backend thay đổi)
  const userPinnedMessages = pinnedMessages.filter(
    (m) => m.type === null || m.type === undefined
  );

  const [isExpanded, setIsExpanded] = useState(false);
  const visibleCount = 1;
  const hasMore = userPinnedMessages.length > visibleCount;
  const visibleMessages = isExpanded
    ? userPinnedMessages
    : userPinnedMessages.slice(0, visibleCount);

  if (userPinnedMessages.length === 0) return null;

  const formatTime = (dateString?: string) => {
    if (!dateString) return "";
    const date = new Date(dateString);
    return date.toLocaleTimeString("vi-VN", {
      hour: "2-digit",
      minute: "2-digit",
    });
  };

  return (
    <div className="border-b border-border bg-card">
      <div className="px-4">

        <div className="space-y-2">
          {visibleMessages.map((message) => (
            <div
              key={message.id}
              className="flex items-start gap-3 p-2.5  hover:bg-muted/50 transition-colors group relative"
            >
              <div className="flex-1 min-w-0">
                <div className="flex items-baseline gap-2 mb-1">
                  <span className="text-sm font-semibold text-foreground">
                    {message.senderName || "Người dùng"}
                  </span>
                  <span className="text-xs text-muted-foreground">
                    {formatTime(message.createdAt)}
                  </span>
                </div>
                <p
                  className="text-sm text-foreground line-clamp-2 cursor-pointer hover:underline"
                  onClick={() => onMessageClick?.(message.id || "")}
                >
                  {message.content}
                </p>
              </div>

              <div className="flex items-center gap-2 flex-shrink-0">
                {hasMore && visibleMessages.length === 1 && (
                  <button
                    onClick={() => setIsExpanded(true)}
                    className="text-xs text-primary hover:text-primary/80 flex items-center gap-1 font-medium"
                  >
                    +{userPinnedMessages.length - 1} ghim
                    <ChevronDown className="h-3 w-3" />
                  </button>
                )}
                {onUnpin && (
                  <DropdownMenu>
                    <DropdownMenuTrigger asChild>
                      <Button
                        variant="ghost"
                        size="sm"
                        className="h-7 w-7 p-0 opacity-0 group-hover:opacity-100 transition-opacity"
                        onClick={(e) => e.stopPropagation()}
                      >
                        <MoreVertical className="h-4 w-4" />
                      </Button>
                    </DropdownMenuTrigger>
                    <DropdownMenuContent align="end" className="min-w-[120px] p-1">
                      <DropdownMenuItem
                        onClick={(e) => {
                          e.stopPropagation();
                          onUnpin(message);
                        }}
                        className="text-xs py-1.5 px-2"
                      >
                        Bỏ ghim
                      </DropdownMenuItem>
                    </DropdownMenuContent>
                  </DropdownMenu>
                )}
              </div>
            </div>
          ))}

          {isExpanded && hasMore && (
            <button
              onClick={() => setIsExpanded(false)}
              className="text-xs text-primary hover:text-primary/80 flex items-center gap-1 mt-2 font-medium"
            >
              Thu gọn
              <ChevronDown className="h-3 w-3 rotate-180" />
            </button>
          )}
        </div>
      </div>
    </div>
  );
}



