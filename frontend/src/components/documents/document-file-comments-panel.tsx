"use client";

import { useEffect, useState } from "react";
import { MessageCircle, Send } from "lucide-react";
import { useNotification } from "@/src/components/providers/notification-provider";
import { useAuthState } from "@/src/hooks/use-auth-state";
import { useUserProfile } from "@/src/hooks/use-user-profile";
import { ScrollArea } from "@/src/components/ui/scroll-area";
import { Button } from "@/src/components/ui/button";
import { Avatar, AvatarImage, AvatarFallback } from "@/src/components/ui/avatar";
import { ReportModal } from "@/src/components/shared/report-modal";
import { CommentItem } from "@/src/components/documents/comment-item";
import { getErrorMessage } from "@/src/lib/error-utils";
import { containsProfanity, getProfanityErrorMessage } from "@/src/lib/profanity-filter";
import { getApiComment, postApiComment } from "@/src/api";
import type { CommentDetailDto, PagedResponseOfCommentDetailDto } from "@/src/api/database/types.gen";

interface DocumentFileCommentsPanelProps {
  documentId: string;
  documentFileId: string;
}

const PAGE_SIZE = 20;

export function DocumentFileCommentsPanel({ documentFileId }: DocumentFileCommentsPanelProps) {
  const { authenticated, ready } = useAuthState();
  const { profile } = useUserProfile();
  const { success: notifySuccess, error: notifyError } = useNotification();

  const [comments, setComments] = useState<CommentDetailDto[]>([]);
  const [page, setPage] = useState(1);
  const [hasMore, setHasMore] = useState(true);
  const [loading, setLoading] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [content, setContent] = useState("");
  const [replyingTo, setReplyingTo] = useState<string | null>(null);
  const [replyContent, setReplyContent] = useState("");
  const [submittingReply, setSubmittingReply] = useState(false);
  const [reportModalOpen, setReportModalOpen] = useState(false);
  const [reportingComment, setReportingComment] = useState<CommentDetailDto | null>(null);

  useEffect(() => {
    if (!documentFileId) return;
    setComments([]); setPage(1); setHasMore(true);
    void loadComments(1, true);
  }, [documentFileId]);

  // Load comments
  const loadComments = async (pageToLoad: number, replace = false) => {
    if (!documentFileId || loading) return;
    setLoading(true);
    try {
      const res = await getApiComment<true>({
        query: { DocumentFileId: documentFileId, Page: pageToLoad, PageSize: PAGE_SIZE },
        throwOnError: true
      });
      const data = (res.data ?? res) as PagedResponseOfCommentDetailDto;
      const items = data.items ?? [];
      setComments(prev => replace ? items : [...prev, ...items]);
      const total = data.totalCount ?? items.length;
      setHasMore((pageToLoad - 1) * PAGE_SIZE + items.length < total);
      setPage(pageToLoad);
    } catch (err: unknown) {
      notifyError(getErrorMessage(err, "Không thể tải bình luận"));
    } finally { setLoading(false); }
  };

  // Submit comment
  const handleSubmit = async () => {
    if (!authenticated) { notifyError("Vui lòng đăng nhập để bình luận"); return; }
    if (!content.trim()) return;
    if (containsProfanity(content)) { notifyError(getProfanityErrorMessage()); return; }
    setSubmitting(true);
    try {
      await postApiComment<true>({
        body: { documentFileId, content: content.trim(), parentId: null },
        throwOnError: true
      });
      setContent("");
      notifySuccess("Đã gửi bình luận");
      await loadComments(1, true);
    } catch (err: unknown) {
      notifyError(getErrorMessage(err, "Không thể gửi bình luận"));
    } finally { setSubmitting(false); }
  };

  // Submit reply
  const handleSubmitReply = async (parentId: string) => {
    if (!authenticated || !replyContent.trim()) return;
    if (containsProfanity(replyContent)) { notifyError(getProfanityErrorMessage()); return; }
    setSubmittingReply(true);
    try {
      await postApiComment<true>({
        body: { documentFileId, content: replyContent.trim(), parentId },
        throwOnError: true
      });
      setReplyContent("");
      setReplyingTo(null);
      notifySuccess("Đã gửi phản hồi");
      await loadComments(1, true);
    } catch (err: unknown) {
      notifyError(getErrorMessage(err, "Không thể gửi phản hồi"));
    } finally { setSubmittingReply(false); }
  };

  const handleReport = (c: CommentDetailDto) => { setReportingComment(c); setReportModalOpen(true); };
  const handleReply = (parentId: string) => { setReplyingTo(parentId); setReplyContent(""); };

  return (
    <div className="flex h-full flex-col bg-background">
      <div className="flex-1 flex flex-col overflow-hidden">
        <ScrollArea className="flex-1 px-4 py-3">
          {comments.length === 0 && !loading ? (
            <div className="text-center py-8">
              <MessageCircle className="h-10 w-10 text-muted-foreground/30 mx-auto mb-2" />
              <p className="text-sm text-muted-foreground">Chưa có bình luận nào.</p>
              <p className="text-xs text-muted-foreground mt-1">Hãy là người đầu tiên bình luận!</p>
            </div>
          ) : (
            <div className="space-y-4">
              {comments.map(c => (
                <div key={c.id}>
                  <CommentItem
                    comment={c}
                    isOwner={profile?.id === c.createdById}
                    authenticated={authenticated}
                    onReport={handleReport}
                    onReply={handleReply}
                    documentFileId={documentFileId}
                    profileId={profile?.id}
                  />
                  {replyingTo === c.id && (
                    <div className="flex gap-2 mt-2 ml-10">
                      <Avatar className="h-6 w-6">
                        <AvatarImage src={profile?.avatarUrl || undefined} alt={profile?.fullName} />
                        <AvatarFallback className="bg-primary/10 text-primary text-[10px]">
                          {profile?.fullName?.charAt(0)?.toUpperCase() || "?"}
                        </AvatarFallback>
                      </Avatar>
                      <div className="flex-1 flex gap-2">
                        <input
                          type="text"
                          value={replyContent}
                          onChange={e => setReplyContent(e.target.value)}
                          placeholder="Viết phản hồi..."
                          className="flex-1 rounded-full bg-muted px-3 py-1.5 text-xs text-foreground placeholder:text-muted-foreground outline-none focus:ring-1 focus:ring-primary"
                          onKeyDown={e => {
                            if (e.key === "Enter" && !e.shiftKey) { e.preventDefault(); handleSubmitReply(c.id!); }
                            if (e.key === "Escape") setReplyingTo(null);
                          }}
                          autoFocus
                        />
                        <Button
                          size="sm"
                          variant="ghost"
                          className="h-7 w-7 p-0 rounded-full"
                          onClick={() => handleSubmitReply(c.id!)}
                          disabled={submittingReply || !replyContent.trim()}
                        >
                          <Send className="h-3.5 w-3.5" />
                        </Button>
                      </div>
                    </div>
                  )}
                </div>
              ))}
              {hasMore && (
                <div className="flex justify-center pt-2">
                  <Button variant="ghost" size="sm" disabled={loading} onClick={() => loadComments(page + 1)} className="text-xs">
                    {loading ? "Đang tải..." : "Xem thêm bình luận"}
                  </Button>
                </div>
              )}
            </div>
          )}
        </ScrollArea>
        <div className="border-t border-border p-3">
          {!ready ? (
            <p className="text-xs text-muted-foreground text-center">Đang kiểm tra đăng nhập...</p>
          ) : !authenticated ? (
            <p className="text-xs text-muted-foreground text-center">Vui lòng đăng nhập để bình luận.</p>
          ) : (
            <div className="flex gap-2 items-start">
              <Avatar className="h-8 w-8">
                <AvatarImage src={profile?.avatarUrl || undefined} alt={profile?.fullName} />
                <AvatarFallback className="bg-primary/10 text-primary text-xs">
                  {profile?.fullName?.charAt(0)?.toUpperCase() || "?"}
                </AvatarFallback>
              </Avatar>
              <div className="flex-1 flex gap-2">
                <textarea
                  value={content}
                  onChange={e => setContent(e.target.value)}
                  placeholder="Viết bình luận..."
                  rows={1}
                  className="flex-1 resize-none rounded-2xl bg-muted px-4 py-2 text-sm text-foreground placeholder:text-muted-foreground outline-none focus:ring-1 focus:ring-primary min-h-[36px] max-h-[120px]"
                  onInput={e => {
                    const target = e.target as HTMLTextAreaElement;
                    target.style.height = "auto";
                    target.style.height = Math.min(target.scrollHeight, 120) + "px";
                  }}
                  onKeyDown={e => {
                    if (e.key === "Enter" && !e.shiftKey) { e.preventDefault(); handleSubmit(); }
                  }}
                />
                <Button
                  size="icon"
                  variant="ghost"
                  className="h-9 w-9 rounded-full text-primary hover:bg-primary/10"
                  onClick={handleSubmit}
                  disabled={submitting || !content.trim()}
                >
                  <Send className="h-4 w-4" />
                </Button>
              </div>
            </div>
          )}
        </div>
      </div>
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