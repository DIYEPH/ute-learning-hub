"use client";

import { useState } from "react";
import { MoreVertical, Edit, Trash2, Paperclip, Pin, PinOff, Reply } from "lucide-react";
import { cn } from "@/lib/utils";
import type { MessageDto } from "@/src/api/database/types.gen";
import { Avatar, AvatarFallback, AvatarImage } from "@/src/components/ui/avatar";
import { Button } from "@/src/components/ui/button";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/src/components/ui/dropdown-menu";
import {
  deleteApiConversationsByConversationIdMessagesById,
  putApiConversationsByConversationIdMessagesById,
} from "@/src/api/database/sdk.gen";
import { Input } from "@/src/components/ui/input";
import { usePinMessage } from "@/src/hooks/use-pin-message";
import { getFileUrlById } from "@/src/lib/file-url";

interface MessageItemProps {
  message: MessageDto;
  conversationId: string;
  currentUserId?: string;
  showDate?: boolean;
  allMessages?: MessageDto[];
  onUpdate?: (message: MessageDto) => void;
  onDelete?: (messageId: string) => void;
  onReply?: (message: MessageDto) => void;
  onScrollToMessage?: (messageId: string) => void;
}

export function MessageItem({
  message,
  conversationId,
  currentUserId,
  showDate: shouldShowDate = false,
  allMessages = [],
  onUpdate,
  onDelete,
  onReply,
  onScrollToMessage,
}: MessageItemProps) {
  const [isEditing, setIsEditing] = useState(false);
  const [editContent, setEditContent] = useState(message.content || "");
  const [isDeleting, setIsDeleting] = useState(false);
  const [isUpdating, setIsUpdating] = useState(false);
  const [showClickedDate, setShowClickedDate] = useState(false);

  const { togglePin, isPinning } = usePinMessage(conversationId, onUpdate);

  const isOwnMessage = message.createdById === currentUserId;
  const messageDate = message.createdAt
    ? new Date(message.createdAt)
    : null;
  const formattedDate = messageDate && shouldShowDate
    ? messageDate.toLocaleString("vi-VN", {
      day: "2-digit",
      month: "2-digit",
      year: "numeric",
      hour: "2-digit",
      minute: "2-digit",
    })
    : null;

  const clickedDate = messageDate
    ? messageDate.toLocaleString("vi-VN", {
      day: "2-digit",
      month: "2-digit",
      year: "numeric",
      hour: "2-digit",
      minute: "2-digit",
    })
    : null;

  const isSystemMessage =
    message.type !== null && message.type !== undefined;

  const parentMessage = message.parentId
    ? allMessages.find((m) => m.id === message.parentId)
    : null;

  const handleMessageClick = () => {
    if (!isEditing) {
      setShowClickedDate((prev) => !prev);
    }
  };

  // System message: render ở giữa, style đơn giản, không cho sửa/xóa
  if (isSystemMessage) {
    const systemText = getSystemMessageText(message, currentUserId);

    return (
      <div className="my-2 flex flex-col items-center">
        {formattedDate && (
          <span className="mb-1 text-[11px] text-slate-400 dark:text-slate-500">
            {formattedDate}
          </span>
        )}
        <div className="rounded-full bg-slate-100 px-3 py-1 text-[11px] text-slate-600 dark:bg-slate-800 dark:text-slate-300">
          {systemText}
        </div>
      </div>
    );
  }

  const handleEdit = () => {
    setIsEditing(true);
    setEditContent(message.content || "");
  };

  const handleCancelEdit = () => {
    setIsEditing(false);
    setEditContent(message.content || "");
  };

  const handleSaveEdit = async () => {
    if (!message.id || !conversationId) return;

    setIsUpdating(true);
    try {
      const response = await putApiConversationsByConversationIdMessagesById({
        path: {
          conversationId,
          id: message.id,
        },
        body: {
          id: message.id,
          conversationId,
          content: editContent.trim(),
        },
      });

      const updatedMessage = (response.data ?? response) as MessageDto | undefined;
      if (updatedMessage) {
        onUpdate?.(updatedMessage);
        setIsEditing(false);
      }
    } catch (err: any) {
      console.error("Error updating message:", err);
      alert(err?.response?.data?.message || "Không thể cập nhật tin nhắn");
    } finally {
      setIsUpdating(false);
    }
  };

  const handleDelete = async () => {
    if (!message.id || !conversationId) return;

    if (!confirm("Bạn có chắc chắn muốn xóa tin nhắn này?")) return;

    setIsDeleting(true);
    try {
      await deleteApiConversationsByConversationIdMessagesById({
        path: {
          conversationId,
          id: message.id,
        },
      });

      onDelete?.(message.id);
    } catch (err: any) {
      console.error("Error deleting message:", err);
      alert(err?.response?.data?.message || "Không thể xóa tin nhắn");
    } finally {
      setIsDeleting(false);
    }
  };

  return (
    <div
      className={cn(
        "flex items-end gap-2 group hover:bg-slate-50 dark:hover:bg-slate-800/50 rounded-lg p-1 -m-1 transition-colors",
        isOwnMessage && "flex-row-reverse"
      )}
    >
      {/* Avatar - chỉ hiển thị bên trái cho tin nhắn của người khác */}
      {!isOwnMessage && (
        <Avatar className="h-8 w-8 flex-shrink-0">
          <AvatarImage
            src={message.senderAvatarUrl || undefined}
            alt={message.senderName || "User"}
          />
          <AvatarFallback>
            {message.senderName?.[0]?.toUpperCase() || "U"}
          </AvatarFallback>
        </Avatar>
      )}

      {/* Message Content */}
      <div
        className={cn(
          "flex-1 min-w-0 flex flex-col",
          isOwnMessage ? "items-end" : "items-start"
        )}
      >
        {/* Header with date - outside bubble */}
        {formattedDate && (
          <div
            className={cn(
              "flex items-baseline gap-2 mb-1 w-full",
              isOwnMessage ? "justify-end" : "justify-start"
            )}
          >
            <span className="text-xs text-slate-500 dark:text-slate-400">
              {formattedDate}
            </span>
          </div>
        )}

        {/* Message Content */}
        {isEditing ? (
          <div className="w-full space-y-2">
            <Input
              value={editContent}
              onChange={(e) => setEditContent(e.target.value)}
              onKeyDown={(e) => {
                if (e.key === "Enter" && !e.shiftKey) {
                  e.preventDefault();
                  void handleSaveEdit();
                }
                if (e.key === "Escape") {
                  handleCancelEdit();
                }
              }}
              className="text-sm"
              autoFocus
            />
            <div className="flex gap-2 justify-end">
              <Button
                variant="ghost"
                size="sm"
                onClick={handleCancelEdit}
                disabled={isUpdating}
              >
                Hủy
              </Button>
              <Button
                size="sm"
                onClick={handleSaveEdit}
                disabled={isUpdating || !editContent.trim()}
              >
                {isUpdating ? "Đang lưu..." : "Lưu"}
              </Button>
            </div>
          </div>
        ) : (
          <>
            <div
              className={cn(
                "rounded-2xl px-4 py-2.5 max-w-[80%] md:max-w-[70%] shadow-sm relative group/message cursor-pointer",
                isOwnMessage
                  ? "bg-sky-500 text-white rounded-br-md"
                  : "bg-slate-200 dark:bg-slate-700 text-foreground rounded-bl-md"
              )}
              onClick={handleMessageClick}
            >
              {/* Pin indicator - bottom right to avoid conflict with menu */}
              {message.isPined && (
                <div className={cn(
                  "absolute bottom-2 right-2 flex items-center gap-1 px-1.5 py-0.5 rounded-full text-[10px] font-medium z-10",
                  isOwnMessage
                    ? "bg-white/20 text-white"
                    : "bg-sky-500/20 text-sky-600 dark:text-sky-400"
                )}>
                  <Pin className="h-3 w-3" />
                </div>
              )}

              {/* Menu button - absolute positioned */}
              <DropdownMenu>
                <DropdownMenuTrigger asChild>
                  <Button
                    variant="ghost"
                    size="sm"
                    className={cn(
                      "absolute h-6 w-6 p-0 opacity-0 group-hover/message:opacity-100 transition-opacity z-10",
                      isOwnMessage
                        ? "top-2 right-2 hover:bg-white/20 text-white"
                        : "top-2 left-2 hover:bg-slate-100 dark:hover:bg-slate-700"
                    )}
                    onClick={(e) => e.stopPropagation()}
                  >
                    <MoreVertical className="h-4 w-4" />
                  </Button>
                </DropdownMenuTrigger>
                <DropdownMenuContent
                  align={isOwnMessage ? "end" : "start"}
                  className="min-w-[120px] p-1"
                >
                  {onReply && (
                    <DropdownMenuItem
                      onClick={(e) => {
                        e.stopPropagation();
                        onReply(message);
                      }}
                      disabled={isDeleting || isUpdating || isPinning}
                      className="text-xs py-1.5 px-2"
                    >
                      <Reply className="h-3.5 w-3.5 mr-2" />
                      Trả lời
                    </DropdownMenuItem>
                  )}
                  {isOwnMessage && (
                    <>
                      <DropdownMenuItem
                        onClick={(e) => {
                          e.stopPropagation();
                          void togglePin(message);
                        }}
                        disabled={isDeleting || isUpdating || isPinning}
                        className="text-xs py-1.5 px-2"
                      >
                        {message.isPined ? (
                          <>
                            <PinOff className="h-3.5 w-3.5 mr-2" />
                            Bỏ ghim
                          </>
                        ) : (
                          <>
                            <Pin className="h-3.5 w-3.5 mr-2" />
                            Ghim
                          </>
                        )}
                      </DropdownMenuItem>
                      <DropdownMenuItem
                        onClick={(e) => {
                          e.stopPropagation();
                          handleEdit();
                        }}
                        disabled={isDeleting || isUpdating || isPinning}
                        className="text-xs py-1.5 px-2"
                      >
                        <Edit className="h-3.5 w-3.5 mr-2" />
                        Chỉnh sửa
                      </DropdownMenuItem>
                      <DropdownMenuItem
                        onClick={(e) => {
                          e.stopPropagation();
                          handleDelete();
                        }}
                        disabled={isDeleting || isUpdating || isPinning}
                        className="text-xs py-1.5 px-2 text-red-600 dark:text-red-400"
                      >
                        <Trash2 className="h-3.5 w-3.5 mr-2" />
                        Xóa
                      </DropdownMenuItem>
                    </>
                  )}
                </DropdownMenuContent>
              </DropdownMenu>

              {/* Reply quote - hiển thị tin nhắn được trả lời */}
              {parentMessage && (
                <button
                  type="button"
                  onClick={(e) => {
                    e.stopPropagation();
                    if (onScrollToMessage && parentMessage.id) {
                      onScrollToMessage(parentMessage.id);
                    }
                  }}
                  className={cn(
                    "mb-2 flex items-start gap-2 rounded-lg border-l-4 px-2 py-1.5 text-left transition-colors hover:opacity-80",
                    isOwnMessage
                      ? "border-sky-300 bg-white/10"
                      : "border-sky-500 bg-slate-100 dark:bg-slate-600"
                  )}
                >
                  <div className="flex-1 min-w-0">
                    <div className={cn(
                      "text-xs font-semibold mb-0.5 truncate",
                      isOwnMessage ? "text-white/90" : "text-slate-700 dark:text-slate-200"
                    )}>
                      {parentMessage.senderName || "Người dùng"}
                    </div>
                    <div className={cn(
                      "text-xs truncate",
                      isOwnMessage ? "text-white/70" : "text-slate-600 dark:text-slate-300"
                    )}>
                      {parentMessage.content}
                    </div>
                  </div>
                </button>
              )}

              {/* Header with name - inside bubble */}
              {(!isOwnMessage || message.isEdit) && (
                <div
                  className={cn(
                    "flex items-baseline gap-2 mb-1.5",
                    isOwnMessage ? "justify-end" : "justify-start"
                  )}
                >
                  {!isOwnMessage && (
                    <span className={cn(
                      "text-xs font-semibold",
                      isOwnMessage ? "text-white/90" : "text-foreground/80"
                    )}>
                      {message.senderName || "Người dùng"}
                    </span>
                  )}
                  {message.isEdit && (
                    <span className={cn(
                      "text-[10px] italic",
                      isOwnMessage ? "text-white/70" : "text-slate-400"
                    )}>
                      (đã chỉnh sửa)
                    </span>
                  )}
                </div>
              )}
              {/* Nội dung tin nhắn */}
              <div className="text-sm leading-relaxed whitespace-pre-wrap break-words">
                {message.content}
              </div>

              {/* Files */}
              {message.files && message.files.length > 0 && (
                <div className="mt-2 space-y-2">
                  {message.files.map((file) => {
                    const isImage = file.mimeType?.startsWith("image/");
                    const fileUrl = getFileUrlById(file.fileId);
                    return isImage ? (
                      <a
                        key={file.fileId}
                        href={fileUrl}
                        target="_blank"
                        rel="noopener noreferrer"
                        className="block"
                        onClick={(e) => e.stopPropagation()}
                      >
                        {/* eslint-disable-next-line @next/next/no-img-element */}
                        <img
                          src={fileUrl}
                          alt="Image"
                          className={cn(
                            "max-w-full max-h-64 rounded-xl border cursor-pointer hover:opacity-90 transition-opacity",
                            isOwnMessage
                              ? "border-sky-400/30"
                              : "border-slate-300 dark:border-slate-600"
                          )}
                        />
                      </a>
                    ) : (
                      <a
                        key={file.fileId}
                        href={fileUrl}
                        target="_blank"
                        rel="noopener noreferrer"
                        className={cn(
                          "flex items-center gap-2 text-xs hover:underline p-2 rounded-xl border transition-colors",
                          isOwnMessage
                            ? "bg-sky-400/20 border-sky-400/30 text-white"
                            : "border-slate-300 dark:border-slate-600 hover:bg-slate-100 dark:hover:bg-slate-600"
                        )}
                        onClick={(e) => e.stopPropagation()}
                      >
                        <Paperclip className="h-3.5 w-3.5" />
                        <span className="truncate">Tệp đính kèm</span>
                      </a>
                    );
                  })}
                </div>
              )}
            </div>

            {/* Clicked date - below bubble */}
            {showClickedDate && clickedDate && (
              <div
                className={cn(
                  "flex items-center justify-center mt-1 w-full",
                  isOwnMessage ? "justify-end" : "justify-start"
                )}
              >
                <span className="text-xs text-slate-500 dark:text-slate-400 px-2">
                  {clickedDate}
                </span>
              </div>
            )}
          </>
        )}
      </div>
    </div>
  );
}

