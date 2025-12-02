"use client";

import { useEffect, useState } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import { Loader2, MessageCircle, Plus, Search } from "lucide-react";

import { getApiConversation } from "@/src/api/database/sdk.gen";
import type {
  ConversationDto,
  PagedResponseOfConversationDto,
} from "@/src/api/database/types.gen";
import { Button } from "@/src/components/ui/button";
import { Input } from "@/src/components/ui/input";
import { ConversationList } from "@/src/components/conversations/conversation-list";
import { ConversationDetail } from "@/src/components/conversations/conversation-detail";
import { CreateConversationModal } from "@/src/components/conversations/create-conversation-modal";
import { useUserProfile } from "@/src/hooks/use-user-profile";
import { cn } from "@/lib/utils";

const PAGE_SIZE = 20;

export default function ChatPage() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const selectedConversationId = searchParams.get("id");
  const { profile } = useUserProfile();

  const [conversations, setConversations] = useState<ConversationDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [searchTerm, setSearchTerm] = useState("");
  const [showCreateModal, setShowCreateModal] = useState(false);

  useEffect(() => {
    if (profile?.id) {
      void fetchConversations();
    }
  }, [searchTerm, profile?.id]);

  const fetchConversations = async () => {
    if (!profile?.id) return;

    setLoading(true);
    setError(null);

    try {
      const response = await getApiConversation({
        query: {
          Page: 1,
          PageSize: PAGE_SIZE,
          SearchTerm: searchTerm || undefined,
          MemberId: profile.id, // Filter by current user as member
        },
      });

      const payload = (response.data ??
        response) as PagedResponseOfConversationDto | undefined;
      const items = payload?.items ?? [];

      // Additional filter: only show conversations where user is a member
      const filteredItems = items.filter(
        (conv) => conv.isCurrentUserMember === true
      );

      setConversations(filteredItems);
    } catch (err: any) {
      const message =
        err?.response?.data?.message ||
        err?.message ||
        "Không thể tải danh sách cuộc trò chuyện";
      setError(message);
    } finally {
      setLoading(false);
    }
  };

  const handleSelectConversation = (conversationId: string) => {
    router.push(`/chat?id=${conversationId}`);
  };

  const handleCreateSuccess = () => {
    setShowCreateModal(false);
    void fetchConversations();
  };

  if (loading && conversations.length === 0) {
    return (
      <div className="flex h-screen items-center justify-center">
        <Loader2 className="h-6 w-6 animate-spin text-sky-500" />
      </div>
    );
  }

  return (
    <div className="flex h-screen overflow-hidden">
      {/* Sidebar: Danh sách conversations */}
      <div
        className={cn(
          "border-r border-slate-200 bg-white dark:border-slate-700 dark:bg-slate-900 flex flex-col transition-transform duration-300",
          "w-full md:w-80",
          selectedConversationId
            ? "hidden md:flex"
            : "flex"
        )}
      >
        {/* Header */}
        <div className="p-4 border-b border-slate-200 dark:border-slate-700">
          <div className="flex items-center justify-between mb-4">
            <h1 className="text-lg font-semibold text-foreground flex items-center gap-2">
              <MessageCircle className="h-5 w-5" />
              Trò chuyện
            </h1>
            <Button
              size="sm"
              onClick={() => setShowCreateModal(true)}
              className="h-8 w-8 p-0"
            >
              <Plus className="h-4 w-4" />
            </Button>
          </div>
          <div className="relative">
            <Search className="absolute left-2 top-1/2 -translate-y-1/2 h-4 w-4 text-slate-400" />
            <Input
              type="text"
              placeholder="Tìm kiếm cuộc trò chuyện..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="pl-8 h-9"
            />
          </div>
        </div>

        {/* Conversation List */}
        <div className="flex-1 overflow-y-auto">
          {error ? (
            <div className="p-4 text-sm text-red-600 dark:text-red-400">
              {error}
            </div>
          ) : conversations.length === 0 ? (
            <div className="p-4 text-center text-sm text-slate-500 dark:text-slate-400">
              <MessageCircle className="h-8 w-8 mx-auto mb-2 opacity-50" />
              <p>Chưa có cuộc trò chuyện nào</p>
            </div>
          ) : (
            <ConversationList
              conversations={conversations}
              selectedId={selectedConversationId || undefined}
              onSelect={handleSelectConversation}
            />
          )}
        </div>
      </div>

      {/* Main: Conversation Detail */}
      <div
        className={cn(
          "flex-1 flex flex-col",
          !selectedConversationId && "hidden md:flex"
        )}
      >
        {selectedConversationId ? (
          <ConversationDetail
            conversationId={selectedConversationId}
            onBack={() => router.push("/chat")}
          />
        ) : (
          <div className="flex h-full items-center justify-center text-slate-500 dark:text-slate-400">
            <div className="text-center">
              <MessageCircle className="h-12 w-12 mx-auto mb-3 opacity-50" />
              <p>Chọn một cuộc trò chuyện để bắt đầu</p>
            </div>
          </div>
        )}
      </div>

      {/* Create Conversation Modal */}
      <CreateConversationModal
        open={showCreateModal}
        onOpenChange={setShowCreateModal}
        onSuccess={handleCreateSuccess}
      />
    </div>
  );
}

