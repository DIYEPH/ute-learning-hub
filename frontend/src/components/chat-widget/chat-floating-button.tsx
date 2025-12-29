"use client";

import { useState } from "react";
import { MessageCircle, X, Loader2, Search, Users, Sparkles } from "lucide-react";
import { useRouter } from "next/navigation";

import { getApiConversation, getApiConversationRecommendations } from "@/src/api/database/sdk.gen";
import type {
  ConversationDto,
  PagedResponseOfConversationDto,
  ConversationRecommendationDto,
} from "@/src/api/database/types.gen";
import { Button } from "@/src/components/ui/button";
import { Input } from "@/src/components/ui/input";
import { ScrollArea } from "@/src/components/ui/scroll-area";
import { Avatar, AvatarFallback, AvatarImage } from "@/src/components/ui/avatar";
import { useUserProfile } from "@/src/hooks/use-user-profile";
import { useChatWidget } from "./chat-widget-context";
import { cn } from "@/lib/utils";

export function ChatFloatingButton() {
  const router = useRouter();
  const { profile } = useUserProfile();
  const { openChat } = useChatWidget();

  const [isOpen, setIsOpen] = useState(false);
  const [loading, setLoading] = useState(false);
  const [conversations, setConversations] = useState<ConversationDto[]>([]);
  const [recommendations, setRecommendations] = useState<ConversationRecommendationDto[]>([]);
  const [searchTerm, setSearchTerm] = useState("");
  const [activeTab, setActiveTab] = useState<"chats" | "recommendations">("chats");

  const fetchConversations = async () => {
    if (!profile?.id) return;

    setLoading(true);
    try {
      const [convResponse, recResponse] = await Promise.all([
        getApiConversation({
          query: {
            Page: 1,
            PageSize: 15,
            SearchTerm: searchTerm || undefined,
            MemberId: profile.id,
          },
        }),
        getApiConversationRecommendations(),
      ]);

      const convPayload = (convResponse.data ?? convResponse) as PagedResponseOfConversationDto | undefined;
      const convItems = (convPayload?.items ?? []).filter((c) => c.isCurrentUserMember);
      setConversations(convItems);

      const recPayload = (recResponse.data ?? recResponse) as any;
      setRecommendations(recPayload?.recommendations ?? []);
    } catch (err) {
      console.error("Error fetching conversations:", err);
    } finally {
      setLoading(false);
    }
  };

  const handleToggle = () => {
    if (!isOpen) {
      void fetchConversations();
    }
    setIsOpen(!isOpen);
  };

  const handleSelectConversation = (conversation: ConversationDto) => {
    openChat(conversation);
    setIsOpen(false);
  };

  const handleViewAllChats = () => {
    router.push("/chat");
    setIsOpen(false);
  };

  const handleViewAllGroups = () => {
    router.push("/conversations");
    setIsOpen(false);
  };

  return (
    <>
      {/* Floating Button */}
      <button
        onClick={handleToggle}
        className={cn(
          "fixed bottom-4 right-4 z-50 flex h-14 w-14 items-center justify-center rounded-full shadow-lg transition-all",
          "bg-primary text-primary-foreground hover:bg-primary/90 hover:scale-105",
          "focus:outline-none focus:ring-2 focus:ring-primary focus:ring-offset-2",
          isOpen && "rotate-90"
        )}
      >
        {isOpen ? (
          <X className="h-6 w-6" />
        ) : (
          <MessageCircle className="h-6 w-6" />
        )}
      </button>

      {/* Chat List Popup */}
      {isOpen && (
        <div className="fixed bottom-20 right-4 z-50 w-80 md:w-96 bg-background border border-border rounded-lg shadow-2xl overflow-hidden">
          {/* Header */}
          <div className="p-3 border-b border-border bg-card">
            <div className="flex items-center justify-between mb-3">
              <h3 className="text-lg font-semibold">Tin nhắn</h3>
              <Button
                variant="ghost"
                size="sm"
                onClick={handleViewAllChats}
                className="text-xs text-primary"
              >
                Xem tất cả
              </Button>
            </div>

            {/* Search */}
            <div className="relative">
              <Search className="absolute left-2.5 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
              <Input
                type="text"
                placeholder="Tìm kiếm cuộc trò chuyện..."
                value={searchTerm}
                onChange={(e) => {
                  setSearchTerm(e.target.value);
                  void fetchConversations();
                }}
                className="pl-9 h-9"
              />
            </div>

            {/* Tabs */}
            <div className="flex gap-2 mt-3">
              <Button
                variant={activeTab === "chats" ? "default" : "ghost"}
                size="sm"
                className="flex-1 text-xs"
                onClick={() => setActiveTab("chats")}
              >
                <MessageCircle className="h-3.5 w-3.5 mr-1.5" />
                Chat của tôi
              </Button>
              <Button
                variant={activeTab === "recommendations" ? "default" : "ghost"}
                size="sm"
                className="flex-1 text-xs"
                onClick={() => setActiveTab("recommendations")}
              >
                <Sparkles className="h-3.5 w-3.5 mr-1.5" />
                Gợi ý (AI)
              </Button>
            </div>
          </div>

          {/* Content */}
          <ScrollArea className="h-80">
            {loading ? (
              <div className="flex items-center justify-center h-40">
                <Loader2 className="h-6 w-6 animate-spin text-muted-foreground" />
              </div>
            ) : activeTab === "chats" ? (
              conversations.length === 0 ? (
                <div className="flex flex-col items-center justify-center h-40 text-center text-muted-foreground px-4">
                  <MessageCircle className="h-10 w-10 mb-2 opacity-40" />
                  <p className="text-sm">Chưa có cuộc trò chuyện nào</p>
                  <Button
                    variant="link"
                    size="sm"
                    onClick={handleViewAllGroups}
                    className="mt-2 text-xs"
                  >
                    Khám phá nhóm học
                  </Button>
                </div>
              ) : (
                <div className="divide-y divide-border">
                  {conversations.map((conversation) => (
                    <button
                      key={conversation.id}
                      onClick={() => handleSelectConversation(conversation)}
                      className="w-full p-3 text-left hover:bg-muted transition-colors flex items-center gap-3"
                    >
                      <Avatar className="h-10 w-10 flex-shrink-0">
                        <AvatarImage
                          src={conversation.avatarUrl || undefined}
                          alt={conversation.conversationName || "Avatar"}
                        />
                        <AvatarFallback>
                          <MessageCircle className="h-5 w-5" />
                        </AvatarFallback>
                      </Avatar>
                      <div className="flex-1 min-w-0">
                        <h4 className="text-sm font-medium truncate">
                          {conversation.conversationName || "Cuộc trò chuyện"}
                        </h4>
                        <div className="flex items-center gap-2 text-xs text-muted-foreground">
                          <Users className="h-3 w-3" />
                          <span>{conversation.memberCount || 0} thành viên</span>
                        </div>
                      </div>
                      {(conversation.unreadCount ?? 0) > 0 && (
                        <span className="flex h-5 w-5 items-center justify-center rounded-full bg-primary text-[10px] font-bold text-primary-foreground">
                          {(conversation.unreadCount ?? 0) > 99 ? "99+" : conversation.unreadCount}
                        </span>
                      )}
                    </button>
                  ))}
                </div>
              )
            ) : recommendations.length === 0 ? (
              <div className="flex flex-col items-center justify-center h-40 text-center text-muted-foreground px-4">
                <Sparkles className="h-10 w-10 mb-2 opacity-40" />
                <p className="text-sm">Chưa có gợi ý nào</p>
                <p className="text-xs mt-1">Hãy hoàn thiện hồ sơ để nhận gợi ý phù hợp</p>
              </div>
            ) : (
              <div className="divide-y divide-border">
                {recommendations.slice(0, 10).map((rec) => (
                  <button
                    key={rec.conversationId}
                    onClick={() => router.push(`/conversations`)}
                    className="w-full p-3 text-left hover:bg-muted transition-colors flex items-center gap-3"
                  >
                    <Avatar className="h-10 w-10 flex-shrink-0">
                      <AvatarImage
                        src={rec.avatarUrl || undefined}
                        alt={rec.conversationName || "Avatar"}
                      />
                      <AvatarFallback>
                        <MessageCircle className="h-5 w-5" />
                      </AvatarFallback>
                    </Avatar>
                    <div className="flex-1 min-w-0">
                      <h4 className="text-sm font-medium truncate">
                        {rec.conversationName || "Nhóm học"}
                      </h4>
                      {rec.similarity !== undefined && (
                        <div className="flex items-center gap-1 text-xs text-primary">
                          <Sparkles className="h-3 w-3" />
                          <span>{Math.round(rec.similarity * 100)}% phù hợp</span>
                        </div>
                      )}
                    </div>
                  </button>
                ))}
              </div>
            )}
          </ScrollArea>

          {/* Footer */}
          <div className="p-2 border-t border-border bg-card">
            <Button
              variant="outline"
              size="sm"
              className="w-full text-xs"
              onClick={handleViewAllGroups}
            >
              <Users className="h-3.5 w-3.5 mr-1.5" />
              Khám phá tất cả nhóm học
            </Button>
          </div>
        </div>
      )}
    </>
  );
}
