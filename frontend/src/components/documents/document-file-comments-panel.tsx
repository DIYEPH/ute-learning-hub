"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { MessageCircle, ThumbsDown, ThumbsUp, MoreVertical, Flag } from "lucide-react";
import { useNotification } from "@/src/components/providers/notification-provider";
import { useAuthState } from "@/src/hooks/use-auth-state";
import { useUserProfile } from "@/src/hooks/use-user-profile";
import { ScrollArea } from "@/src/components/ui/scroll-area";
import { Button } from "@/src/components/ui/button";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/src/components/ui/dropdown-menu";
import { ReportModal } from "@/src/components/shared/report-modal";
import {
  getApiComment,
  getApiDocumentById,
  postApiComment,
  postApiDocumentReview,
} from "@/src/api/database/sdk.gen";
import type {
  CommentDetailDto,
  CreateCommentCommand,
  DocumentDetailDto,
  PagedResponseOfCommentDetailDto,
} from "@/src/api/database/types.gen";

interface DocumentFileCommentsPanelProps {
  documentId: string;
  documentFileId: string;
  initialUsefulCount?: number;
  initialNotUsefulCount?: number;
}

// Backend: DocumentReviewType { Useful = 0, NotUseful = 1 }
const DocumentReviewType = {
  Useful: 0,
  NotUseful: 1,
} as const;

const PAGE_SIZE = 20;

