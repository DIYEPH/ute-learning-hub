"use client";

import { useEffect, useRef, useState } from "react";
import { useParams, useRouter } from "next/navigation";
import { ArrowLeft, FileText, MessageSquare, X } from "lucide-react";

import {
  getApiDocumentById,
  postApiDocumentFilesByFileIdView,
} from "@/src/api/database/sdk.gen";
import type { DocumentDetailDto, DocumentFileDto } from "@/src/api/database/types.gen";

import { Button } from "@/src/components/ui/button";
import { Badge } from "@/src/components/ui/badge";
import { DocumentFileCommentsPanel } from "@/src/components/documents/document-file-comments-panel";
import { PdfViewer } from "@/src/components/documents/pdf-viewer";

import { getFileUrlById } from "@/src/lib/file-url";
import { cn } from "@/lib/utils";

export default function DocumentFileDetailPage() {
  const { id: documentId, fileId: documentFileId } =
    useParams<{ id: string; fileId: string }>();

  const router = useRouter();
  const viewCountedRef = useRef(false);

  const [doc, setDoc] = useState<DocumentDetailDto | null>(null);
  const [file, setFile] = useState<DocumentFileDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [showComments, setShowComments] = useState(() => {
    if (typeof window !== 'undefined') {
      return window.innerWidth >= 1024; // lg breakpoint
    }
    return false;
  });

  /* ======================= DATA ======================= */

  const loadData = async () => {
    if (!documentId || !documentFileId) return;

    setLoading(true);
    try {
      const res = await getApiDocumentById({ path: { id: documentId } });
      const payload = (res.data ?? res) as DocumentDetailDto;
      if (!payload) return;

      const target = payload.files?.find(f => f.id === documentFileId) ?? null;

      setDoc(payload);
      setFile(target);

      if (target && !viewCountedRef.current) {
        viewCountedRef.current = true;
        void postApiDocumentFilesByFileIdView({ path: { fileId: documentFileId } });
      }
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { void loadData(); }, [documentId, documentFileId]);

  /* ======================= GUARDS ======================= */

  if (!documentId || !documentFileId)
    return <p className="text-sm text-red-600">Thiếu mã tài liệu hoặc file</p>;

  if (loading && !doc)
    return (
      <div className="flex justify-center py-12">
        <FileText className="h-6 w-6 animate-pulse text-sky-500" />
      </div>
    );

  if (!doc || !file) return null;

  /* ======================= DERIVED ======================= */

  const fileUrl = getFileUrlById(file.fileId);
  const fileSize = file.fileSize
    ? `${(file.fileSize / 1024 / 1024).toFixed(2)} MB`
    : "";

  /* ======================= UI ======================= */

  return (
    <div className="flex h-full flex-col bg-slate-900">

      {/* Header */}
      <div className="flex items-center justify-between px-2 py-1.5 border-b bg-white dark:bg-slate-900">

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

      {/* Content */}
      <div className="flex flex-1 overflow-hidden">

        {/* Viewer */}
        <div className={cn(
          "flex-1 bg-slate-100 dark:bg-slate-950 transition-all",
          showComments && "lg:mr-0"
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

        {/* Comments */}
        {showComments && (
          <>
            <div className="fixed inset-0 bg-black/50 z-40 lg:hidden"
              onClick={() => setShowComments(false)} />

            <div className={cn(
              "fixed right-0 top-0 bottom-0 z-50 w-[85vw] max-w-[320px]",
              "lg:relative lg:w-[300px]",
              "border-l bg-white dark:bg-slate-900 flex flex-col"
            )}>

              <div className="flex items-center justify-between px-3 py-2 border-b">
                <h2 className="text-sm font-semibold">Bình luận</h2>
                <Button size="icon" variant="ghost" onClick={() => setShowComments(false)}>
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
          </>
        )}

      </div>
    </div>
  );
}
