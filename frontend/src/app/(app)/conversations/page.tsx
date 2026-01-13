"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { Loader2, Search, Filter, X, Sparkles, Mail, Check, Users } from "lucide-react";
import { getApiConversation, getApiTag, getApiConversationRecommendations, getApiConversationMyInvitations, postApiConversationInvitationsByInvitationIdRespond, getApiProposalMy, postApiProposalByConversationIdRespond } from "@/src/api";
import type {
  ConversationDto,
  PagedResponseOfConversationDto,
  TagDto,
  ConversationRecommendationDto,
  InvitationDto,
  ProposalDto,
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

  // Proposals (AI tự động gợi ý)
  const [proposals, setProposals] = useState<ProposalDto[]>([]);
  const [loadingProposals, setLoadingProposals] = useState(true);
  const [respondingProposal, setRespondingProposal] = useState<string | null>(null);

  useEffect(() => {
    void loadFilterOptions();
    void fetchRecommendations();
    void fetchInvitations();
    void fetchProposals();
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
        router.push(`/chat?id=${invitation.conversationId}`);
      }
    } catch (err) {
      console.error("Failed to respond to invitation:", err);
    } finally {
      setRespondingInvitation(null);
    }
  };

  const fetchProposals = async () => {
    setLoadingProposals(true);
    try {
      const response = await getApiProposalMy();
      const payload = (response.data ?? response) as any;
      // Filter pending and accepted proposals (1 = Pending, 2 = Accepted)
      const items = (payload?.proposals ?? []).filter((p: ProposalDto) => p.myStatus === 1 || p.myStatus === 2);
      setProposals(items);
    } catch (err) {
      console.error("Error loading proposals:", err);
      setProposals([]);
    } finally {
      setLoadingProposals(false);
    }
  };

  const respondToProposal = async (proposal: ProposalDto, accept: boolean) => {
    if (!proposal.conversationId) return;

    setRespondingProposal(proposal.conversationId);
    try {
      const response = await postApiProposalByConversationIdRespond({
        path: { conversationId: proposal.conversationId },
        body: { accept },
      });

      const result = (response.data ?? response) as any;

      if (accept && result.isActivated && result.conversation?.id) {
        // Group activated - navigate to chat
        setProposals((prev) => prev.filter((p) => p.conversationId !== proposal.conversationId));
        router.push(`/chat?id=${result.conversation.id}`);
      } else if (accept) {
        // Accepted but not activated yet - refresh to show updated count
        await fetchProposals();
      } else {
        // Declined - remove from list
        setProposals((prev) => prev.filter((p) => p.conversationId !== proposal.conversationId));
      }
    } catch (err) {
      console.error("Failed to respond to proposal:", err);
    } finally {
      setRespondingProposal(null);
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
                  <div className="h-10 w-10 rounded-full bg-accent/20 flex items-center justify-center shrink-0">
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

      {/* Gợi ý AI */}
      {!loadingProposals && proposals.length > 0 && (
        <div className="mb-8">
          <div className="flex items-center gap-2 mb-4">
            <h2 className="text-lg font-semibold text-foreground">AI gợi ý tạo nhóm mới</h2>
            <span className="bg-yellow-500 text-white text-xs px-2 py-0.5 rounded-full">
              {proposals.length}
            </span>
          </div>
          <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
            {proposals.map((proposal) => (
              <div
                key={proposal.conversationId}
                className="border rounded-lg p-4 bg-linear-to-br from-yellow-50 to-orange-50 dark:from-yellow-950/30 dark:to-orange-950/30 border-yellow-200 dark:border-yellow-800"
              >
                <div className="mb-3">
                  <h3 className="font-semibold text-foreground">
                    {proposal.conversationName}
                  </h3>
                  {proposal.subjectName && (
                    <p className="text-xs text-muted-foreground mt-1">
                      {proposal.subjectName}
                    </p>
                  )}
                  {proposal.tags && proposal.tags.length > 0 && (
                    <div className="flex flex-wrap gap-1 mt-2">
                      {proposal.tags.slice(0, 3).map((tag, idx) => (
                        <span
                          key={idx}
                          className="text-[10px] px-1.5 py-0.5 rounded bg-yellow-100 dark:bg-yellow-900/50 text-yellow-800 dark:text-yellow-200"
                        >
                          {tag}
                        </span>
                      ))}
                    </div>
                  )}
                </div>

                {/* Thành viên */}
                <div className="mb-3">
                  <p className="text-xs text-muted-foreground mb-2">
                    Thành viên: ({proposal.members?.length ?? 0}):
                  </p>
                  <div className="flex items-center gap-2">
                    <div className="flex -space-x-2">
                      {proposal.members?.slice(0, 4).map((member) => (
                        <div
                          key={member.userId}
                          className="w-7 h-7 rounded-full bg-primary/20 border-2 border-background flex items-center justify-center text-xs font-medium overflow-hidden"
                          title={member.fullName}
                        >
                          {member.avatarUrl ? (
                            <img src={member.avatarUrl} alt={member.fullName} className="w-full h-full object-cover" />
                          ) : (
                            member.fullName?.charAt(0) || "?"
                          )}
                        </div>
                      ))}
                      {(proposal.members?.length ?? 0) > 4 && (
                        <div className="w-7 h-7 rounded-full bg-muted border-2 border-background flex items-center justify-center text-[10px]">
                          +{(proposal.members?.length ?? 0) - 4}
                        </div>
                      )}
                    </div>
                  </div>
                </div>

                {/* Trạng thái */}
                <div className="flex items-center justify-between text-xs text-muted-foreground mb-3">
                  <span>
                    ✅ {proposal.acceptedCount ?? 0}/{proposal.totalMembers ?? 0} đã đồng ý
                  </span>
                  {proposal.mySimilarityScore && (
                    <span className="text-primary font-medium">
                      {Math.round((proposal.mySimilarityScore ?? 0) * 100)}% phù hợp
                    </span>
                  )}
                </div>

                {/* Hành động */}
                <div className="flex gap-2">
                  {proposal.myStatus === 2 ? (
                    <Button
                      size="sm"
                      className="flex-1"
                      variant="secondary"
                      disabled
                    >
                      <Check className="h-4 w-4 mr-1" />
                      Đã đồng ý - Chờ thêm người
                    </Button>
                  ) : (
                    <>
                      <Button
                        size="sm"
                        className="flex-1"
                        onClick={() => respondToProposal(proposal, true)}
                        disabled={respondingProposal === proposal.conversationId}
                      >
                        {respondingProposal === proposal.conversationId ? (
                          <Loader2 className="h-4 w-4 animate-spin" />
                        ) : (
                          <>
                            <Check className="h-4 w-4 mr-1" />
                            Tham gia
                          </>
                        )}
                      </Button>
                      <Button
                        size="sm"
                        variant="outline"
                        className="flex-1"
                        onClick={() => respondToProposal(proposal, false)}
                        disabled={respondingProposal === proposal.conversationId}
                      >
                        <X className="h-4 w-4 mr-1" />
                        Từ chối
                      </Button>
                    </>
                  )}
                </div>
              </div>
            ))}
          </div>
        </div>
      )}

      {/* Gợi ý cho bạn */}
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

      {/* Bộ lọc */}
      <div className="mb-6 space-y-4">
        <div className="flex flex-col md:flex-row gap-4">
          {/* Tìm kiếm */}
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

          {/* Môn học */}
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

          {/* Chủ đề */}
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

          {/* Xóa bộ lọc */}
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

      {/* Lỗi */}
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

      {/* Không tìm thấy */}
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


