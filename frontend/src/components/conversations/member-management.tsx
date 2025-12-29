"use client";

import { useState } from "react";
import { MoreVertical, Loader2, Sparkles } from "lucide-react";
import { putApiConversationByIdMembersByMemberIdRole } from "@/src/api/database/sdk.gen";
import type { ConversationMemberDto, ConversationMemberRoleType, ConversationDetailDto } from "@/src/api/database/types.gen";
import { Button } from "@/src/components/ui/button";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/src/components/ui/dropdown-menu";
import { Avatar, AvatarFallback, AvatarImage } from "@/src/components/ui/avatar";
import { useUserProfile } from "@/src/hooks/use-user-profile";
import { SuggestedUsersSidebar } from "@/src/components/conversations/suggested-users-sidebar";

interface MemberManagementProps {
  conversation: ConversationDetailDto;
  onSuccess?: () => void;
}

const ROLE_LABELS: Record<number, string> = {
  0: "Thành viên",
  1: "Nhóm phó",
  2: "Nhóm trưởng",
};

export function MemberManagement({ conversation, onSuccess }: MemberManagementProps) {
  const { profile } = useUserProfile();
  const [updatingMemberId, setUpdatingMemberId] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [showSuggestedUsers, setShowSuggestedUsers] = useState(false);

  if (!conversation.members || conversation.members.length === 0) {
    return null;
  }

  const currentUserMember = conversation.members.find(
    (m) => m.userId === profile?.id
  );

  const isOwner = currentUserMember?.roleType === 2;

  if (!isOwner) {
    return (
      <div className="space-y-2">
        <h4 className="text-sm font-semibold text-foreground">Thành viên</h4>
        <div className="space-y-2">
          {conversation.members.map((member) => (
            <div
              key={member.id}
              className="flex items-center gap-3 p-2 hover:bg-muted"
            >
              <Avatar className="h-8 w-8">
                <AvatarImage src={member.userAvatarUrl || undefined} />
                <AvatarFallback>
                  {member.userName?.[0]?.toUpperCase() || "U"}
                </AvatarFallback>
              </Avatar>
              <div className="flex-1 min-w-0">
                <p className="text-sm font-medium text-foreground truncate">
                  {member.userName || "Không tên"}
                </p>
                <p className="text-xs text-muted-foreground">
                  {ROLE_LABELS[member.roleType ?? 0]}
                </p>
              </div>
            </div>
          ))}
        </div>
      </div>
    );
  }

  const handleRoleChange = async (memberId: string, newRole: ConversationMemberRoleType) => {
    if (!conversation.id || !memberId) return;

    setUpdatingMemberId(memberId);
    setError(null);

    try {
      await putApiConversationByIdMembersByMemberIdRole({
        path: {
          id: conversation.id,
          memberId: memberId,
        },
        body: {
          roleType: newRole,
        },
      });

      onSuccess?.();
    } catch (err: any) {
      const errorMessage =
        err?.response?.data?.message ||
        err?.message ||
        "Không thể cập nhật vai trò";
      setError(errorMessage);
    } finally {
      setUpdatingMemberId(null);
    }
  };

  return (
    <div className="space-y-2">
      <h4 className="text-sm font-semibold text-foreground">Quản lý thành viên</h4>
      {error && (
        <div className="p-2 text-xs text-red-600 dark:text-red-400 bg-red-50 dark:bg-red-950 rounded">
          {error}
        </div>
      )}
      <div className="space-y-2">
        {conversation.members.map((member) => {
          const isCurrentUser = member.userId === profile?.id;
          const canChangeRole = !isCurrentUser && member.roleType !== 2;

          return (
            <div
              key={member.id}
              className="flex items-center gap-3 p-2 hover:bg-muted"
            >
              <Avatar className="h-8 w-8">
                <AvatarImage src={member.userAvatarUrl || undefined} />
                <AvatarFallback>
                  {member.userName?.[0]?.toUpperCase() || "U"}
                </AvatarFallback>
              </Avatar>
              <div className="flex-1 min-w-0">
                <p className="text-sm font-medium text-foreground truncate">
                  {member.userName || "Không tên"}
                  {isCurrentUser && " (Bạn)"}
                </p>
                <p className="text-xs text-muted-foreground">
                  {ROLE_LABELS[member.roleType ?? 0]}
                </p>
              </div>
              {canChangeRole && (
                <DropdownMenu>
                  <DropdownMenuTrigger asChild>
                    <Button
                      variant="ghost"
                      size="sm"
                      className="h-8 w-8 p-0"
                      disabled={updatingMemberId === member.id}
                    >
                      {updatingMemberId === member.id ? (
                        <Loader2 className="h-4 w-4 animate-spin" />
                      ) : (
                        <MoreVertical className="h-4 w-4" />
                      )}
                    </Button>
                  </DropdownMenuTrigger>
                  <DropdownMenuContent align="end">
                    <DropdownMenuItem
                      onClick={() => member.id && handleRoleChange(member.id, 0)}
                      disabled={member.roleType === 0}
                    >
                      Thành viên
                    </DropdownMenuItem>
                    <DropdownMenuItem
                      onClick={() => member.id && handleRoleChange(member.id, 1)}
                      disabled={member.roleType === 1}
                    >
                      Nhóm phó
                    </DropdownMenuItem>
                    <DropdownMenuItem
                      onClick={() => member.id && handleRoleChange(member.id, 2)}
                      disabled={member.roleType === 2}
                    >
                      Nhóm trưởng
                    </DropdownMenuItem>
                  </DropdownMenuContent>
                </DropdownMenu>
              )}
            </div>
          );
        })}
      </div>

      {/* AI Suggestion Button */}
      <Button
        variant="outline"
        size="sm"
        onClick={() => setShowSuggestedUsers(true)}
        className="w-full mt-3"
      >
        <Sparkles className="h-4 w-4 mr-2 text-amber-500" />
        Gợi ý thành viên (AI)
      </Button>

      {/* Suggested Users Sidebar */}
      <SuggestedUsersSidebar
        open={showSuggestedUsers}
        onClose={() => setShowSuggestedUsers(false)}
        conversationId={conversation.id || ""}
        conversationName={conversation.conversationName || "Cuộc trò chuyện"}
        onInviteSent={onSuccess}
      />
    </div>
  );
}



