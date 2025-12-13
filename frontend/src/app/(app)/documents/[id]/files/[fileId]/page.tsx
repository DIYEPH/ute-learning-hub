"use client";

import { useEffect, useState } from "react";
import { useParams, useRouter } from "next/navigation";
import { ArrowLeft, FileText, MessageSquare, X } from "lucide-react";

import { getApiDocumentById } from "@/src/api/database/sdk.gen";
import type { DocumentDetailDto, DocumentFileDto } from "@/src/api/database/types.gen";
import { Button } from "@/src/components/ui/button";
import { Badge } from "@/src/components/ui/badge";
import { DocumentFileCommentsPanel } from "@/src/components/documents/document-file-comments-panel";
import { PdfViewer } from "@/src/components/documents/pdf-viewer";
import { getFileUrlById } from "@/src/lib/file-url";
import { cn } from "@/lib/utils";

export default function DocumentFileDetailPage() {
  const params = useParams<{ id: string; fileId: string }>();
  const router = useRouter();
  const documentId = params?.id;
  const documentFileId = params?.fileId;

  const [doc, setDoc] = useState<DocumentDetailDto | null>(null);
  const [file, setFile] = useState<DocumentFileDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [showComments, setShowComments] = useState(false);

  useEffect(() => {
    if (!documentId || !documentFileId) return;
    let cancelled = false;

    const load = async () => {
      setLoading(true);
      setError(null);
      try {
        const res = await getApiDocumentById({
          path: { id: documentId },
        });
        const payload = (res.data ?? res) as DocumentDetailDto | undefined;
        if (!payload || cancelled) return;

        const target = (payload.files ?? []).find((f) => f.id === documentFileId);
        if (!target) {
          setError("Không tìm thấy chương/file tương ứng.");
          setDoc(payload);
          setFile(null);
          return;
        }

        setDoc(payload);
        setFile(target);
      } catch (err: any) {
        if (cancelled) return;
        const msg =
          err?.response?.data?.message ||
          err?.response?.data ||
          err?.message ||
          "Không thể tải chi tiết chương/file";
        setError(msg);
      } finally {
        if (!cancelled) {
          setLoading(false);
        }
      }
    };

    void load();

    return () => {
      cancelled = true;
    };
  }, [documentId, documentFileId]);

  if (!documentId || !documentFileId) {
    return (
      <div className="space-y-4">
        <p className="text-sm text-red-600 dark:text-red-400">
          Không tìm thấy mã tài liệu hoặc chương/file hợp lệ.
        </p>
      </div>
    );
  }

  if (loading && !doc) {
    return (
      <div className="flex items-center justify-center py-12">
        <FileText className="h-6 w-6 animate-pulse text-sky-500" />
      </div>
    );
  }

  if (error && (!doc || !file)) {
    return (
      <div className="space-y-4">
        <Button
          variant="ghost"
          size="sm"
          onClick={() => router.back()}
          className="inline-flex items-center gap-2"
        >
          <ArrowLeft className="h-4 w-4" />
          Quay lại
        </Button>
        <div className=" border border-red-200 bg-red-50 p-4 text-sm text-red-600 dark:border-red-900 dark:bg-red-950 dark:text-red-300">
          {error}
        </div>
      </div>
    );
  }

  if (!doc || !file) {
    return null;
  }

  const handleBackToDocument = () => {
    router.push(`/documents/${documentId}`);
  };

  const fileUrl = getFileUrlById(file.fileId);
  const fileSize = file.fileSize ? `${(file.fileSize / 1024 / 1024).toFixed(2)} MB` : "";

  return (
    <div className="flex h-full flex-col bg-slate-900">
      {/* Compact Header */}
      <div className="flex items-center justify-between gap-2 px-2 py-1.5 border-b border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-900 shrink-0">
        <div className="flex items-center gap-2 min-w-0">
          <Button
            variant="ghost"
            size="icon"
            onClick={() => router.back()}
            className="h-8 w-8 shrink-0"
          >
            <ArrowLeft className="h-4 w-4" />
          </Button>
          <div className="min-w-0 hidden sm:block">
            <p className="text-[10px] text-slate-500 dark:text-slate-400 truncate">
              {doc.documentName ?? "Tài liệu"}
            </p>
            <h1 className="text-xs font-medium text-foreground truncate max-w-[300px]">
              {file.title || "Chương / file"}
            </h1>
          </div>
        </div>

        <div className="flex items-center gap-1.5">
          {doc.subject?.subjectName && (
            <Badge variant="outline" className="border-emerald-200 text-emerald-700 text-[10px] py-0 h-5 hidden md:inline-flex">
              {doc.subject.subjectName}
            </Badge>
          )}
          <span className="text-[10px] text-slate-500 hidden lg:inline">
            {fileSize}{file.totalPages && ` • ${file.totalPages} trang`}
          </span>
          <Button
            variant={showComments ? "secondary" : "ghost"}
            size="icon"
            onClick={() => setShowComments(!showComments)}
            className="h-8 w-8"
            title={showComments ? "Ẩn bình luận" : "Hiện bình luận"}
          >
            <MessageSquare className="h-4 w-4" />
          </Button>
        </div>
      </div>

      {/* Main Content Area */}
      <div className="flex-1 flex overflow-hidden">
        {/* PDF/File Viewer - Takes maximum space */}
        <div className={cn(
          "flex-1 bg-slate-100 dark:bg-slate-950 transition-all duration-300",
          showComments ? "lg:mr-0" : ""
        )}>
          {fileUrl ? (
            file.mimeType === "application/pdf" ? (
              <PdfViewer
                fileUrl={fileUrl}
                fileId={file.id!}
                documentId={documentId}
                title={file.title || undefined}
                className="h-full w-full"
              />
            ) : (
              <iframe
                src={fileUrl}
                title={file.title || "Document file"}
                className="h-full w-full border-0"
              />
            )
          ) : (
            <div className="flex h-full flex-col items-center justify-center gap-2 text-sm text-slate-500 dark:text-slate-400">
              <FileText className="h-10 w-10 opacity-40" />
              <p>Không có URL để xem file.</p>
              <p className="text-xs">Vui lòng tải lại trang hoặc liên hệ quản trị.</p>
            </div>
          )}
        </div>

        {/* Comments Panel - Overlay on mobile, sidebar on desktop */}
        {showComments && (
          <>
            {/* Mobile overlay backdrop */}
            <div
              className="fixed inset-0 bg-black/50 z-40 lg:hidden"
              onClick={() => setShowComments(false)}
            />
            {/* Comments panel */}
            <div className={cn(
              "fixed right-0 top-0 bottom-0 z-50 w-[85vw] max-w-[320px]",
              "lg:relative lg:z-auto lg:w-[300px]",
              "border-l border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-900 flex flex-col"
            )}>
              <div className="flex flex-col h-full">
                <div className="flex items-center justify-between px-3 py-2 border-b border-slate-200 dark:border-slate-700 shrink-0">
                  <h2 className="text-sm font-semibold text-foreground">Bình luận</h2>
                  <Button
                    variant="ghost"
                    size="icon"
                    onClick={() => setShowComments(false)}
                    className="h-7 w-7"
                  >
                    <X className="h-4 w-4" />
                  </Button>
                </div>
                <div className="flex-1 overflow-hidden">
                  <DocumentFileCommentsPanel
                    documentId={documentId}
                    documentFileId={documentFileId}
                    initialUsefulCount={file.usefulCount ?? 0}
                    initialNotUsefulCount={file.notUsefulCount ?? 0}
                  />
                </div>
              </div>
            </div>
          </>
        )}
      </div>
    </div>
  );
}


