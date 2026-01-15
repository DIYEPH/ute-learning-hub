"use client";

import { useEffect, useRef, useState, useCallback } from "react";
import { useParams, useRouter } from "next/navigation";
import { ArrowLeft, FileText, MessageSquare, X, ThumbsUp, ThumbsDown } from "lucide-react";
import {
  getApiDocumentById,
  getApiDocumentFilesByFileId,
  postApiDocumentFilesByFileIdView,
  postApiDocumentReview,
} from "@/src/api";
import type { DocumentDetailDto, DocumentFileDto } from "@/src/api/database/types.gen";
import { Button } from "@/src/components/ui/button";
import { Badge } from "@/src/components/ui/badge";
import { DocumentFileCommentsPanel } from "@/src/components/documents/document-file-comments-panel";
import { PdfViewer } from "@/src/components/documents/pdf-viewer";
import { useNotification } from "@/src/components/providers/notification-provider";
import { useAuthState } from "@/src/hooks/use-auth-state";
import { getFileUrlById } from "@/src/lib/file-url";
import { cn } from "@/lib/utils";

const DocumentReviewType = { Useful: 0, NotUseful: 1 } as const;

export default function DocumentFileDetailPage() {
  const { id: documentId, fileId: documentFileId } = useParams<{ id: string; fileId: string }>();
  const router = useRouter();
  const viewCountedRef = useRef(false);
  const { authenticated, ready } = useAuthState();
  const { success: notifySuccess, error: notifyError } = useNotification();

  const [doc, setDoc] = useState<DocumentDetailDto | null>(null);
  const [file, setFile] = useState<DocumentFileDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [showComments, setShowComments] = useState(() => {
    if (typeof window !== 'undefined') return window.innerWidth >= 1024;
    return false;
  });

  const [localUseful, setLocalUseful] = useState(0);
  const [localNotUseful, setLocalNotUseful] = useState(0);
  const [myReviewType, setMyReviewType] = useState<number | null>(null);
  const [reviewLoading, setReviewLoading] = useState<number | null>(null);

  const loadData = async () => {
    if (!documentId || !documentFileId) return;

    setLoading(true);
    try {
      const [docRes, fileRes] = await Promise.all([
        getApiDocumentById({ path: { id: documentId } }),
        getApiDocumentFilesByFileId({ path: { fileId: documentFileId } }),
      ]);

      const docPayload = (docRes.data ?? docRes) as DocumentDetailDto;
      const filePayload = (fileRes.data ?? fileRes) as DocumentFileDto;

      setDoc(docPayload);
      setFile(filePayload);
      setLocalUseful(filePayload?.usefulCount ?? 0);
      setLocalNotUseful(filePayload?.notUsefulCount ?? 0);
      setMyReviewType(filePayload?.myReviewType ?? null);

      if (filePayload && !viewCountedRef.current) {
        viewCountedRef.current = true;
        void postApiDocumentFilesByFileIdView({ path: { fileId: documentFileId } });
      }
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { void loadData(); }, [documentId, documentFileId]);

  const handleReview = useCallback(async (type: number) => {
    if (!authenticated || !documentFileId) {
      notifyError("Vui lòng đăng nhập để đánh giá");
      return;
    }
    if (reviewLoading !== null) return;

    setReviewLoading(type);
    const prevUseful = localUseful;
    const prevNotUseful = localNotUseful;
    const prevMyReview = myReviewType;

    if (myReviewType === type) {
      if (type === DocumentReviewType.Useful) setLocalUseful(prev => Math.max(0, prev - 1));
      else setLocalNotUseful(prev => Math.max(0, prev - 1));
      setMyReviewType(null);
    } else if (myReviewType !== null) {
      if (myReviewType === DocumentReviewType.Useful) {
        setLocalUseful(prev => Math.max(0, prev - 1));
        setLocalNotUseful(prev => prev + 1);
      } else {
        setLocalNotUseful(prev => Math.max(0, prev - 1));
        setLocalUseful(prev => prev + 1);
      }
      setMyReviewType(type);
    } else {
      if (type === DocumentReviewType.Useful) setLocalUseful(prev => prev + 1);
      else setLocalNotUseful(prev => prev + 1);
      setMyReviewType(type);
    }

    try {
      await postApiDocumentReview<true>({
        body: { documentFileId, documentReviewType: type },
        throwOnError: true
      });
      notifySuccess(
        prevMyReview === type
          ? "Đã bỏ đánh giá"
          : (type === DocumentReviewType.Useful ? "Đã đánh dấu hữu ích" : "Đã đánh dấu không hữu ích")
      );
    } catch {
      setLocalUseful(prevUseful);
      setLocalNotUseful(prevNotUseful);
      setMyReviewType(prevMyReview);
      notifyError("Không thể gửi đánh giá");
    } finally {
      setReviewLoading(null);
    }
  }, [authenticated, documentFileId, localUseful, localNotUseful, myReviewType, reviewLoading, notifyError, notifySuccess]);

  if (!documentId || !documentFileId)
    return <p className="text-sm text-red-600">Thiếu mã tài liệu hoặc file</p>;

  if (loading && !doc) {
    return (
      <div className="flex justify-center py-12">
        <FileText className="h-6 w-6 animate-pulse text-primary" />
      </div>
    );
  }

  if (!doc || !file) return null;

  const fileUrl = getFileUrlById(file.fileId);
  const fileSize = file.fileSize ? `${(file.fileSize / 1024 / 1024).toFixed(2)} MB` : "";

  return (
    <div className="flex h-full flex-col bg-secondary">
      <div className="flex items-center justify-between px-2 py-1.5 border-b bg-card">
        <div className="flex items-center gap-2 min-w-0">
          <Button variant="ghost" size="icon" onClick={() => router.back()}>
            <ArrowLeft className="h-4 w-4" />
          </Button>
          <div className="hidden sm:block min-w-0">
            <p className="text-[10px] truncate">{doc.documentName}</p>
            <h1 className="text-xs font-medium truncate max-w-[300px]">
              {file.title || "Chương / file"}
            </h1>
          </div>
        </div>

        <div className="flex items-center gap-1.5">
          {doc.subject?.subjectName && (
            <Badge variant="outline" className="text-[10px] hidden md:inline-flex">
              {doc.subject.subjectName}
            </Badge>
          )}
          <span className="text-[10px] hidden lg:inline">
            {fileSize}{file.totalPages && ` • ${file.totalPages} trang`}
          </span>
          <Button
            size="icon"
            variant={showComments ? "secondary" : "ghost"}
            onClick={() => setShowComments(v => !v)}
          >
            <MessageSquare className="h-4 w-4" />
          </Button>
        </div>
      </div>

      <div className="flex flex-1 overflow-hidden">
        <div className={cn("flex-1 bg-muted transition-all", showComments && "lg:mr-0")}>
          {fileUrl ? (
            file.mimeType === "application/pdf" ? (
              <PdfViewer
                fileUrl={fileUrl}
                fileId={file.id!}
                documentId={documentId}
                title={file.title || undefined}
                className="h-full w-full"
              />
            ) : file.mimeType?.startsWith("image/") ? (
              <div className="h-full flex items-center justify-center p-4">
                <img src={fileUrl} className="max-w-full max-h-full object-contain" />
              </div>
            ) : (
              <iframe src={fileUrl} className="h-full w-full border-0" />
            )
          ) : (
            <div className="flex h-full flex-col items-center justify-center text-sm">
              <FileText className="h-10 w-10 opacity-40" />
              <p>Không có URL để xem file</p>
            </div>
          )}
        </div>

        {showComments && (
          <>
            <div
              className="fixed inset-0 bg-black/50 z-40 lg:hidden"
              onClick={() => setShowComments(false)}
            />

            <div
              className={cn(
                "fixed right-0 top-0 bottom-0 z-50 w-[85vw] max-w-[320px]",
                "lg:relative lg:w-[300px]",
                "border-l bg-card flex flex-col"
              )}
            >
              <div className="flex items-center justify-between px-3 py-2 border-b">
                <div className="flex items-center gap-2">
                  <h2 className="text-sm font-semibold">Bình luận</h2>
                  <button
                    type="button"
                    disabled={!ready || !authenticated || reviewLoading !== null}
                    onClick={() => handleReview(DocumentReviewType.Useful)}
                    className={`inline-flex items-center gap-1 rounded-full px-2 py-1 text-[10px] font-medium transition-all
                      ${reviewLoading === DocumentReviewType.Useful ? "opacity-70" : ""}
                      ${myReviewType === DocumentReviewType.Useful
                        ? "bg-green-500 text-white"
                        : "bg-green-50 text-green-700 hover:bg-green-100 dark:bg-green-900/20 dark:text-green-400"}
                      disabled:opacity-50`}
                  >
                    <ThumbsUp className="h-3 w-3" />
                    <span>{localUseful}</span>
                  </button>
                  <button
                    type="button"
                    disabled={!ready || !authenticated || reviewLoading !== null}
                    onClick={() => handleReview(DocumentReviewType.NotUseful)}
                    className={`inline-flex items-center gap-1 rounded-full px-2 py-1 text-[10px] font-medium transition-all
                      ${reviewLoading === DocumentReviewType.NotUseful ? "opacity-70" : ""}
                      ${myReviewType === DocumentReviewType.NotUseful
                        ? "bg-red-500 text-white"
                        : "bg-red-50 text-red-700 hover:bg-red-100 dark:bg-red-900/20 dark:text-red-400"}
                      disabled:opacity-50`}
                  >
                    <ThumbsDown className="h-3 w-3" />
                    <span>{localNotUseful}</span>
                  </button>
                </div>
                <Button size="icon" variant="ghost" onClick={() => setShowComments(false)}>
                  <X className="h-4 w-4" />
                </Button>
              </div>

              <div className="flex-1 overflow-hidden">
                <DocumentFileCommentsPanel
                  documentId={documentId}
                  documentFileId={documentFileId}
                />
              </div>
            </div>
          </>
        )}
      </div>
    </div>
  );
}