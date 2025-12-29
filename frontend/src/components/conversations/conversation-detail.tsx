"use client";

import { useEffect, useState, useRef, useCallback, useMemo } from "react";
import { ArrowLeft, Send, Loader2, Paperclip, List, Settings, FolderOpen, X, Image as ImageIcon, UserPlus } from "lucide-react";

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
import { EditConversationSidebar } from "@/src/components/conversations/edit-conversation-sidebar";
import { JoinRequestSidebar } from "@/src/components/conversations/join-request-sidebar";
import { ConversationFilesSidebar } from "@/src/components/conversations/conversation-files-sidebar";
import { MessageItem } from "@/src/components/conversations/message-item";
import { PinnedMessagesSection } from "@/src/components/conversations/pinned-messages-section";
import { useUserProfile } from "@/src/hooks/use-user-profile";
import { usePinMessage } from "@/src/hooks/use-pin-message";
import { useFileUpload } from "@/src/hooks/use-file-upload";
import { useSignalR } from "@/src/components/providers/signalr-provider";

interface ConversationDetailProps {
  conversationId: string;
  onBack: () => void;
}

export function ConversationDetail({
  conversationId,
  onBack,
}: ConversationDetailProps) {
  const { profile } = useUserProfile();
  const [conversation, setConversation] = useState<ConversationDetailDto | null>(null);
  const [messages, setMessages] = useState<MessageDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [sending, setSending] = useState(false);
  const [messageContent, setMessageContent] = useState("");
  const [selectedFiles, setSelectedFiles] = useState<File[]>([]);
  const [replyTo, setReplyTo] = useState<MessageDto | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [showEditSidebar, setShowEditSidebar] = useState(false);
  const [showFilesSidebar, setShowFilesSidebar] = useState(false);
  const [showJoinRequestSidebar, setShowJoinRequestSidebar] = useState(false);
  const [typingUsers, setTypingUsers] = useState<Map<string, string>>(new Map());
  const messagesEndRef = useRef<HTMLDivElement>(null);
  const scrollAreaRef = useRef<HTMLDivElement>(null);
  const fileInputRef = useRef<HTMLInputElement>(null);
  const typingTimeoutRef = useRef<NodeJS.Timeout | null>(null);

  // Check if current user is owner or deputy
  const isOwnerOrDeputy = useMemo(() => {
    if (!conversation?.members || !profile?.id) return false;
    const currentMember = conversation.members.find(m => m.userId === profile.id);
    return currentMember?.roleType === 2 || currentMember?.roleType === 1; // Owner = 2, Deputy = 1
  }, [conversation?.members, profile?.id]);

  // Check if conversation is private
  const isPrivate = conversation?.visibility === 0;

  // SignalR integration
  const {
    isConnected,
    joinConversation,
    leaveConversation,
    sendTyping,
    onMessageReceived,
    onMessageUpdated,
    onMessageDeleted,
    onMessagePinned,
    onMessageUnpinned,
    onUserTyping,
  } = useSignalR();

  const { togglePin } = usePinMessage(conversationId, (updatedMessage) => {
    if (!updatedMessage) return;
    setMessages((prev) =>
      prev.map((m) => (m.id === updatedMessage.id ? updatedMessage : m))
    );
  });

  const pinnedMessages = messages
    .filter((m) => m.isPined)
    .sort((a, b) => {
      const dateA = a.createdAt ? new Date(a.createdAt).getTime() : 0;
      const dateB = b.createdAt ? new Date(b.createdAt).getTime() : 0;
      return dateB - dateA; // Mới nhất trước
    });

  // Mark message as read callback
  const markLastMessageAsRead = useCallback(async (messageId: string) => {
    try {
      await postApiConversationsByConversationIdMessagesByIdMarkAsRead({
        path: { conversationId, id: messageId },
      });
    } catch (err) {
      // Silently fail - not critical
      console.error("Failed to mark message as read:", err);
    }
  }, [conversationId]);

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
    // Message received
    const unsubReceived = onMessageReceived((message) => {
      if (message.conversationId === conversationId) {
        setMessages((prev) => {
          // Avoid duplicates
          if (prev.some((m) => m.id === message.id)) return prev;
          return [...prev, message];
        });

        // Mark as read if not own message
        if (message.id && message.createdById !== profile?.id) {
          void markLastMessageAsRead(message.id);
        }
      }
    });

    // Message updated
    const unsubUpdated = onMessageUpdated((message) => {
      if (message.conversationId === conversationId) {
        setMessages((prev) =>
          prev.map((m) => (m.id === message.id ? message : m))
        );
      }
    });

    // Message deleted
    const unsubDeleted = onMessageDeleted((data) => {
      if (data.conversationId === conversationId) {
        setMessages((prev) => prev.filter((m) => m.id !== data.messageId));
      }
    });

    // Message pinned
    const unsubPinned = onMessagePinned((message) => {
      if (message.conversationId === conversationId) {
        setMessages((prev) =>
          prev.map((m) => (m.id === message.id ? message : m))
        );
      }
    });

    // Message unpinned
    const unsubUnpinned = onMessageUnpinned((message) => {
      if (message.conversationId === conversationId) {
        setMessages((prev) =>
          prev.map((m) => (m.id === message.id ? message : m))
        );
      }
    });

    // User typing
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
      unsubPinned();
      unsubUnpinned();
      unsubTyping();
    };
  }, [conversationId, profile?.id, onMessageReceived, onMessageUpdated, onMessageDeleted, onMessagePinned, onMessageUnpinned, onUserTyping]);

  const fetchConversation = async () => {
    try {
      const response = await getApiConversationById({
        path: { id: conversationId },
      });
      const payload = (response.data ?? response) as ConversationDetailDto | undefined;
      if (payload) {
        setConversation(payload);
      }
    } catch (err: any) {
      console.error("Error fetching conversation:", err);
    }
  };

  const fetchMessages = async () => {
    setLoading(true);
    setError(null);

    try {
      const response = await getApiConversationsByConversationIdMessages({
        path: { conversationId },
        query: {
          Page: 1,
          PageSize: 50,
        },
      });

      const payload = (response.data ??
        response) as PagedResponseOfMessageDto | undefined;
      const items = payload?.items ?? [];

      setMessages(items);

      // Mark last message as read
      if (items.length > 0) {
        const lastMessage = items[items.length - 1];
        if (lastMessage.id && lastMessage.createdById !== profile?.id) {
          void markLastMessageAsRead(lastMessage.id);
        }
      }
    } catch (err: any) {
      const message =
        err?.response?.data?.message ||
        err?.message ||
        "Không thể tải tin nhắn";
      setError(message);
    } finally {
      setLoading(false);
    }
  };

  const handleFileSelect = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files) {
      const files = Array.from(e.target.files);
      setSelectedFiles((prev) => [...prev, ...files]);
    }
    // Reset input để có thể chọn lại file giống nhau
    if (fileInputRef.current) {
      fileInputRef.current.value = "";
    }
  };

  const handleRemoveFile = (index: number) => {
    setSelectedFiles((prev) => prev.filter((_, i) => i !== index));
  };

  const { uploadFiles } = useFileUpload();

  const handleSendMessage = async (e: React.FormEvent) => {
    e.preventDefault();
    if ((!messageContent.trim() && selectedFiles.length === 0) || sending) return;

    setSending(true);
    setError(null);
    try {
      const fileIds: string[] = [];
      if (selectedFiles.length > 0) {
        const uploaded = await uploadFiles(selectedFiles);
        const ids = uploaded
          .map((f) => f.id)
          .filter((id): id is string => typeof id === "string" && id.length > 0);
        fileIds.push(...ids);
      }

      const response = await postApiConversationsByConversationIdMessages({
        path: { conversationId },
        body: {
          conversationId,
          parentId: replyTo?.id,
          content: messageContent.trim() || undefined,
          fileIds: fileIds.length > 0 ? fileIds : undefined,
        },
      });

      const newMessage = (response.data ?? response) as MessageDto | undefined;
      if (newMessage) {
        setMessageContent("");
        setSelectedFiles([]);
        setReplyTo(null);
        void fetchConversation();
      }
    } catch (err: any) {
      const message =
        err?.response?.data?.message ||
        err?.message ||
        "Không thể gửi tin nhắn";
      setError(message);
    } finally {
      setSending(false);
    }
  };

  const isImageFile = (file: File) => {
    return file.type.startsWith("image/");
  };

  // Handle typing indicator
  const handleTyping = useCallback(() => {
    if (!isConnected) return;

    // Send typing = true
    sendTyping(conversationId, true);

    // Clear previous timeout
    if (typingTimeoutRef.current) {
      clearTimeout(typingTimeoutRef.current);
    }

    // Set timeout to send typing = false after 2 seconds of no typing
    typingTimeoutRef.current = setTimeout(() => {
      sendTyping(conversationId, false);
    }, 2000);
  }, [isConnected, conversationId, sendTyping]);

  const scrollToBottom = () => {
    if (messagesEndRef.current) {
      messagesEndRef.current.scrollIntoView({ behavior: "smooth" });
    }
  };

  if (loading && !conversation) {
    return (
      <div className="flex h-full items-center justify-center">
        <Loader2 className="h-6 w-6 animate-spin text-primary" />
      </div>
    );
  }

  if (!conversation) {
    return (
      <div className="flex h-full items-center justify-center">
        <p className="text-muted-foreground">
          Không tìm thấy cuộc trò chuyện
        </p>
      </div>
    );
  }

  return (
    <div className="flex h-full flex-col overflow-hidden">
      {/* Header */}
      <div className="flex-shrink-0 border-b border-border bg-card px-4 py-3">
        <div className="flex items-center gap-3">
          <Button
            variant="ghost"
            size="sm"
            onClick={onBack}
            className="h-8 w-8 p-0"
            title="Quay lại danh sách"
          >
            <ArrowLeft className="h-4 w-4 md:hidden" />
            <List className="h-4 w-4 hidden md:block" />
          </Button>
          <div className="flex-1 min-w-0">
            <h2 className="text-sm font-semibold text-foreground truncate">
              {conversation.conversationName || "Cuộc trò chuyện"}
            </h2>
            {conversation.tags && conversation.tags.length > 0 && (
              <div className="flex flex-wrap gap-1 mt-1">
                {conversation.tags.map((tag) => (
                  <span
                    key={tag.id}
                    className="inline-flex items-center rounded-full bg-secondary px-2 py-0.5 text-[10px] font-semibold uppercase tracking-wide text-secondary-foreground"
                  >
                    {tag.tagName}
                  </span>
                ))}
              </div>
            )}
          </div>
          <div className="flex items-center gap-1">
            <Button
              variant="ghost"
              size="sm"
              onClick={() => setShowFilesSidebar(true)}
              className="h-8 w-8 p-0"
              title="Xem tệp đã gửi"
            >
              <FolderOpen className="h-4 w-4" />
            </Button>
            {/* Join Requests Button - Only for private groups and owner/deputy */}
            {isPrivate && isOwnerOrDeputy && (
              <Button
                variant="ghost"
                size="sm"
                onClick={() => setShowJoinRequestSidebar(true)}
                className="h-8 w-8 p-0"
                title="Yêu cầu tham gia"
              >
                <UserPlus className="h-4 w-4" />
              </Button>
            )}
            <Button
              variant="ghost"
              size="sm"
              onClick={() => setShowEditSidebar(true)}
              className="h-8 w-8 p-0"
              title="Chỉnh sửa cuộc trò chuyện"
            >
              <Settings className="h-4 w-4" />
            </Button>
          </div>
        </div>
      </div>

      {/* Pinned Messages Section */}
      {pinnedMessages.length > 0 && (
        <div className="flex-shrink-0">
          <PinnedMessagesSection
            pinnedMessages={pinnedMessages}
            conversationId={conversationId}
            onMessageClick={(messageId) => {
              // Scroll to message
              const element = document.querySelector(`[data-message-id="${messageId}"]`);
              if (element) {
                element.scrollIntoView({ behavior: "smooth", block: "center" });
              }
            }}
            onUnpin={(message) => {
              void togglePin(message);
            }}
          />
        </div>
      )}

      {/* Messages */}
      <ScrollArea className="flex-1 min-h-0 p-2 md:p-1" ref={scrollAreaRef}>
        <div className="flex flex-col gap-4">
          {error && (
            <div className="p-3 text-sm text-red-600 dark:text-red-400 bg-red-50 dark:bg-red-950 rounded">
              {error}
            </div>
          )}

          {messages.length === 0 ? (
            <div className="text-center py-8 text-muted-foreground">
              <p>Chưa có tin nhắn nào</p>
            </div>
          ) : (
            messages.map((message, index) => {
              // So sánh ngày của tin nhắn hiện tại với tin nhắn trước đó
              const currentDate = message.createdAt
                ? new Date(message.createdAt)
                : null;
              const previousMessage = index > 0 ? messages[index - 1] : null;
              const previousDate =
                previousMessage?.createdAt
                  ? new Date(previousMessage.createdAt)
                  : null;

              // Chỉ hiển thị date nếu là tin nhắn đầu tiên hoặc khác ngày với tin nhắn trước
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
                        prev.map((m) =>
                          m.id === updatedMessage.id ? updatedMessage : m
                        )
                      );
                    }}
                    onDelete={(messageId) => {
                      setMessages((prev) => prev.filter((m) => m.id !== messageId));
                    }}
                  />
                </div>
              );
            })
          )}

          {/* Typing indicator */}
          {typingUsers.size > 0 && (
            <div className="flex items-center gap-2 px-4 py-2 text-sm text-muted-foreground">
              <div className="flex gap-1">
                <span className="w-2 h-2 bg-muted-foreground rounded-full animate-bounce" style={{ animationDelay: '0ms' }} />
                <span className="w-2 h-2 bg-muted-foreground rounded-full animate-bounce" style={{ animationDelay: '150ms' }} />
                <span className="w-2 h-2 bg-muted-foreground rounded-full animate-bounce" style={{ animationDelay: '300ms' }} />
              </div>
              <span>
                {typingUsers.size === 1
                  ? "Ai đó đang nhập..."
                  : `${typingUsers.size} người đang nhập...`}
              </span>
            </div>
          )}

          <div ref={messagesEndRef} />
        </div>
      </ScrollArea>

      {/* Message Input */}
      <div className="flex-shrink-0 border-t border-border bg-card p-2 md:p-4">
        {/* Reply preview */}
        {replyTo && (
          <div className="mb-2 flex items-start justify-between bg-muted px-3 py-2 text-xs text-foreground">
            <div className="mr-2 min-w-0">
              <div className="font-semibold truncate">
                Trả lời {replyTo.senderName || "người dùng"}
              </div>
              <div className="truncate text-[11px] opacity-80">
                {replyTo.content}
              </div>
            </div>
            <button
              type="button"
              onClick={() => setReplyTo(null)}
              className="ml-2 text-muted-foreground hover:text-foreground"
              title="Hủy trả lời"
            >
              <X className="h-3 w-3" />
            </button>
          </div>
        )}

        {/* Preview selected files */}
        {selectedFiles.length > 0 && (
          <div className="mb-2 space-y-2">
            <div className="flex flex-wrap gap-2">
              {selectedFiles.map((file, index) => (
                <div
                  key={index}
                  className="relative group border border-border overflow-hidden"
                >
                  {isImageFile(file) ? (
                    <div className="relative">
                      <img
                        src={URL.createObjectURL(file)}
                        alt={file.name}
                        className="h-20 w-20 object-cover"
                      />
                      <button
                        type="button"
                        onClick={() => handleRemoveFile(index)}
                        className="absolute top-1 right-1 bg-red-500 text-white rounded-full p-1 opacity-0 group-hover:opacity-100 transition-opacity"
                      >
                        <X className="h-3 w-3" />
                      </button>
                    </div>
                  ) : (
                    <div className="flex items-center gap-2 p-2 bg-muted">
                      <Paperclip className="h-4 w-4 text-muted-foreground" />
                      <span className="text-xs text-foreground truncate max-w-[120px]">
                        {file.name}
                      </span>
                      <button
                        type="button"
                        onClick={() => handleRemoveFile(index)}
                        className="text-red-500 hover:text-red-700"
                      >
                        <X className="h-3 w-3" />
                      </button>
                    </div>
                  )}
                </div>
              ))}
            </div>
          </div>
        )}

        <form onSubmit={handleSendMessage} className="space-y-2">
          <div className="flex gap-2">
            <input
              type="file"
              ref={fileInputRef}
              multiple
              onChange={handleFileSelect}
              className="hidden"
              id="message-file-input"
              accept="image/*,application/pdf,.doc,.docx,.xls,.xlsx,.ppt,.pptx,.txt"
            />
            <label
              htmlFor="message-file-input"
              className="flex items-center justify-center p-2 border border-border hover:bg-muted cursor-pointer transition-colors"
              title="Đính kèm file"
            >
              <Paperclip className="h-5 w-5 text-muted-foreground" />
            </label>
            <Input
              type="text"
              placeholder="Nhập tin nhắn..."
              value={messageContent}
              onChange={(e) => {
                setMessageContent(e.target.value);
                handleTyping();
              }}
              disabled={sending}
              className="flex-1 text-sm md:text-base"
              onKeyDown={(e) => {
                if (e.key === "Enter" && !e.shiftKey) {
                  e.preventDefault();
                  void handleSendMessage(e);
                }
              }}
            />
            <Button
              type="submit"
              disabled={sending || (!messageContent.trim() && selectedFiles.length === 0)}
              size="sm"
              className="md:size-default"
            >
              {sending ? (
                <Loader2 className="h-4 w-4 animate-spin" />
              ) : (
                <Send className="h-4 w-4" />
              )}
            </Button>
          </div>
        </form>
      </div>

      {/* Edit Conversation Sidebar */}
      <EditConversationSidebar
        open={showEditSidebar}
        onClose={() => setShowEditSidebar(false)}
        conversation={conversation}
        onSuccess={() => {
          void fetchConversation();
        }}
      />

      {/* Files Sidebar */}
      <ConversationFilesSidebar
        open={showFilesSidebar}
        onClose={() => setShowFilesSidebar(false)}
        messages={messages}
      />

      {/* Join Requests Sidebar */}
      <JoinRequestSidebar
        open={showJoinRequestSidebar}
        onClose={() => setShowJoinRequestSidebar(false)}
        conversationId={conversationId}
        conversationName={conversation.conversationName || "Cuộc trò chuyện"}
        onSuccess={() => {
          void fetchConversation();
        }}
      />
    </div>
  );
}



