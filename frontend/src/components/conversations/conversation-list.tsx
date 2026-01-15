"use client";

import { MessageCircle, Users } from "lucide-react";
import { cn } from "@/lib/utils";
import type { ConversationDto } from "@/src/api/database/types.gen";
import { Avatar, AvatarFallback, AvatarImage } from "@/src/components/ui/avatar";
import { Tooltip, TooltipContent, TooltipTrigger } from "@/src/components/ui/tooltip";

interface ConversationListProps {
  conversations: ConversationDto[];
  selectedId?: string;
  onSelect: (conversationId: string) => void;
  collapsed?: boolean;
}

export function ConversationList({
  conversations,
  selectedId,
  onSelect,
  collapsed = false,
}: ConversationListProps) {
  return (
    <div className={cn(collapsed ? "space-y-1 p-1" : "divide-y divide-border")}>
      {conversations.map((conversation) => {
        const isSelected = conversation.id === selectedId;
        const unreadCount = conversation.unreadCount ?? 0;
        const hasUnread = unreadCount > 0;

        const content = (
          <button
            key={conversation.id}
            onClick={() => conversation.id && onSelect(conversation.id)}
            className={cn(
              "w-full text-left hover:bg-secondary/50 transition-colors",
              collapsed ? "p-1.5 rounded-lg flex justify-center" : "p-2 md:p-3",
              isSelected && (collapsed ? "bg-secondary rounded-lg" : "bg-secondary border-l-2 border-primary"),
              hasUnread && !isSelected && "bg-secondary/50"
            )}
          >
            <div className={cn("flex items-start", collapsed ? "justify-center" : "gap-3")}>
              <div className="relative">
                <Avatar className={cn("shrink-0", collapsed ? "h-10 w-10" : "h-12 w-12")}>
                  <AvatarImage
                    src={conversation.avatarUrl || undefined}
                    alt={conversation.conversationName || "Avatar"}
                  />
                  <AvatarFallback>
                    <MessageCircle className={cn(collapsed ? "h-5 w-5" : "h-6 w-6")} />
                  </AvatarFallback>
                </Avatar>
                {hasUnread && (
                  <span className={cn(
                    "absolute flex items-center justify-center rounded-full bg-primary font-bold text-primary-foreground",
                    collapsed ? "-top-0.5 -right-0.5 h-4 w-4 text-[8px]" : "-top-1 -right-1 h-5 w-5 text-[10px]"
                  )}>
                    {unreadCount > 99 ? "99+" : unreadCount}
                  </span>
                )}
              </div>

              {!collapsed && (
                <div className="flex-1 min-w-0">
                  <div className="flex items-center justify-between mb-1">
                    <h3
                      className={cn(
                        "text-sm truncate",
                        hasUnread ? "font-bold" : "font-semibold",
                        isSelected ? "text-primary" : "text-foreground"
                      )}
                    >
                      {conversation.conversationName || "Cuộc trò chuyện"}
                    </h3>
                  </div>

                  {conversation.tags && conversation.tags.length > 0 && (
                    <div className="flex flex-wrap gap-1 mb-1">
                      {conversation.tags.slice(0, 3).map((tag) => (
                        <span
                          key={tag.id}
                          className="inline-flex items-center rounded-full border border-border bg-background px-2 py-0.5 text-[10px] font-medium text-foreground"
                        >
                          {tag.tagName}
                        </span>
                      ))}
                      {conversation.tags.length > 3 && (
                        <span className="text-[10px] font-medium text-muted-foreground">
                          +{conversation.tags.length - 3}
                        </span>
                      )}
                    </div>
                  )}

                  <div className="flex items-center gap-3 text-xs text-muted-foreground">
                    {conversation.memberCount !== undefined && (
                      <div className="flex items-center gap-1">
                        <Users className="h-3 w-3" />
                        <span>{conversation.memberCount}</span>
                      </div>
                    )}
                  </div>
                </div>
              )}
            </div>
          </button>
        );

        if (collapsed) {
          return (
            <Tooltip key={conversation.id}>
              <TooltipTrigger asChild>{content}</TooltipTrigger>
              <TooltipContent side="right" className="max-w-[200px]">
                <p className="font-semibold">{conversation.conversationName || "Cuộc trò chuyện"}</p>
                {hasUnread && <p className="text-xs text-muted-foreground">{unreadCount} tin nhắn mới</p>}
              </TooltipContent>
            </Tooltip>
          );
        }

        return content;
      })}
    </div>
  );
}



