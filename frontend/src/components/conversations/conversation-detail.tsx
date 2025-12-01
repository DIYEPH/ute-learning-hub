"use client";

import { useEffect, useState, useRef } from "react";
import { ArrowLeft, Send, Loader2, Paperclip } from "lucide-react";
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
import { Avatar, AvatarFallback, AvatarImage } from "@/src/components/ui/avatar";
import { ScrollArea } from "@/src/components/ui/scroll-area";

interface ConversationDetailProps {
  conversationId: string;
  onBack: () => void;
}

export function ConversationDetail({
  conversationId,
  onBack,
}: ConversationDetailProps) {
  const [conversation, setConversation] = useState<ConversationDetailDto | null>(null);
  const [messages, setMessages] = useState<MessageDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [sending, setSending] = useState(false);
  const [messageContent, setMessageContent] = useState("");
  const [error, setError] = useState<string | null>(null);
  const messagesEndRef = useRef<HTMLDivElement>(null);
  const scrollAreaRef = useRef<HTMLDivElement>(null);

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

  const handleSendMessage = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!messageContent.trim() || sending) return;

    setSending(true);
    try {
      const response = await postApiConversationsByConversationIdMessages({
        path: { conversationId },
        body: {
          Content: messageContent.trim(),
        },
      });

      const newMessage = (response.data ?? response) as MessageDto | undefined;
      if (newMessage) {
        setMessages((prev) => [...prev, newMessage]);
        setMessageContent("");
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
          >
            <ArrowLeft className="h-4 w-4" />
          </Button>
          <div className="flex-1">
            <h2 className="text-sm font-semibold text-foreground">
              {conversation.conversationName || "Cuộc trò chuyện"}
            </h2>
            {conversation.topic && (
              <p className="text-xs text-slate-500 dark:text-slate-400">
                {conversation.topic}
              </p>
            )}
          </div>
        </div>
      </div>

      {/* Messages */}
      <ScrollArea className="flex-1 p-4" ref={scrollAreaRef}>
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
            messages.map((message) => {
              const messageDate = message.createdAt
                ? new Date(message.createdAt)
                : null;
              const showDate = messageDate
                ? messageDate.toLocaleString("vi-VN", {
                    day: "2-digit",
                    month: "2-digit",
                    year: "numeric",
                    hour: "2-digit",
                    minute: "2-digit",
                  })
                : null;

              return (
                <div
                  key={message.id}
                  className="flex items-start gap-3 group hover:bg-slate-50 dark:hover:bg-slate-800/50 rounded-lg p-2 -m-2 transition-colors"
                >
                  <Avatar className="h-8 w-8 flex-shrink-0">
                    <AvatarImage
                      src={message.senderAvatarUrl || undefined}
                      alt={message.senderName || "User"}
                    />
                    <AvatarFallback>
                      {message.senderName?.[0]?.toUpperCase() || "U"}
                    </AvatarFallback>
                  </Avatar>

                  <div className="flex-1 min-w-0">
                    <div className="flex items-baseline gap-2 mb-1">
                      <span className="text-sm font-semibold text-foreground">
                        {message.senderName || "Người dùng"}
                      </span>
                      {showDate && (
                        <span className="text-xs text-slate-500 dark:text-slate-400">
                          {showDate}
                        </span>
                      )}
                      {message.isEdit && (
                        <span className="text-xs text-slate-400 italic">
                          (đã chỉnh sửa)
                        </span>
                      )}
                    </div>

                    <div className="text-sm text-foreground whitespace-pre-wrap break-words">
                      {message.content}
                    </div>

                    {message.files && message.files.length > 0 && (
                      <div className="mt-2 space-y-1">
                        {message.files.map((file) => (
                          <a
                            key={file.fileId}
                            href={file.fileUrl}
                            target="_blank"
                            rel="noopener noreferrer"
                            className="flex items-center gap-2 text-xs text-sky-600 dark:text-sky-400 hover:underline"
                          >
                            <Paperclip className="h-3 w-3" />
                            {file.fileName}
                          </a>
                        ))}
                      </div>
                    )}
                  </div>
                </div>
              );
            })
          )}
          <div ref={messagesEndRef} />
        </div>
      </ScrollArea>

      {/* Message Input */}
      <div className="border-t border-slate-200 bg-white p-4 dark:border-slate-700 dark:bg-slate-900">
        <form onSubmit={handleSendMessage} className="flex gap-2">
          <Input
            type="text"
            placeholder="Nhập tin nhắn..."
            value={messageContent}
            onChange={(e) => setMessageContent(e.target.value)}
            disabled={sending}
            className="flex-1"
          />
          <Button type="submit" disabled={sending || !messageContent.trim()}>
            {sending ? (
              <Loader2 className="h-4 w-4 animate-spin" />
            ) : (
              <Send className="h-4 w-4" />
            )}
          </Button>
        </form>
      </div>
    </div>
  );
}