export function DocumentFileCommentsPanel({
  documentId,
  documentFileId,
  initialUsefulCount,
  initialNotUsefulCount,
}: DocumentFileCommentsPanelProps) {
  const { authenticated, ready } = useAuthState();
  const { profile } = useUserProfile();
  const { success: notifySuccess, error: notifyError } = useNotification();

  const [comments, setComments] = useState<CommentDetailDto[]>([]);
  const [page, setPage] = useState(1);
  const [hasMore, setHasMore] = useState(true);
  const [loading, setLoading] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [content, setContent] = useState("");
  const [localUseful, setLocalUseful] = useState(initialUsefulCount ?? 0);
  const [localNotUseful, setLocalNotUseful] = useState(initialNotUsefulCount ?? 0);

  // Report modal state
  const [reportModalOpen, setReportModalOpen] = useState(false);
  const [reportingComment, setReportingComment] = useState<CommentDetailDto | null>(null);

  useEffect(() => {
    setLocalUseful(initialUsefulCount ?? 0);
    setLocalNotUseful(initialNotUsefulCount ?? 0);
  }, [initialUsefulCount, initialNotUsefulCount, documentFileId]);

  useEffect(() => {
    if (!documentFileId) return;
    setComments([]);
    setPage(1);
    setHasMore(true);
    void loadComments(1, true);
  }, [documentFileId]);

  const loadComments = async (pageToLoad: number, replace = false) => {
    if (!documentFileId || loading) return;
    setLoading(true);
    try {
      const res = await getApiComment<true>({
        query: {
          DocumentFileId: documentFileId,
          Page: pageToLoad,
          PageSize: PAGE_SIZE,
        },
        throwOnError: true,
      });

      const data = (res.data ?? res) as PagedResponseOfCommentDetailDto;
      const items = data.items ?? [];

      setComments((prev) => (replace ? items : [...prev, ...items]));
      const total = data.totalCount ?? items.length;
      const loaded = (pageToLoad - 1) * PAGE_SIZE + items.length;
      setHasMore(loaded < total);
      setPage(pageToLoad);
    } catch (err: any) {
      const msg =
        err?.response?.data?.message ||
        err?.response?.data ||
        err?.message ||
        "Không thể tải bình luận";
      notifyError(msg);
    } finally {
      setLoading(false);
    }
  };

  const handleSubmit = async () => {
    if (!authenticated) {
      notifyError("Vui lòng đăng nhập để bình luận");
      return;
    }
    if (!content.trim()) return;

    setSubmitting(true);
    try {
      const body: CreateCommentCommand = {
        documentFileId,
        content: content.trim(),
        parentId: null,
      };

      await postApiComment<true>({
        body,
        throwOnError: true,
      });

      setContent("");
      notifySuccess("Đã gửi bình luận");
      // reload từ trang đầu
      await loadComments(1, true);
    } catch (err: any) {
      const msg =
        err?.response?.data?.message ||
        err?.response?.data ||
        err?.message ||
        "Không thể gửi bình luận";
      notifyError(msg);
    } finally {
      setSubmitting(false);
    }
  };

  const handleReview = async (type: number) => {
    if (!authenticated) {
      notifyError("Vui lòng đăng nhập để đánh giá");
      return;
    }
    try {
      const body = {
        documentFileId,
        documentReviewType: type,
      };
      await postApiDocumentReview<true>({
        body,
        throwOnError: true,
      });

      // Sau khi backend xử lý toggle, đọc lại thống kê từ DocumentDetailDto để đảm bảo đồng bộ
      try {
        const res = await getApiDocumentById({
          path: { id: documentId },
        });
        const payload = (res.data ?? res) as DocumentDetailDto | undefined;
        const target = payload?.files?.find((f) => f.id === documentFileId);
        if (target) {
          setLocalUseful(target.usefulCount ?? 0);
          setLocalNotUseful(target.notUsefulCount ?? 0);
        }
      } catch {
        // Nếu load thống kê fail thì bỏ qua, giữ giá trị hiện tại
      }

      notifySuccess(
        type === DocumentReviewType.Useful ? "Đã đánh dấu hữu ích" : "Đã đánh dấu không hữu ích",
      );
    } catch (err: any) {
      const msg =
        err?.response?.data?.message ||
        err?.response?.data ||
        err?.message ||
        "Không thể gửi đánh giá";
      notifyError(msg);
    }
  };

  return (
    <div className="flex h-full flex-col">
      <div className="border-b border-border px-3 py-2 flex items-center justify-between">
        <div className="flex items-center gap-2 text-sm font-semibold text-foreground">
          <MessageCircle className="h-4 w-4" />
          <span>Bình luận & đánh giá</span>
        </div>
        <div className="flex items-center gap-3 text-xs text-muted-foreground">
          <button
            type="button"
            disabled={!ready || !authenticated}
            onClick={() => handleReview(DocumentReviewType.Useful)}
            className="inline-flex items-center gap-1  border border-border px-2 py-1 text-[11px] hover:bg-muted disabled:opacity-50"
            title="Đánh dấu hữu ích"
          >
            <ThumbsUp className="h-3.5 w-3.5" />
            <span>{localUseful}</span>
          </button>
          <button
            type="button"
            disabled={!ready || !authenticated}
            onClick={() => handleReview(DocumentReviewType.NotUseful)}
            className="inline-flex items-center gap-1  border border-border px-2 py-1 text-[11px] hover:bg-muted disabled:opacity-50"
            title="Đánh dấu không hữu ích"
          >
            <ThumbsDown className="h-3.5 w-3.5" />
            <span>{localNotUseful}</span>
          </button>
        </div>
      </div>

      <div className="flex-1 flex flex-col">
        <ScrollArea className="flex-1 px-3 py-2">
          {comments.length === 0 && !loading ? (
            <p className="text-xs text-muted-foreground">
              Chưa có bình luận nào. Hãy là người đầu tiên bình luận.
            </p>
          ) : (
            <div className="space-y-3">
              {comments.map((c) => {
                const isCommentOwner = profile?.id === c.createdById;
                return (
                  <div
                    key={c.id}
                    className=" border border-border bg-muted p-2.5 text-xs"
                  >
                    <div className="flex items-center justify-between mb-1.5">
                      <div className="flex items-center gap-2 min-w-0">
                        <Link href={`/profile/${c.createdById}`} className="flex items-center gap-2 min-w-0 hover:opacity-80">
                          {c.authorAvatarUrl && (
                            // eslint-disable-next-line @next/next/no-img-element
                            <img
                              src={c.authorAvatarUrl}
                              alt={c.authorName}
                              className="h-6 w-6 rounded-full object-cover"
                            />
                          )}
                          <div className="flex flex-col min-w-0">
                            <span className="text-xs font-semibold text-foreground truncate hover:text-primary">
                              {c.authorName}
                            </span>
                            <span className="text-[10px] text-muted-foreground">
                              {new Date(c.createdAt as any).toLocaleString()}
                            </span>
                          </div>
                        </Link>
                      </div>
                      {/* Dropdown menu - only for non-owners */}
                      {authenticated && !isCommentOwner && (
                        <DropdownMenu>
                          <DropdownMenuTrigger asChild>
                            <Button variant="ghost" size="icon" className="h-6 w-6">
                              <MoreVertical className="h-3.5 w-3.5" />
                            </Button>
                          </DropdownMenuTrigger>
                          <DropdownMenuContent align="end">
                            <DropdownMenuItem
                              onClick={() => {
                                setReportingComment(c);
                                setReportModalOpen(true);
                              }}
                              className="text-red-600 focus:text-red-600"
                            >
                              <Flag className="h-4 w-4 mr-2" />
                              Báo cáo
                            </DropdownMenuItem>
                          </DropdownMenuContent>
                        </DropdownMenu>
                      )}
                    </div>
                    <p className="text-[13px] text-foreground whitespace-pre-wrap break-words">
                      {c.content}
                    </p>
                  </div>
                );
              })}

              {hasMore && (
                <div className="flex justify-center pt-1">
                  <Button
                    variant="ghost"
                    size="sm"
                    disabled={loading}
                    onClick={() => loadComments(page + 1)}
                  >
                    {loading ? "Đang tải..." : "Tải thêm"}
                  </Button>
                </div>
              )}
            </div>
          )}
        </ScrollArea>

        <div className="border-t border-border p-2.5">
          {!ready ? (
            <p className="text-xs text-muted-foreground">
              Đang kiểm tra trạng thái đăng nhập...
            </p>
          ) : !authenticated ? (
            <p className="text-xs text-muted-foreground">
              Vui lòng đăng nhập để bình luận và đánh giá tài liệu.
            </p>
          ) : (
            <div className="space-y-2">
              <textarea
                value={content}
                onChange={(e) => setContent(e.target.value)}
                placeholder="Nhập bình luận của bạn..."
                rows={3}
                className="w-full resize-none  border border-border bg-card px-3 py-2 text-sm text-foreground shadow-sm outline-none focus:border-primary focus:ring-1 focus:ring-primary"
              />
              <div className="flex justify-end">
                <Button
                  size="sm"
                  onClick={handleSubmit}
                  disabled={submitting || !content.trim()}
                >
                  {submitting ? "Đang gửi..." : "Gửi bình luận"}
                </Button>
              </div>
            </div>
          )}
        </div>
      </div>

      {/* Report Modal */}
      <ReportModal
        open={reportModalOpen}
        onOpenChange={setReportModalOpen}
        targetType="comment"
        targetId={reportingComment?.id || ""}
        targetTitle={reportingComment?.content?.substring(0, 50) || "Bình luận"}
      />
    </div>
  );
}



