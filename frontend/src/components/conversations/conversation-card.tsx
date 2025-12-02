"use client";

import { useState } from "react";
import { MessageCircle, Users, Lock, Globe, Loader2 } from "lucide-react";
import { useRouter } from "next/navigation";
import { cn } from "@/lib/utils";
import type { ConversationDto } from "@/src/api/database/types.gen";
import { Avatar, AvatarFallback, AvatarImage } from "@/src/components/ui/avatar";
import { Button } from "@/src/components/ui/button";
import { postApiConversationJoinRequest, postApiConversationByIdJoin } from "@/src/api/database/sdk.gen";

interface ConversationCardProps {
  conversation: ConversationDto;
  onJoinSuccess?: () => void;
}

export function ConversationCard({
  conversation,
  onJoinSuccess,
}: ConversationCardProps) {
  const router = useRouter();
  const [joining, setJoining] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // ConversitionType enum: 0 = Private, 1 = Group (Public), 2 = AI
  const isPrivate = conversation.conversationType === 0; // 0 = Private
  const isMember = conversation.isCurrentUserMember ?? false;
  const hasPendingRequest = conversation.hasPendingJoinRequest ?? false;

  const handleJoin = async (e: React.MouseEvent) => {
    e.stopPropagation(); // Prevent card click

    if (!conversation.id) return;

    setJoining(true);
    setError(null);

    try {
      if (isPrivate) {
        // Private: use join request
        await postApiConversationJoinRequest({
          body: {
            conversationId: conversation.id,
            content: "",
          },
        });
        onJoinSuccess?.();
      } else {
        // Public: join directly
        await postApiConversationByIdJoin({
          path: {
            id: conversation.id,
          },
        });
        onJoinSuccess?.();
        router.push(`/chat?id=${conversation.id}`);
      }
    } catch (err: any) {
      const message =
        err?.response?.data?.message ||
        err?.message ||
        (isPrivate ? "Không thể gửi yêu cầu tham gia" : "Không thể tham gia nhóm");
      setError(message);
    } finally {
      setJoining(false);
    }
  };

  const handleCardClick = () => {
    // Only allow clicking if user is already a member
    if (conversation.id && isMember) {
      router.push(`/chat?id=${conversation.id}`);
    }
  };

  return (
    <div
      className={cn(
        "rounded-lg border border-slate-200 bg-white p-4 shadow-sm transition-all hover:shadow-md dark:border-slate-700 dark:bg-slate-900",
        isMember && "cursor-pointer"
      )}
      onClick={handleCardClick}
    >
      <div className="flex items-start gap-3">
        <Avatar className="h-12 w-12 flex-shrink-0">
          <AvatarImage
            src={conversation.creatorAvatarUrl || undefined}
            alt={conversation.creatorName || "Avatar"}
          />
          <AvatarFallback>
            <MessageCircle className="h-6 w-6" />
          </AvatarFallback>
        </Avatar>

        <div className="flex-1 min-w-0">
          <div className="flex items-start justify-between gap-2 mb-2">
            <div className="flex-1 min-w-0">
              <h3 className="text-sm font-semibold text-foreground truncate mb-1">
                {conversation.conversationName || "Cuộc trò chuyện"}
              </h3>
              <div className="flex items-center gap-2 flex-wrap">
                {isPrivate ? (
                  <span className="inline-flex items-center gap-1 rounded-full bg-amber-100 px-2 py-0.5 text-[10px] font-semibold text-amber-700 dark:bg-amber-900/30 dark:text-amber-400">
                    <Lock className="h-3 w-3" />
                    Riêng tư
                  </span>
                ) : (
                  <span className="inline-flex items-center gap-1 rounded-full bg-green-100 px-2 py-0.5 text-[10px] font-semibold text-green-700 dark:bg-green-900/30 dark:text-green-400">
                    <Globe className="h-3 w-3" />
                    Công khai
                  </span>
                )}
                {conversation.subject && (
                  <span className="text-xs text-slate-500 dark:text-slate-400">
                    {conversation.subject.subjectName}
                  </span>
                )}
              </div>
            </div>
          </div>

          {conversation.tags && conversation.tags.length > 0 && (
            <div className="flex flex-wrap gap-1 mb-2">
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

          <div className="flex items-center justify-between">
            <div className="flex items-center gap-3 text-xs text-slate-500 dark:text-slate-400">
              {conversation.memberCount !== undefined && (
                <div className="flex items-center gap-1">
                  <Users className="h-3 w-3" />
                  <span>{conversation.memberCount} thành viên</span>
                </div>
              )}
              {conversation.messageCount !== undefined && conversation.messageCount > 0 && (
                <span>{conversation.messageCount} tin nhắn</span>
              )}
            </div>

            {!isMember && (
              <div className="flex-shrink-0">
                {isPrivate && hasPendingRequest ? (
                  <Button
                    size="sm"
                    variant="outline"
                    disabled
                    className="text-xs"
                    onClick={(e) => e.stopPropagation()}
                  >
                    Đã gửi yêu cầu
                  </Button>
                ) : (
                  <Button
                    size="sm"
                    variant="default"
                    onClick={handleJoin}
                    disabled={joining}
                    className="text-xs"
                  >
                    {joining ? (
                      <>
                        <Loader2 className="h-3 w-3 mr-1 animate-spin" />
                        Đang tham gia...
                      </>
                    ) : (
                      "Tham gia"
                    )}
                  </Button>
                )}
              </div>
            )}

            {isMember && (
              <Button
                size="sm"
                variant="ghost"
                className="text-xs"
                onClick={(e) => {
                  e.stopPropagation();
                  if (conversation.id) {
                    router.push(`/chat?id=${conversation.id}`);
                  }
                }}
              >
                Mở
              </Button>
            )}
          </div>

          {error && (
            <div className="mt-2 text-xs text-red-600 dark:text-red-400">
              {error}
            </div>
          )}
        </div>
      </div>
    </div>
  );
}

