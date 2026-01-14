"use client";

import { useState } from "react";
import { MoreVertical, Edit, Trash2, Paperclip, Pin, PinOff, Reply } from "lucide-react";
import { cn } from "@/lib/utils";
import type { MessageDto2 as MessageDto } from "@/src/api/database/types.gen";
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
} from "@/src/api";
import { Input } from "@/src/components/ui/input";
import { usePinMessage } from "@/src/hooks/use-pin-message";
import { getFileUrlById } from "@/src/lib/file-url";
import { parseDocumentEmbeds } from "./document-embed";

interface MessageItemProps {
  message: MessageDto;
  conversationId: string;
  currentUserId?: string;
  showDate?: boolean;
  showSender?: boolean;
  allMessages?: MessageDto[];
  onUpdate?: (message: MessageDto) => void;
  onDelete?: (messageId: string) => void;
  onReply?: (message: MessageDto) => void;
  onScrollToMessage?: (messageId: string) => void;
  compact?: boolean;
}

export function MessageItem({
  message,
  conversationId,
  currentUserId,
  showDate: shouldShowDate = false,
  showSender = true,
  allMessages = [],
  onUpdate,
  onDelete,
  onReply,
  onScrollToMessage,
  compact = false,
}: MessageItemProps) {
  const [isEditing, setIsEditing] = useState(false);
  const [editContent, setEditContent] = useState(message.content || "");
  const [isDeleting, setIsDeleting] = useState(false);
  const [isUpdating, setIsUpdating] = useState(false);

  const { togglePin, isPinning } = usePinMessage(conversationId, onUpdate);

  const isOwnMessage = message.createdById === currentUserId;
  const messageDate = message.createdAt ? new Date(message.createdAt) : null;

  const formattedTime = messageDate
    ? messageDate.toLocaleString("vi-VN", { hour: "2-digit", minute: "2-digit" })
    : null;

  const formattedDate = messageDate && shouldShowDate
    ? messageDate.toLocaleString("vi-VN", { day: "2-digit", month: "2-digit", year: "numeric" })
    : null;

  const isSystemMessage = message.type !== null && message.type !== undefined;

  const parentMessage = message.parentId
    ? allMessages.find((m) => m.id === message.parentId)
    : null;

  // System message
  if (isSystemMessage) {
    const systemText = getSystemMessageText(message, currentUserId);
    return (
      <div className="my-2 flex flex-col items-center">
        {formattedDate && (
          <span className="mb-1 text-[11px] text-muted-foreground">{formattedDate}</span>
        )}
        <div className="rounded-full bg-muted px-3 py-1 text-[11px] text-muted-foreground">
          {systemText}
        </div>
      </div>
    );
  }

  const handleEdit = () => { setIsEditing(true); setEditContent(message.content || ""); };
  const handleCancelEdit = () => { setIsEditing(false); setEditContent(message.content || ""); };

  const handleSaveEdit = async () => {
    if (!message.id || !conversationId) return;
    setIsUpdating(true);
    try {
      const response = await putApiConversationsByConversationIdMessagesById({
        path: { conversationId, id: message.id },
        body: { content: editContent.trim() },
      });
      const updatedMessage = (response.data ?? response) as MessageDto | undefined;
      if (updatedMessage) { onUpdate?.(updatedMessage); setIsEditing(false); }
    } catch (err: any) {
      console.error("Error updating message:", err);
      alert(err?.response?.data?.message || "Không thể cập nhật tin nhắn");
    } finally { setIsUpdating(false); }
  };

  const handleDelete = async () => {
    if (!message.id || !conversationId) return;
    if (!confirm("Bạn có chắc chắn muốn xóa tin nhắn này?")) return;
    setIsDeleting(true);
    try {
      await deleteApiConversationsByConversationIdMessagesById({
        path: { conversationId, id: message.id },
      });
      onDelete?.(message.id);
    } catch (err: any) {
      console.error("Error deleting message:", err);
      alert(err?.response?.data?.message || "Không thể xóa tin nhắn");
    } finally { setIsDeleting(false); }
  };

  // ===================== OWN MESSAGE (căn phải) =====================
  if (isOwnMessage) {
    return (
      <div className={cn("group hover:bg-muted/30 transition-colors", compact ? "py-1" : "py-1.5")}>
        {formattedDate && (
          <div className="flex items-center justify-center mb-2">
            <span className="text-[11px] text-muted-foreground bg-muted px-2 py-0.5 rounded-full">{formattedDate}</span>
          </div>
        )}

        <div className="flex justify-end px-2">
          <div className="flex flex-col items-end">
            {/* Time */}
            {showSender && formattedTime && (
              <div className="flex justify-end mb-0.5">
                <span className="text-[11px] text-muted-foreground">{formattedTime}</span>
                {message.isEdit && <span className="text-[10px] text-muted-foreground italic ml-1">(đã sửa)</span>}
              </div>
            )}

            {isEditing ? (
              <div className="space-y-2">
                <Input value={editContent} onChange={(e) => setEditContent(e.target.value)}
                  onKeyDown={(e) => { if (e.key === "Enter" && !e.shiftKey) { e.preventDefault(); void handleSaveEdit(); } if (e.key === "Escape") handleCancelEdit(); }}
                  className="text-sm" autoFocus />
                <div className="flex gap-2 justify-end">
                  <Button variant="ghost" size="sm" onClick={handleCancelEdit} disabled={isUpdating}>Hủy</Button>
                  <Button size="sm" onClick={handleSaveEdit} disabled={isUpdating || !editContent.trim()}>{isUpdating ? "Đang lưu..." : "Lưu"}</Button>
                </div>
              </div>
            ) : (
              <div className="relative group/bubble">
                <div className="bg-primary text-primary-foreground rounded-lg rounded-br-sm px-3 py-2 w-fit max-w-[80vw] md:max-w-[60vw]">
                  {/* Reply quote */}
                  {parentMessage && (
                    <button type="button" onClick={() => onScrollToMessage?.(parentMessage.id!)}
                      className="mb-2 flex items-start gap-2 bg-primary-foreground/10 border-l-2 border-primary-foreground/50 px-2 py-1 text-left rounded-r w-full">
                      <div className="flex-1 min-w-0">
                        <div className="text-xs font-semibold text-primary-foreground/80 truncate">{parentMessage.senderName || "Người dùng"}</div>
                        <div className="text-xs text-primary-foreground/60 truncate">{parentMessage.content}</div>
                      </div>
                    </button>
                  )}
                  <div className="text-sm leading-relaxed whitespace-pre-wrap wrap-break-word">{parseDocumentEmbeds(message.content || "")}</div>
                  {/* Files */}
                  {message.files && message.files.length > 0 && (
                    <div className="mt-2 space-y-1.5">
                      {message.files.map((file) => {
                        const isImage = file.mimeType?.startsWith("image/");
                        const fileUrl = getFileUrlById(file.fileId);
                        return isImage ? (
                          <a key={file.fileId} href={fileUrl} target="_blank" rel="noopener noreferrer" className="block">
                            {/* eslint-disable-next-line @next/next/no-img-element */}
                            <img src={fileUrl} alt="Image" className="max-w-xs max-h-48 rounded border border-primary-foreground/30" />
                          </a>
                        ) : (
                          <a key={file.fileId} href={fileUrl} target="_blank" rel="noopener noreferrer"
                            className="inline-flex items-center gap-2 text-xs p-2 rounded border border-primary-foreground/30 bg-primary-foreground/10 hover:bg-primary-foreground/20 transition-colors">
                            <Paperclip className="h-3.5 w-3.5" />
                            <span className="truncate max-w-[200px]">{file.fileName || "Tệp đính kèm"}</span>
                          </a>
                        );
                      })}
                    </div>
                  )}
                  {message.isPined && (
                    <div className="mt-1 flex items-center gap-1 text-[10px] text-primary-foreground/70"><Pin className="h-3 w-3" /><span>Đã ghim</span></div>
                  )}
                </div>
                {/* Actions */}
                <DropdownMenu>
                  <DropdownMenuTrigger asChild>
                    <Button variant="ghost" size="sm" className="absolute top-1 right-1 h-6 w-6 p-0 opacity-0 group-hover/bubble:opacity-100 transition-opacity bg-primary-foreground/10 hover:bg-primary-foreground/20">
                      <MoreVertical className="h-4 w-4" />
                    </Button>
                  </DropdownMenuTrigger>
                  <DropdownMenuContent align="end" className="min-w-[120px] p-1">
                    {onReply && <DropdownMenuItem onClick={() => onReply(message)} className="text-xs py-1.5 px-2"><Reply className="h-3.5 w-3.5 mr-2" />Trả lời</DropdownMenuItem>}
                    <DropdownMenuItem onClick={() => void togglePin(message)} disabled={isPinning} className="text-xs py-1.5 px-2">
                      {message.isPined ? <><PinOff className="h-3.5 w-3.5 mr-2" />Bỏ ghim</> : <><Pin className="h-3.5 w-3.5 mr-2" />Ghim</>}
                    </DropdownMenuItem>
                    <DropdownMenuItem onClick={handleEdit} className="text-xs py-1.5 px-2"><Edit className="h-3.5 w-3.5 mr-2" />Chỉnh sửa</DropdownMenuItem>
                    <DropdownMenuItem onClick={handleDelete} disabled={isDeleting} className="text-xs py-1.5 px-2 text-red-600 dark:text-red-400"><Trash2 className="h-3.5 w-3.5 mr-2" />Xóa</DropdownMenuItem>
                  </DropdownMenuContent>
                </DropdownMenu>
              </div>
            )}
          </div>
        </div>
      </div>
    );
  }

  // ===================== OTHER'S MESSAGE (căn trái, có avatar) =====================
  return (
    <div className={cn("group hover:bg-muted/30 transition-colors", compact ? "py-1" : "py-1.5")}>
      {formattedDate && (
        <div className="flex items-center justify-center mb-2">
          <span className="text-[11px] text-muted-foreground bg-muted px-2 py-0.5 rounded-full">{formattedDate}</span>
        </div>
      )}

      <div className="flex gap-3 px-2">
        {/* Avatar */}
        <div className={cn("shrink-0", compact ? "w-7" : "w-9")}>
          {showSender && (
            <Avatar className={cn(compact ? "h-7 w-7" : "h-9 w-9")}>
              <AvatarImage src={message.senderAvatarUrl || undefined} alt={message.senderName || "User"} />
              <AvatarFallback className={compact ? "text-xs" : "text-sm"}>{message.senderName?.[0]?.toUpperCase() || "U"}</AvatarFallback>
            </Avatar>
          )}
        </div>

        <div className="flex-1 min-w-0">
          {/* Sender name + time */}
          {showSender && (
            <div className="flex items-baseline gap-2 mb-0.5">
              <span className={cn("font-semibold text-foreground", compact ? "text-xs" : "text-sm")}>{message.senderName || "Người dùng"}</span>
              {formattedTime && <span className="text-[11px] text-muted-foreground">{formattedTime}</span>}
              {message.isEdit && <span className="text-[10px] text-muted-foreground italic">(đã sửa)</span>}
            </div>
          )}

          <div className="relative group/bubble">
            <div className="bg-secondary text-secondary-foreground rounded-lg rounded-bl-sm px-3 py-2 w-fit max-w-[80vw] md:max-w-[60vw]">
              {/* Reply quote */}
              {parentMessage && (
                <button type="button" onClick={() => onScrollToMessage?.(parentMessage.id!)}
                  className="mb-2 flex items-start gap-2 bg-muted border-l-2 border-muted-foreground/30 px-2 py-1 text-left rounded-r w-full">
                  <div className="flex-1 min-w-0">
                    <div className="text-xs font-semibold text-muted-foreground truncate">{parentMessage.senderName || "Người dùng"}</div>
                    <div className="text-xs text-muted-foreground truncate">{parentMessage.content}</div>
                  </div>
                </button>
              )}
              <div className="text-sm leading-relaxed whitespace-pre-wrap wrap-break-word">{parseDocumentEmbeds(message.content || "")}</div>
              {/* Files */}
              {message.files && message.files.length > 0 && (
                <div className="mt-2 space-y-1.5">
                  {message.files.map((file) => {
                    const isImage = file.mimeType?.startsWith("image/");
                    const fileUrl = getFileUrlById(file.fileId);
                    return isImage ? (
                      <a key={file.fileId} href={fileUrl} target="_blank" rel="noopener noreferrer" className="block">
                        {/* eslint-disable-next-line @next/next/no-img-element */}
                        <img src={fileUrl} alt="Image" className="max-w-xs max-h-48 rounded border border-border" />
                      </a>
                    ) : (
                      <a key={file.fileId} href={fileUrl} target="_blank" rel="noopener noreferrer"
                        className="inline-flex items-center gap-2 text-xs p-2 rounded border border-border bg-muted/50 hover:bg-muted transition-colors">
                        <Paperclip className="h-3.5 w-3.5" />
                        <span className="truncate max-w-[200px]">{file.fileName || "Tệp đính kèm"}</span>
                      </a>
                    );
                  })}
                </div>
              )}
              {message.isPined && (
                <div className="mt-1 flex items-center gap-1 text-[10px] text-primary"><Pin className="h-3 w-3" /><span>Đã ghim</span></div>
              )}
            </div>
            {/* Time for grouped messages */}
            {!showSender && formattedTime && (
              <div className="text-[10px] text-muted-foreground mt-0.5 opacity-0 group-hover:opacity-100 transition-opacity">{formattedTime}</div>
            )}
            {/* Actions */}
            <DropdownMenu>
              <DropdownMenuTrigger asChild>
                <Button variant="ghost" size="sm" className="absolute top-1 right-1 h-6 w-6 p-0 opacity-0 group-hover/bubble:opacity-100 transition-opacity">
                  <MoreVertical className="h-4 w-4" />
                </Button>
              </DropdownMenuTrigger>
              <DropdownMenuContent align="end" className="min-w-[120px] p-1">
                {onReply && <DropdownMenuItem onClick={() => onReply(message)} className="text-xs py-1.5 px-2"><Reply className="h-3.5 w-3.5 mr-2" />Trả lời</DropdownMenuItem>}
              </DropdownMenuContent>
            </DropdownMenu>
          </div>
        </div>
      </div>
    </div>
  );
}

function getSystemMessageText(message: MessageDto, currentUserId?: string) {
  const actorName = message.senderName || "Hệ thống";
  const isSelf = currentUserId && message.createdById === currentUserId;
  switch (message.type) {
    case 0: return "Cuộc trò chuyện đã được tạo";
    case 1: return isSelf ? "Bạn đã tham gia nhóm" : `${actorName} đã tham gia nhóm`;
    case 2: return isSelf ? "Bạn đã rời nhóm" : `${actorName} đã rời nhóm`;
    case 3: return `${actorName} đã được cập nhật vai trò`;
    case 4: return isSelf ? "Bạn đã ghim tin nhắn" : `${actorName} đã ghim tin nhắn`;
    case 5: return isSelf ? "Bạn đã bỏ ghim tin nhắn" : `${actorName} đã bỏ ghim tin nhắn`;
    case 6: return isSelf ? "Yêu cầu tham gia của bạn đã được duyệt" : `Yêu cầu tham gia của ${actorName} đã được duyệt`;
    default: return "Thông báo hệ thống";
  }
}
