"use client";

import { useEffect, useState } from "react";
import { MessageCircle, ThumbsDown, ThumbsUp } from "lucide-react";
import { useNotification } from "@/src/components/providers/notification-provider";
import { useAuthState } from "@/src/hooks/use-auth-state";
import { ScrollArea } from "@/src/components/ui/scroll-area";
import { Button } from "@/src/components/ui/button";
import {
  getApiComment,
  getApiDocumentById,
  postApiComment,
  postApiDocumentReview,
} from "@/src/api/database/sdk.gen";
import type {
  CommentDto,
  CreateCommentCommand,
  CreateDocumentReviewCommand,
  DocumentDetailDto,
  PagedResponseOfCommentDto,
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
  const { success: notifySuccess, error: notifyError } = useNotification();

  const [comments, setComments] = useState<CommentDto[]>([]);
  const [page, setPage] = useState(1);
  const [hasMore, setHasMore] = useState(true);
  const [loading, setLoading] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [content, setContent] = useState("");
  const [localUseful, setLocalUseful] = useState(initialUsefulCount ?? 0);
  const [localNotUseful, setLocalNotUseful] = useState(initialNotUsefulCount ?? 0);

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

      const data = (res.data ?? res) as PagedResponseOfCommentDto;
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
      const body: CreateDocumentReviewCommand = {
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
      <div className="border-b border-slate-200 px-3 py-2 dark:border-slate-700 flex items-center justify-between">
        <div className="flex items-center gap-2 text-sm font-semibold text-foreground">
          <MessageCircle className="h-4 w-4" />
          <span>Bình luận & đánh giá</span>
        </div>
        <div className="flex items-center gap-3 text-xs text-slate-500 dark:text-slate-400">
          <button
            type="button"
            disabled={!ready || !authenticated}
            onClick={() => handleReview(DocumentReviewType.Useful)}
            className="inline-flex items-center gap-1  border border-slate-200 px-2 py-1 text-[11px] hover:bg-slate-50 disabled:opacity-50 dark:border-slate-700 dark:hover:bg-slate-800"
            title="Đánh dấu hữu ích"
          >
            <ThumbsUp className="h-3.5 w-3.5" />
            <span>{localUseful}</span>
          </button>
          <button
            type="button"
            disabled={!ready || !authenticated}
            onClick={() => handleReview(DocumentReviewType.NotUseful)}
            className="inline-flex items-center gap-1  border border-slate-200 px-2 py-1 text-[11px] hover:bg-slate-50 disabled:opacity-50 dark:border-slate-700 dark:hover:bg-slate-800"
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
            <p className="text-xs text-slate-500 dark:text-slate-400">
              Chưa có bình luận nào. Hãy là người đầu tiên bình luận.
            </p>
          ) : (
            <div className="space-y-3">
              {comments.map((c) => (
                <div
                  key={c.id}
                  className=" border border-slate-200 bg-slate-50 p-2.5 text-xs dark:border-slate-700 dark:bg-slate-900"
                >
                  <div className="flex items-center justify-between mb-1.5">
                    <div className="flex items-center gap-2 min-w-0">
                      {c.authorAvatarUrl && (
                        // eslint-disable-next-line @next/next/no-img-element
                        <img
                          src={c.authorAvatarUrl}
                          alt={c.authorName}
                          className="h-6 w-6 rounded-full object-cover"
                        />
                      )}
                      <div className="flex flex-col min-w-0">
                        <span className="text-xs font-semibold text-foreground truncate">
                          {c.authorName}
                        </span>
                        <span className="text-[10px] text-slate-400">
                          {new Date(c.createdAt as any).toLocaleString()}
                        </span>
                      </div>
                    </div>
                  </div>
                  <p className="text-[13px] text-slate-700 dark:text-slate-200 whitespace-pre-wrap break-words">
                    {c.content}
                  </p>
                </div>
              ))}

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

        <div className="border-t border-slate-200 p-2.5 dark:border-slate-700">
          {!ready ? (
            <p className="text-xs text-slate-500 dark:text-slate-400">
              Đang kiểm tra trạng thái đăng nhập...
            </p>
          ) : !authenticated ? (
            <p className="text-xs text-slate-500 dark:text-slate-400">
              Vui lòng đăng nhập để bình luận và đánh giá tài liệu.
            </p>
          ) : (
            <div className="space-y-2">
              <textarea
                value={content}
                onChange={(e) => setContent(e.target.value)}
                placeholder="Nhập bình luận của bạn..."
                rows={3}
                className="w-full resize-none  border border-slate-300 bg-white px-3 py-2 text-sm text-foreground shadow-sm outline-none focus:border-sky-500 focus:ring-1 focus:ring-sky-500 dark:border-slate-700 dark:bg-slate-900 dark:text-slate-100"
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
    </div>
  );
}



