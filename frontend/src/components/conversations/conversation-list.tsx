"use client";

import { MessageCircle, Users } from "lucide-react";
import { cn } from "@/lib/utils";
import type { ConversationDto } from "@/src/api/database/types.gen";
import { Avatar, AvatarFallback, AvatarImage } from "@/src/components/ui/avatar";

interface ConversationListProps {
  conversations: ConversationDto[];
  selectedId?: string;
  onSelect: (conversationId: string) => void;
}

export function ConversationList({
  conversations,
  selectedId,
  onSelect,
}: ConversationListProps) {
  return (
    <div className="divide-y divide-slate-200 dark:divide-slate-700">
      {conversations.map((conversation) => {
        const isSelected = conversation.id === selectedId;
        const unreadCount = conversation.unreadCount ?? 0;
        const hasUnread = unreadCount > 0;

        return (
          <button
            key={conversation.id}
            onClick={() => conversation.id && onSelect(conversation.id)}
            className={cn(
              "w-full p-2 md:p-3 text-left hover:bg-slate-50 dark:hover:bg-slate-800 transition-colors",
              isSelected && "bg-sky-50 dark:bg-sky-950 border-r-2 border-sky-500",
              hasUnread && !isSelected && "bg-sky-50/50 dark:bg-sky-950/30"
            )}
          >
            <div className="flex items-start gap-3">
              <div className="relative">
                <Avatar className="h-12 w-12 flex-shrink-0">
                  <AvatarImage
                    src={conversation.avatarUrl || undefined}
                    alt={conversation.conversationName || "Avatar"}
                  />
                  <AvatarFallback>
                    <MessageCircle className="h-6 w-6" />
                  </AvatarFallback>
                </Avatar>
                {hasUnread && (
                  <span className="absolute -top-1 -right-1 flex h-5 w-5 items-center justify-center rounded-full bg-sky-500 text-[10px] font-bold text-white">
                    {unreadCount > 99 ? "99+" : unreadCount}
                  </span>
                )}
              </div>

              <div className="flex-1 min-w-0">
                <div className="flex items-center justify-between mb-1">
                  <h3
                    className={cn(
                      "text-sm truncate",
                      hasUnread ? "font-bold" : "font-semibold",
                      isSelected
                        ? "text-sky-700 dark:text-sky-300"
                        : "text-foreground"
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
                        className="inline-flex items-center rounded-full bg-slate-100 px-2 py-0.5 text-[10px] font-semibold uppercase tracking-wide text-slate-600 dark:bg-slate-800 dark:text-slate-300"
                      >
                        {tag.tagName}
                      </span>
                    ))}
                    {conversation.tags.length > 3 && (
                      <span className="text-[10px] font-medium text-slate-500 dark:text-slate-400">
                        +{conversation.tags.length - 3}
                      </span>
                    )}
                  </div>
                )}

                <div className="flex items-center gap-3 text-xs text-slate-500 dark:text-slate-400">
                  {conversation.memberCount !== undefined && (
                    <div className="flex items-center gap-1">
                      <Users className="h-3 w-3" />
                      <span>{conversation.memberCount}</span>
                    </div>
                  )}
                </div>
              </div>
            </div>
          </button>
        );
      })}
    </div>
  );
}



