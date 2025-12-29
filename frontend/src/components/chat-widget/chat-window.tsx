"use client";

import { useEffect, useState, useRef, useCallback, useMemo } from "react";
import {
  Minus,
  X,
  Send,
  Loader2,
  Paperclip,
  MoreHorizontal,
  Settings,
  MessageCircle,
  Maximize2,
} from "lucide-react";
import { useRouter } from "next/navigation";

import {
  getApiConversationById,
  getApiConversationsByConversationIdMessages,
  postApiConversationsByConversationIdMessages,
  postApiConversationsByConversationIdMessagesByIdMarkAsRead,
} from "@/src/api/database/sdk.gen";
import type {
  ConversationDetailDto,
  MessageDto2 as MessageDto,
  PagedResponseOfMessageDto,
} from "@/src/api/database/types.gen";
import { Button } from "@/src/components/ui/button";
import { Input } from "@/src/components/ui/input";
import { ScrollArea } from "@/src/components/ui/scroll-area";
import { Avatar, AvatarFallback, AvatarImage } from "@/src/components/ui/avatar";
import { MessageItem } from "@/src/components/conversations/message-item";
import { useUserProfile } from "@/src/hooks/use-user-profile";
import { useFileUpload } from "@/src/hooks/use-file-upload";
import { useSignalR } from "@/src/components/providers/signalr-provider";
import { cn } from "@/lib/utils";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/src/components/ui/dropdown-menu";

interface ChatWindowProps {
  conversationId: string;
  onClose: () => void;
  onMinimize: () => void;
  onMarkAsRead: () => void;
}

