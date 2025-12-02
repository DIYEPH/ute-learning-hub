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
        const hasUnread = false; // TODO: Implement unread count

        return (
          <button
            key={conversation.id}
            onClick={() => conversation.id && onSelect(conversation.id)}
            className={cn(
              "w-full p-2 md:p-3 text-left hover:bg-slate-50 dark:hover:bg-slate-800 transition-colors",
              isSelected && "bg-sky-50 dark:bg-sky-950 border-r-2 border-sky-500"
            )}
          >
            <div className="flex items-start gap-3">
              <Avatar className="h-12 w-12 flex-shrink-0">
                <AvatarImage
                  src={conversation.avatarUrl || undefined}
                  alt={conversation.conversationName || "Avatar"}
                />
                <AvatarFallback>
                  <MessageCircle className="h-6 w-6" />
                </AvatarFallback>
              </Avatar>

              <div className="flex-1 min-w-0">
                <div className="flex items-center justify-between mb-1">
                  <h3
                    className={cn(
                      "text-sm font-semibold truncate",
                      isSelected
                        ? "text-sky-700 dark:text-sky-300"
                        : "text-foreground"
                    )}
                  >
                    {conversation.conversationName || "Cuộc trò chuyện"}
                  </h3>
                  {hasUnread && (
                    <span className="flex-shrink-0 h-2 w-2 rounded-full bg-sky-500" />
                  )}
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
                  {conversation.messageCount !== undefined && (
                    <span>{conversation.messageCount} tin nhắn</span>
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

