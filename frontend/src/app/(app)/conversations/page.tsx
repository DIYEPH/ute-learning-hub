"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { Loader2, Search, Filter, X } from "lucide-react";
import { getApiConversation, getApiTag } from "@/src/api/database/sdk.gen";
import type {
  ConversationDto,
  PagedResponseOfConversationDto,
  TagDto,
} from "@/src/api/database/types.gen";
import { Button } from "@/src/components/ui/button";
import { Input } from "@/src/components/ui/input";
import { Label } from "@/src/components/ui/label";
import { ConversationCard } from "@/src/components/conversations/conversation-card";
import { useSubjects } from "@/src/hooks/use-subjects";
import type { SubjectDto2 } from "@/src/api/database/types.gen";

const PAGE_SIZE = 20;

type ConversationType = "all" | "0" | "1"; // 0 = Public (Group), 1 = Private

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
  const [conversationType, setConversationType] = useState<ConversationType>("all");

  // Options for filters
  const [subjects, setSubjects] = useState<SubjectDto2[]>([]);
  const [tags, setTags] = useState<TagDto[]>([]);

  useEffect(() => {
    void loadFilterOptions();
  }, []);

  useEffect(() => {
    setPage(1);
    setConversations([]);
    void fetchConversations(1, true);
  }, [searchTerm, subjectId, tagId, conversationType]);

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

  const fetchConversations = async (pageNum: number, reset: boolean = false) => {
    setLoading(true);
    setError(null);

    try {
      const query: any = {
        Page: pageNum,
        PageSize: PAGE_SIZE,
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

      if (conversationType !== "all") {
        query.ConversationType = parseInt(conversationType);
      }

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
    setConversationType("all");
  };

  const hasActiveFilters =
    searchTerm.trim() || subjectId || tagId || conversationType !== "all";

  return (
    <div className="container mx-auto p-4 md:p-6 max-w-7xl">
      <div className="mb-6">
        <h1 className="text-2xl font-semibold text-foreground mb-2">
          Khám phá cuộc trò chuyện
        </h1>
        <p className="text-sm text-slate-500 dark:text-slate-400">
          Tìm và tham gia các cuộc trò chuyện công khai hoặc xin tham gia các nhóm riêng tư
        </p>
      </div>

      {/* Filters */}
      <div className="mb-6 space-y-4">
        <div className="flex flex-col md:flex-row gap-4">
          {/* Search */}
          <div className="flex-1 relative">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-slate-400" />
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
              className="w-full rounded-md border border-input bg-background px-3 py-2 text-sm shadow-sm transition-colors focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-50"
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
              className="w-full rounded-md border border-input bg-background px-3 py-2 text-sm shadow-sm transition-colors focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-50"
            >
              <option value="all">Tất cả thẻ</option>
              {tags
                .filter((t): t is TagDto & { id: string } => !!t?.id)
                .map((tag) => (
                  <option key={tag.id} value={tag.id}>
                    {tag.tagName}
                  </option>
                ))}
            </select>
          </div>

          {/* Type Filter */}
          <div className="w-full md:w-[180px]">
            <select
              value={conversationType}
              onChange={(e) => setConversationType(e.target.value as ConversationType)}
              className="w-full rounded-md border border-input bg-background px-3 py-2 text-sm shadow-sm transition-colors focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-50"
            >
              <option value="all">Tất cả</option>
              <option value="0">Công khai</option>
              <option value="1">Riêng tư</option>
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
          <Loader2 className="h-6 w-6 animate-spin text-sky-500" />
        </div>
      )}

      {/* Conversations Grid */}
      {!loading && conversations.length === 0 && (
        <div className="text-center py-12 text-slate-500 dark:text-slate-400">
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