export function ChatWindow({
  conversationId,
  onClose,
  onMinimize,
  onMarkAsRead,
}: ChatWindowProps) {
  const router = useRouter();
  const { profile } = useUserProfile();
  const [conversation, setConversation] = useState<ConversationDetailDto | null>(null);
  const [messages, setMessages] = useState<MessageDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [sending, setSending] = useState(false);
  const [messageContent, setMessageContent] = useState("");
  const [selectedFiles, setSelectedFiles] = useState<File[]>([]);
  const [replyTo, setReplyTo] = useState<MessageDto | null>(null);
  const [typingUsers, setTypingUsers] = useState<Map<string, string>>(new Map());
  const messagesEndRef = useRef<HTMLDivElement>(null);
  const fileInputRef = useRef<HTMLInputElement>(null);
  const typingTimeoutRef = useRef<NodeJS.Timeout | null>(null);

  const {
    isConnected,
    joinConversation,
    leaveConversation,
    sendTyping,
    onMessageReceived,
    onMessageUpdated,
    onMessageDeleted,
    onUserTyping,
  } = useSignalR();

  const { uploadFiles } = useFileUpload();

  // Mark message as read callback
  const markLastMessageAsRead = useCallback(async (messageId: string) => {
    try {
      await postApiConversationsByConversationIdMessagesByIdMarkAsRead({
        path: { conversationId, id: messageId },
      });
      onMarkAsRead();
    } catch (err) {
      console.error("Failed to mark message as read:", err);
    }
  }, [conversationId, onMarkAsRead]);

  useEffect(() => {
    void fetchConversation();
    void fetchMessages();
  }, [conversationId]);

  useEffect(() => {
    scrollToBottom();
  }, [messages]);

  // SignalR: Join/Leave conversation
  useEffect(() => {
    if (isConnected && conversationId) {
      joinConversation(conversationId);
      return () => {
        leaveConversation(conversationId);
      };
    }
  }, [isConnected, conversationId, joinConversation, leaveConversation]);

  // SignalR: Subscribe to message events
  useEffect(() => {
    const unsubReceived = onMessageReceived((message) => {
      if (message.conversationId === conversationId) {
        setMessages((prev) => {
          if (prev.some((m) => m.id === message.id)) return prev;
          return [...prev, message];
        });

        if (message.id && message.createdById !== profile?.id) {
          void markLastMessageAsRead(message.id);
        }
      }
    });

    const unsubUpdated = onMessageUpdated((message) => {
      if (message.conversationId === conversationId) {
        setMessages((prev) =>
          prev.map((m) => (m.id === message.id ? message : m))
        );
      }
    });

    const unsubDeleted = onMessageDeleted((data) => {
      if (data.conversationId === conversationId) {
        setMessages((prev) => prev.filter((m) => m.id !== data.messageId));
      }
    });

    const unsubTyping = onUserTyping((data) => {
      if (data.conversationId === conversationId && data.userId !== profile?.id) {
        setTypingUsers((prev) => {
          const next = new Map(prev);
          if (data.isTyping) {
            next.set(data.userId, data.userId);
          } else {
            next.delete(data.userId);
          }
          return next;
        });
      }
    });

    return () => {
      unsubReceived();
      unsubUpdated();
      unsubDeleted();
      unsubTyping();
    };
  }, [conversationId, profile?.id, onMessageReceived, onMessageUpdated, onMessageDeleted, onUserTyping, markLastMessageAsRead]);

  const fetchConversation = async () => {
    try {
      const response = await getApiConversationById({
        path: { id: conversationId },
      });
      const payload = (response.data ?? response) as ConversationDetailDto | undefined;
      if (payload) {
        setConversation(payload);
      }
    } catch (err) {
      console.error("Error fetching conversation:", err);
    }
  };

  const fetchMessages = async () => {
    setLoading(true);
    try {
      const response = await getApiConversationsByConversationIdMessages({
        path: { conversationId },
        query: { Page: 1, PageSize: 30 },
      });

      const payload = (response.data ?? response) as PagedResponseOfMessageDto | undefined;
      const items = payload?.items ?? [];
      setMessages(items);

      if (items.length > 0) {
        const lastMessage = items[items.length - 1];
        if (lastMessage.id && lastMessage.createdById !== profile?.id) {
          void markLastMessageAsRead(lastMessage.id);
        }
      }
    } catch (err) {
      console.error("Error fetching messages:", err);
    } finally {
      setLoading(false);
    }
  };

  const handleFileSelect = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files) {
      const files = Array.from(e.target.files);
      setSelectedFiles((prev) => [...prev, ...files]);
    }
    if (fileInputRef.current) {
      fileInputRef.current.value = "";
    }
  };

  const handleRemoveFile = (index: number) => {
    setSelectedFiles((prev) => prev.filter((_, i) => i !== index));
  };

  const handleSendMessage = async (e: React.FormEvent) => {
    e.preventDefault();
    if ((!messageContent.trim() && selectedFiles.length === 0) || sending) return;

    setSending(true);
    try {
      const fileIds: string[] = [];
      if (selectedFiles.length > 0) {
        const uploaded = await uploadFiles(selectedFiles);
        const ids = uploaded
          .map((f) => f.id)
          .filter((id): id is string => typeof id === "string" && id.length > 0);
        fileIds.push(...ids);
      }

      await postApiConversationsByConversationIdMessages({
        path: { conversationId },
        body: {
          conversationId,
          parentId: replyTo?.id,
          content: messageContent.trim() || undefined,
          fileIds: fileIds.length > 0 ? fileIds : undefined,
        },
      });

      setMessageContent("");
      setSelectedFiles([]);
      setReplyTo(null);
    } catch (err) {
      console.error("Error sending message:", err);
    } finally {
      setSending(false);
    }
  };

  const handleTyping = useCallback(() => {
    if (!isConnected) return;
    sendTyping(conversationId, true);

    if (typingTimeoutRef.current) {
      clearTimeout(typingTimeoutRef.current);
    }

    typingTimeoutRef.current = setTimeout(() => {
      sendTyping(conversationId, false);
    }, 2000);
  }, [isConnected, conversationId, sendTyping]);

  const scrollToBottom = () => {
    if (messagesEndRef.current) {
      messagesEndRef.current.scrollIntoView({ behavior: "smooth" });
    }
  };

  const handleOpenFullPage = () => {
    router.push(`/chat?id=${conversationId}`);
    onClose();
  };

  const isImageFile = (file: File) => file.type.startsWith("image/");

  return (
    <div className="flex flex-col h-full bg-background border border-border rounded-t-lg shadow-2xl overflow-hidden">
      {/* Header */}
      <div className="flex-shrink-0 flex items-center justify-between px-3 py-2 bg-primary text-primary-foreground">
        <div className="flex items-center gap-2 min-w-0 flex-1">
          <Avatar className="h-8 w-8 flex-shrink-0">
            <AvatarImage
              src={conversation?.avatarUrl || undefined}
              alt={conversation?.conversationName || "Avatar"}
            />
            <AvatarFallback className="bg-primary-foreground/20 text-primary-foreground text-xs">
              <MessageCircle className="h-4 w-4" />
            </AvatarFallback>
          </Avatar>
          <div className="min-w-0">
            <h3 className="text-sm font-semibold truncate">
              {conversation?.conversationName || "Chat"}
            </h3>
            {typingUsers.size > 0 && (
              <p className="text-xs text-primary-foreground/80">Đang nhập...</p>
            )}
          </div>
        </div>
        <div className="flex items-center gap-1">
          <DropdownMenu>
            <DropdownMenuTrigger asChild>
              <Button variant="ghost" size="sm" className="h-7 w-7 p-0 text-primary-foreground hover:bg-primary-foreground/20">
                <MoreHorizontal className="h-4 w-4" />
              </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end">
              <DropdownMenuItem onClick={handleOpenFullPage}>
                <Maximize2 className="h-4 w-4 mr-2" />
                Mở toàn màn hình
              </DropdownMenuItem>
              <DropdownMenuItem onClick={handleOpenFullPage}>
                <Settings className="h-4 w-4 mr-2" />
                Cài đặt nhóm
              </DropdownMenuItem>
            </DropdownMenuContent>
          </DropdownMenu>
          <Button
            variant="ghost"
            size="sm"
            onClick={onMinimize}
            className="h-7 w-7 p-0 text-primary-foreground hover:bg-primary-foreground/20"
            title="Thu nhỏ"
          >
            <Minus className="h-4 w-4" />
          </Button>
          <Button
            variant="ghost"
            size="sm"
            onClick={onClose}
            className="h-7 w-7 p-0 text-primary-foreground hover:bg-primary-foreground/20"
            title="Đóng"
          >
            <X className="h-4 w-4" />
          </Button>
        </div>
      </div>

      {/* Messages */}
      <ScrollArea className="flex-1 min-h-0">
        <div className="flex flex-col gap-2 p-2">
          {loading ? (
            <div className="flex items-center justify-center py-8">
              <Loader2 className="h-5 w-5 animate-spin text-muted-foreground" />
            </div>
          ) : messages.length === 0 ? (
            <div className="text-center py-8 text-sm text-muted-foreground">
              Chưa có tin nhắn
            </div>
          ) : (
            messages.map((message, index) => {
              const currentDate = message.createdAt ? new Date(message.createdAt) : null;
              const previousMessage = index > 0 ? messages[index - 1] : null;
              const previousDate = previousMessage?.createdAt ? new Date(previousMessage.createdAt) : null;
              const showDate =
                index === 0 ||
                !currentDate ||
                !previousDate ||
                currentDate.toDateString() !== previousDate.toDateString();

              return (
                <div key={message.id} data-message-id={message.id}>
                  <MessageItem
                    message={message}
                    conversationId={conversationId}
                    currentUserId={profile?.id}
                    showDate={showDate}
                    allMessages={messages}
                    onReply={(m) => setReplyTo(m)}
                    onScrollToMessage={(messageId) => {
                      const element = document.querySelector(`[data-message-id="${messageId}"]`);
                      if (element) {
                        element.scrollIntoView({ behavior: "smooth", block: "center" });
                      }
                    }}
                    onUpdate={(updatedMessage) => {
                      setMessages((prev) =>
                        prev.map((m) => (m.id === updatedMessage.id ? updatedMessage : m))
                      );
                    }}
                    onDelete={(messageId) => {
                      setMessages((prev) => prev.filter((m) => m.id !== messageId));
                    }}
                    compact
                  />
                </div>
              );
            })
          )}

          {/* Typing indicator */}
          {typingUsers.size > 0 && (
            <div className="flex items-center gap-1 px-2 py-1 text-xs text-muted-foreground">
              <div className="flex gap-0.5">
                <span className="w-1.5 h-1.5 bg-muted-foreground rounded-full animate-bounce" style={{ animationDelay: '0ms' }} />
                <span className="w-1.5 h-1.5 bg-muted-foreground rounded-full animate-bounce" style={{ animationDelay: '150ms' }} />
                <span className="w-1.5 h-1.5 bg-muted-foreground rounded-full animate-bounce" style={{ animationDelay: '300ms' }} />
              </div>
            </div>
          )}

          <div ref={messagesEndRef} />
        </div>
      </ScrollArea>

      {/* Reply preview */}
      {replyTo && (
        <div className="flex-shrink-0 flex items-center justify-between bg-muted px-2 py-1 border-t border-border">
          <div className="min-w-0 flex-1">
            <p className="text-xs font-medium truncate">Trả lời {replyTo.senderName}</p>
            <p className="text-xs text-muted-foreground truncate">{replyTo.content}</p>
          </div>
          <button onClick={() => setReplyTo(null)} className="text-muted-foreground hover:text-foreground ml-2">
            <X className="h-3 w-3" />
          </button>
        </div>
      )}

      {/* File preview */}
      {selectedFiles.length > 0 && (
        <div className="flex-shrink-0 flex gap-1 px-2 py-1 border-t border-border overflow-x-auto">
          {selectedFiles.map((file, index) => (
            <div key={index} className="relative group flex-shrink-0">
              {isImageFile(file) ? (
                <div className="relative w-12 h-12">
                  <img
                    src={URL.createObjectURL(file)}
                    alt={file.name}
                    className="w-12 h-12 object-cover rounded"
                  />
                  <button
                    onClick={() => handleRemoveFile(index)}
                    className="absolute -top-1 -right-1 bg-destructive text-destructive-foreground rounded-full w-4 h-4 flex items-center justify-center text-xs"
                  >
                    ×
                  </button>
                </div>
              ) : (
                <div className="flex items-center gap-1 px-2 py-1 bg-muted rounded text-xs">
                  <Paperclip className="h-3 w-3" />
                  <span className="max-w-[60px] truncate">{file.name}</span>
                  <button onClick={() => handleRemoveFile(index)} className="text-destructive">
                    <X className="h-3 w-3" />
                  </button>
                </div>
              )}
            </div>
          ))}
        </div>
      )}

      {/* Input */}
      <form onSubmit={handleSendMessage} className="flex-shrink-0 flex items-center gap-1 p-2 border-t border-border bg-card">
        <input
          type="file"
          ref={fileInputRef}
          multiple
          onChange={handleFileSelect}
          className="hidden"
          accept="image/*,application/pdf,.doc,.docx,.xls,.xlsx,.ppt,.pptx,.txt"
        />
        <Button
          type="button"
          variant="ghost"
          size="sm"
          className="h-8 w-8 p-0 flex-shrink-0"
          onClick={() => fileInputRef.current?.click()}
        >
          <Paperclip className="h-4 w-4" />
        </Button>
        <Input
          type="text"
          placeholder="Aa"
          value={messageContent}
          onChange={(e) => {
            setMessageContent(e.target.value);
            handleTyping();
          }}
          disabled={sending}
          className="flex-1 h-8 text-sm"
          onKeyDown={(e) => {
            if (e.key === "Enter" && !e.shiftKey) {
              e.preventDefault();
              void handleSendMessage(e);
            }
          }}
        />
        <Button
          type="submit"
          size="sm"
          disabled={sending || (!messageContent.trim() && selectedFiles.length === 0)}
          className="h-8 w-8 p-0 flex-shrink-0"
        >
          {sending ? (
            <Loader2 className="h-4 w-4 animate-spin" />
          ) : (
            <Send className="h-4 w-4" />
          )}
        </Button>
      </form>
    </div>
  );
}