// Mapping type number -> text hiển thị
function getSystemMessageText(message: MessageDto, currentUserId?: string) {
  const actorName = message.senderName || "Hệ thống";
  const isSelf = currentUserId && message.createdById === currentUserId;

  // Backend enum MessageType:
  // 0: ConversationCreated
  // 1: MemberJoined
  // 2: MemberLeft
  // 3: MemberRoleUpdated
  // 4: MessagePinned
  // 5: MessageUnpinned
  // 6: JoinRequestApproved
  switch (message.type) {
    case 0:
      return isSelf
        ? "Bạn đã tạo cuộc trò chuyện"
        : `${actorName} đã tạo cuộc trò chuyện`;
    case 1:
      return isSelf
        ? "Bạn đã tham gia cuộc trò chuyện"
        : `${actorName} đã tham gia cuộc trò chuyện`;
    case 2:
      return isSelf
        ? "Bạn đã rời cuộc trò chuyện"
        : `${actorName} đã rời cuộc trò chuyện`;
    case 3:
      return isSelf
        ? "Bạn đã cập nhật quyền của một thành viên"
        : `${actorName} đã cập nhật quyền của một thành viên`;
    case 4:
      return isSelf
        ? "Bạn đã ghim một tin nhắn"
        : `${actorName} đã ghim một tin nhắn`;
    case 5:
      return isSelf
        ? "Bạn đã bỏ ghim một tin nhắn"
        : `${actorName} đã bỏ ghim một tin nhắn`;
    case 6:
      return isSelf
        ? "Bạn đã chấp nhận một thành viên tham gia cuộc trò chuyện"
        : `${actorName} đã chấp nhận một thành viên tham gia cuộc trò chuyện`;
    default:
      return actorName;
  }
}

