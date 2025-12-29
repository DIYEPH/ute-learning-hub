"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { Loader2, Search, Filter, X, Sparkles, Mail, Check, Users } from "lucide-react";
import { getApiConversation, getApiTag, getApiConversationRecommendations, getApiConversationMyInvitations, postApiConversationInvitationsByInvitationIdRespond } from "@/src/api/database/sdk.gen";
import type {
  ConversationDto,
  PagedResponseOfConversationDto,
  TagDto,
  ConversationRecommendationDto,
  InvitationDto,
} from "@/src/api/database/types.gen";
import { Button } from "@/src/components/ui/button";
import { Input } from "@/src/components/ui/input";
import { ConversationCard } from "@/src/components/conversations/conversation-card";
import { useSubjects } from "@/src/hooks/use-subjects";
import type { SubjectDto2 } from "@/src/api/database/types.gen";

const PAGE_SIZE = 20;

export default function ConversationsPage() {
  const router = useRouter();
  const { fetchSubjects } = useSubjects();

  const [conversations, setConversations] = useState<ConversationDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [hasMore, setHasMore] = useState(false);
  const [page, setPage] = useState(1);

  // Filters
  const [searchTerm, setSearchTerm] = useState("");
  const [subjectId, setSubjectId] = useState<string | null>(null);
  const [tagId, setTagId] = useState<string | null>(null);

  // Options for filters
  const [subjects, setSubjects] = useState<SubjectDto2[]>([]);
  const [tags, setTags] = useState<TagDto[]>([]);

  // Recommendations
  const [recommendations, setRecommendations] = useState<ConversationRecommendationDto[]>([]);
  const [loadingRecs, setLoadingRecs] = useState(true);

  // Invitations
  const [invitations, setInvitations] = useState<InvitationDto[]>([]);
  const [loadingInvitations, setLoadingInvitations] = useState(true);
  const [respondingInvitation, setRespondingInvitation] = useState<string | null>(null);

  useEffect(() => {
    void loadFilterOptions();
    void fetchRecommendations();
    void fetchInvitations();
  }, []);

  useEffect(() => {
    setPage(1);
    setConversations([]);
    void fetchConversations(1, true);
  }, [searchTerm, subjectId, tagId]);

  const loadFilterOptions = async () => {
    try {
      const [subjectsRes, tagsRes] = await Promise.all([
        fetchSubjects({ Page: 1, PageSize: 1000 }),
        getApiTag({ query: { Page: 1, PageSize: 1000 } }).then(
          (res: any) => res?.data || res
        ),
      ]);

      if (subjectsRes?.items) {
        setSubjects(subjectsRes.items);
      }
      if (tagsRes?.items) {
        setTags(tagsRes.items);
      }
    } catch (err) {
      console.error("Error loading filter options:", err);
    }
  };

  const fetchRecommendations = async () => {
    setLoadingRecs(true);
    try {
      const response = await getApiConversationRecommendations();
      const payload = (response.data ?? response) as any;
      const items = payload?.recommendations ?? [];
      setRecommendations(items);
    } catch (err) {
      console.error("Error loading recommendations:", err);
      setRecommendations([]);
    } finally {
      setLoadingRecs(false);
    }
  };

  const fetchInvitations = async () => {
    setLoadingInvitations(true);
    try {
      const response = await getApiConversationMyInvitations({
        query: { PageNumber: 1, PageSize: 10, PendingOnly: true },
      });
      const payload = (response.data ?? response) as any;
      const items = payload?.items ?? [];
      setInvitations(items);
    } catch (err) {
      console.error("Error loading invitations:", err);
      setInvitations([]);
    } finally {
      setLoadingInvitations(false);
    }
  };

  const respondToInvitation = async (invitation: InvitationDto, accept: boolean) => {
    if (!invitation.id) return;

    setRespondingInvitation(invitation.id);
    try {
      await postApiConversationInvitationsByInvitationIdRespond({
        path: { invitationId: invitation.id },
        body: { accept, note: null },
      });

      // Remove from local state
      setInvitations((prev) => prev.filter((i) => i.id !== invitation.id));

      if (accept) {
        router.push(`/conversations/${invitation.conversationId}`);
      }
    } catch (err) {
      console.error("Failed to respond to invitation:", err);
    } finally {
      setRespondingInvitation(null);
    }
  };

  const fetchConversations = async (pageNum: number, reset: boolean = false) => {
    setLoading(true);
    setError(null);

    try {
      const query: any = {
        Page: pageNum,
        PageSize: PAGE_SIZE,

        ConversationType: 1, // 1 = Group
      };

      if (searchTerm.trim()) {
        query.SearchTerm = searchTerm.trim();
      }

      if (subjectId) {
        query.SubjectId = subjectId;
      }

      if (tagId) {
        query.TagId = tagId;
      }

      // conversationType filter removed - always Group

      const response = await getApiConversation({ query });

      const payload = (response.data ??
        response) as PagedResponseOfConversationDto | undefined;
      const items = payload?.items ?? [];

      if (reset) {
        setConversations(items);
      } else {
        setConversations((prev) => [...prev, ...items]);
      }

      setHasMore((payload?.totalCount ?? 0) > pageNum * PAGE_SIZE);
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

  const handleLoadMore = () => {
    const nextPage = page + 1;
    setPage(nextPage);
    void fetchConversations(nextPage, false);
  };

  const handleJoinSuccess = () => {
    // Refresh conversations to update join status
    void fetchConversations(page, true);
  };

  const clearFilters = () => {
    setSearchTerm("");
    setSubjectId(null);
    setTagId(null);
  };

  const hasActiveFilters =
    searchTerm.trim() || subjectId || tagId;

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold text-foreground">
          Khám phá cuộc trò chuyện
        </h1>
        <p className="text-sm text-muted-foreground mt-1">
          Tìm và tham gia các cuộc trò chuyện công khai hoặc xin tham gia các nhóm riêng tư
        </p>
      </div>

      {/* Pending Invitations Section */}
      {!loadingInvitations && invitations.length > 0 && (
        <div className="mb-8">
          <div className="flex items-center gap-2 mb-4">
            <Mail className="h-5 w-5 text-accent" />
            <h2 className="text-lg font-semibold text-foreground">Lời mời tham gia nhóm</h2>
            <span className="bg-accent text-accent-foreground text-xs px-2 py-0.5 rounded-full">
              {invitations.length}
            </span>
          </div>
          <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
            {invitations.map((invitation) => (
              <div
                key={invitation.id}
                className="border rounded-lg p-4 bg-accent/10 border-accent/30"
              >
                <div className="flex items-start gap-3">
                  <div className="h-10 w-10 rounded-full bg-accent/20 flex items-center justify-center flex-shrink-0">
                    <Users className="h-5 w-5 text-accent" />
                  </div>
                  <div className="flex-1 min-w-0">
                    <h3 className="font-semibold text-foreground truncate">
                      {invitation.conversationName}
                    </h3>
                    <p className="text-sm text-muted-foreground">
                      {invitation.invitedByName} đã mời bạn
                    </p>
                    <p className="text-xs text-muted-foreground mt-1">
                      {invitation.memberCount} thành viên
                    </p>
                    {invitation.message && (
                      <p className="text-sm text-muted-foreground/80 mt-2 italic">
                        "{invitation.message}"
                      </p>
                    )}
                  </div>
                </div>
                <div className="flex gap-2 mt-4">
                  <Button
                    size="sm"
                    className="flex-1"
                    onClick={() => respondToInvitation(invitation, true)}
                    disabled={respondingInvitation === invitation.id}
                  >
                    {respondingInvitation === invitation.id ? (
                      <Loader2 className="h-4 w-4 animate-spin" />
                    ) : (
                      <>
                        <Check className="h-4 w-4 mr-1" />
                        Chấp nhận
                      </>
                    )}
                  </Button>
                  <Button
                    size="sm"
                    variant="outline"
                    className="flex-1"
                    onClick={() => respondToInvitation(invitation, false)}
                    disabled={respondingInvitation === invitation.id}
                  >
                    <X className="h-4 w-4 mr-1" />
                    Từ chối
                  </Button>
                </div>
              </div>
            ))}
          </div>
        </div>
      )}

      {/* Recommendations Section */}
      {!loadingRecs && recommendations.length > 0 && (
        <div className="mb-8">
          <div className="flex items-center gap-2 mb-4">
            <Sparkles className="h-5 w-5 text-primary" />
            <h2 className="text-lg font-semibold text-foreground">Gợi ý cho bạn</h2>
          </div>
          <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
            {recommendations.map((rec) => (
              <ConversationCard
                key={rec.conversationId}
                conversation={{
                  id: rec.conversationId,
                  conversationName: rec.conversationName,
                  subject: rec.subject,
                  tags: rec.tags,
                  avatarUrl: rec.avatarUrl,
                  memberCount: rec.memberCount,
                  isCurrentUserMember: rec.isCurrentUserMember,
                  hasPendingJoinRequest: rec.hasPendingJoinRequest,
                }}
                similarity={rec.similarity}
                onJoinSuccess={() => {
                  fetchRecommendations();
                  fetchConversations(page, true);
                }}
              />
            ))}
          </div>
        </div>
      )}
      {loadingRecs && (
        <div className="mb-8 flex items-center gap-2 text-sm text-muted-foreground">
          <Loader2 className="h-4 w-4 animate-spin" />
          <span>Đang tải gợi ý...</span>
        </div>
      )}

      {/* Filters */}
      <div className="mb-6 space-y-4">
        <div className="flex flex-col md:flex-row gap-4">
          {/* Search */}
          <div className="flex-1 relative">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
            <Input
              type="text"
              placeholder="Tìm kiếm cuộc trò chuyện..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="pl-9"
            />
          </div>

          {/* Subject Filter */}
          <div className="w-full md:w-[200px]">
            <select
              value={subjectId || "all"}
              onChange={(e) => setSubjectId(e.target.value === "all" ? null : e.target.value)}
              className="w-full  border border-input bg-background px-3 py-2 text-sm shadow-sm transition-colors focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-50"
            >
              <option value="all">Tất cả môn học</option>
              {subjects
                .filter((s): s is SubjectDto2 & { id: string } => !!s?.id)
                .map((subject) => (
                  <option key={subject.id} value={subject.id}>
                    {subject.subjectName}
                  </option>
                ))}
            </select>
          </div>

          {/* Tag Filter */}
          <div className="w-full md:w-[200px]">
            <select
              value={tagId || "all"}
              onChange={(e) => setTagId(e.target.value === "all" ? null : e.target.value)}
              className="w-full  border border-input bg-background px-3 py-2 text-sm shadow-sm transition-colors focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-50"
            >
              <option value="all">Tất cả chủ đề</option>
              {tags
                .filter((t): t is TagDto & { id: string } => !!t?.id)
                .map((tag) => (
                  <option key={tag.id} value={tag.id}>
                    {tag.tagName}
                  </option>
                ))}
            </select>
          </div>

          {/* Clear Filters */}
          {hasActiveFilters && (
            <Button
              variant="outline"
              size="sm"
              onClick={clearFilters}
              className="w-full md:w-auto"
            >
              <X className="h-4 w-4 mr-2" />
              Xóa bộ lọc
            </Button>
          )}
        </div>
      </div>

      {/* Error */}
      {error && (
        <div className="mb-4 p-3 text-sm text-red-600 dark:text-red-400 bg-red-50 dark:bg-red-950 rounded">
          {error}
        </div>
      )}

      {/* Loading */}
      {loading && conversations.length === 0 && (
        <div className="flex items-center justify-center py-12">
          <Loader2 className="h-6 w-6 animate-spin text-primary" />
        </div>
      )}

      {/* Conversations Grid */}
      {!loading && conversations.length === 0 && (
        <div className="text-center py-12 text-muted-foreground">
          <p>Không tìm thấy cuộc trò chuyện nào</p>
        </div>
      )}

      {conversations.length > 0 && (
        <>
          <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
            {conversations.map((conversation) => (
              <ConversationCard
                key={conversation.id}
                conversation={conversation}
                onJoinSuccess={handleJoinSuccess}
              />
            ))}
          </div>

          {/* Load More */}
          {hasMore && (
            <div className="mt-6 text-center">
              <Button
                variant="outline"
                onClick={handleLoadMore}
                disabled={loading}
              >
                {loading ? (
                  <>
                    <Loader2 className="h-4 w-4 mr-2 animate-spin" />
                    Đang tải...
                  </>
                ) : (
                  "Tải thêm"
                )}
              </Button>
            </div>
          )}
        </>
      )}
    </div>
  );
}


