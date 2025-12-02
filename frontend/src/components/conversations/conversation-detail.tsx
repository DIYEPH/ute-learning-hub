"use client";

import { useEffect, useState, useRef } from "react";
import { ArrowLeft, Send, Loader2, Paperclip, List, Settings, FolderOpen, X, Image as ImageIcon } from "lucide-react";
// import { format } from "date-fns";
// import { vi } from "date-fns/locale";

import {
  getApiConversationById,
  getApiConversationsByConversationIdMessages,
  postApiConversationsByConversationIdMessages,
} from "@/src/api/database/sdk.gen";
import type {
  ConversationDetailDto,
  MessageDto,
  PagedResponseOfMessageDto,
} from "@/src/api/database/types.gen";
import { Button } from "@/src/components/ui/button";
import { Input } from "@/src/components/ui/input";
import { ScrollArea } from "@/src/components/ui/scroll-area";
import { EditConversationSidebar } from "@/src/components/conversations/edit-conversation-sidebar";
import { ConversationFilesSidebar } from "@/src/components/conversations/conversation-files-sidebar";
import { MessageItem } from "@/src/components/conversations/message-item";
import { PinnedMessagesSection } from "@/src/components/conversations/pinned-messages-section";
import { useUserProfile } from "@/src/hooks/use-user-profile";
import { usePinMessage } from "@/src/hooks/use-pin-message";

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
  const [error, setError] = useState<string | null>(null);
  const [showEditSidebar, setShowEditSidebar] = useState(false);
  const [showFilesSidebar, setShowFilesSidebar] = useState(false);
  const messagesEndRef = useRef<HTMLDivElement>(null);
  const scrollAreaRef = useRef<HTMLDivElement>(null);
  const fileInputRef = useRef<HTMLInputElement>(null);
  
  const { togglePin } = usePinMessage(conversationId, (updatedMessage) => {
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

  useEffect(() => {
    void fetchConversation();
    void fetchMessages();
  }, [conversationId]);

  useEffect(() => {
    scrollToBottom();
  }, [messages]);

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

  const handleSendMessage = async (e: React.FormEvent) => {
    e.preventDefault();
    if ((!messageContent.trim() && selectedFiles.length === 0) || sending) return;

    setSending(true);
    setError(null);
    try {
      const response = await postApiConversationsByConversationIdMessages({
        path: { conversationId },
        body: {
          Content: messageContent.trim() || undefined,
          Files: selectedFiles.length > 0 ? selectedFiles : undefined,
        },
      });

      const newMessage = (response.data ?? response) as MessageDto | undefined;
      if (newMessage) {
        setMessages((prev) => [...prev, newMessage]);
        setMessageContent("");
        setSelectedFiles([]);
        void fetchConversation(); // Refresh để cập nhật messageCount
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

  const scrollToBottom = () => {
    if (messagesEndRef.current) {
      messagesEndRef.current.scrollIntoView({ behavior: "smooth" });
    }
  };

  if (loading && !conversation) {
    return (
      <div className="flex h-full items-center justify-center">
        <Loader2 className="h-6 w-6 animate-spin text-sky-500" />
      </div>
    );
  }

  if (!conversation) {
    return (
      <div className="flex h-full items-center justify-center">
        <p className="text-slate-500 dark:text-slate-400">
          Không tìm thấy cuộc trò chuyện
        </p>
      </div>
    );
  }

  return (
    <div className="flex h-full flex-col">
      {/* Header */}
      <div className="border-b border-slate-200 bg-white px-4 py-3 dark:border-slate-700 dark:bg-slate-900">
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
                    className="inline-flex items-center rounded-full bg-slate-100 px-2 py-0.5 text-[10px] font-semibold uppercase tracking-wide text-slate-600 dark:bg-slate-800 dark:text-slate-300"
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
      )}

      {/* Messages */}
      <ScrollArea className="flex-1 p-2 md:p-4" ref={scrollAreaRef}>
        <div className="space-y-4">
          {error && (
            <div className="p-3 text-sm text-red-600 dark:text-red-400 bg-red-50 dark:bg-red-950 rounded">
              {error}
            </div>
          )}

          {messages.length === 0 ? (
            <div className="text-center py-8 text-slate-500 dark:text-slate-400">
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
          <div ref={messagesEndRef} />
        </div>
      </ScrollArea>

      {/* Message Input */}
      <div className="border-t border-slate-200 bg-white p-2 md:p-4 dark:border-slate-700 dark:bg-slate-900">
        {/* Preview selected files */}
        {selectedFiles.length > 0 && (
          <div className="mb-2 space-y-2">
            <div className="flex flex-wrap gap-2">
              {selectedFiles.map((file, index) => (
                <div
                  key={index}
                  className="relative group border border-slate-200 dark:border-slate-700 rounded-lg overflow-hidden"
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
                    <div className="flex items-center gap-2 p-2 bg-slate-50 dark:bg-slate-800">
                      <Paperclip className="h-4 w-4 text-slate-500" />
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
              className="flex items-center justify-center p-2 rounded-lg border border-slate-200 dark:border-slate-700 hover:bg-slate-50 dark:hover:bg-slate-800 cursor-pointer transition-colors"
              title="Đính kèm file"
            >
              <Paperclip className="h-5 w-5 text-slate-500" />
            </label>
            <Input
              type="text"
              placeholder="Nhập tin nhắn..."
              value={messageContent}
              onChange={(e) => setMessageContent(e.target.value)}
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
    </div>
  );
}

