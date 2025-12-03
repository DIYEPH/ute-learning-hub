"use client";

import { useEffect, useState } from "react";
import { useParams, useRouter } from "next/navigation";
import { ArrowLeft, FileText } from "lucide-react";

import { getApiDocumentById } from "@/src/api/database/sdk.gen";
import type { DocumentDetailDto, DocumentFileDto } from "@/src/api/database/types.gen";
import { Button } from "@/src/components/ui/button";
import { Badge } from "@/src/components/ui/badge";
import { ScrollArea } from "@/src/components/ui/scroll-area";
import { DocumentFileCommentsPanel } from "@/src/components/documents/document-file-comments-panel";

export default function DocumentFileDetailPage() {
  const params = useParams<{ id: string; fileId: string }>();
  const router = useRouter();
  const documentId = params?.id;
  const documentFileId = params?.fileId;

  const [doc, setDoc] = useState<DocumentDetailDto | null>(null);
  const [file, setFile] = useState<DocumentFileDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

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
        <div className="rounded-md border border-red-200 bg-red-50 p-4 text-sm text-red-600 dark:border-red-900 dark:bg-red-950 dark:text-red-300">
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

  const fileUrl = file.fileUrl ?? "";
  const fileSize = file.fileSize ? `${(file.fileSize / 1024 / 1024).toFixed(2)} MB` : "";

  return (
    <div className="flex h-full flex-col gap-2 lg:gap-3">
      <div className="flex items-center justify-between gap-3">
        <div className="flex items-center gap-2">
          <Button
            variant="ghost"
            size="sm"
            onClick={() => router.back()}
            className="inline-flex items-center gap-2"
          >
            <ArrowLeft className="h-4 w-4" />
            Quay lại
          </Button>
          <Button
            variant="ghost"
            size="sm"
            onClick={handleBackToDocument}
            className="hidden sm:inline-flex text-xs"
          >
            Xem trang tài liệu
          </Button>
        </div>
      </div>

      <div className="grid gap-3 lg:gap-4 lg:grid-cols-[minmax(0,2fr)_minmax(280px,340px)] items-stretch h-[calc(100vh-130px)]">
        {/* Trái: viewer nội dung file */}
        <div className="rounded-2xl border border-slate-200 bg-white shadow-sm dark:border-slate-700 dark:bg-slate-900 flex flex-col overflow-hidden">
          <div className="border-b border-slate-200 px-4 py-2 dark:border-slate-700 flex items-center justify-between gap-2">
            <div className="min-w-0">
              <p className="text-[11px] font-semibold text-slate-500 dark:text-slate-400">
                {doc.documentName ?? "Tài liệu"}
              </p>
              <h1 className="text-sm font-semibold text-foreground truncate">
                {file.title || file.fileName || "Chương / file"}
              </h1>
              <p className="text-[11px] text-slate-500 dark:text-slate-400 truncate">
                {file.fileName}
                {fileSize && ` • ${fileSize}`}
                {file.totalPages && ` • ${file.totalPages} trang`}
              </p>
            </div>
            <div className="hidden sm:flex flex-col items-end gap-1 text-[11px] text-slate-500 dark:text-slate-400">
              <div className="flex flex-wrap justify-end gap-1">
                {file.isPrimary && (
                  <Badge variant="outline" className="border-amber-300 text-amber-700">
                    Chính
                  </Badge>
                )}
                {doc.subject?.subjectName && (
                  <Badge variant="outline" className="border-emerald-200 text-emerald-700">
                    {doc.subject.subjectName}
                  </Badge>
                )}
              </div>
            </div>
          </div>

          <div className="flex-1 bg-slate-50 dark:bg-slate-950">
            {fileUrl ? (
              <iframe
                src={fileUrl}
                title={file.fileName || "Document file"}
                className="h-full w-full border-0"
              />
            ) : (
              <div className="flex h-full flex-col items-center justify-center gap-2 text-sm text-slate-500 dark:text-slate-400">
                <FileText className="h-6 w-6 opacity-60" />
                <p>Không có URL để xem file. Vui lòng tải lại trang hoặc liên hệ quản trị.</p>
              </div>
            )}
          </div>
        </div>

        {/* Phải: bình luận + useful / unuseful */}
        <div className="rounded-2xl border border-slate-200 bg-white shadow-sm dark:border-slate-700 dark:bg-slate-900 flex flex-col">
          <DocumentFileCommentsPanel
            documentId={documentId}
            documentFileId={documentFileId}
            initialUsefulCount={file.usefulCount ?? 0}
            initialNotUsefulCount={file.notUsefulCount ?? 0}
          />
        </div>
      </div>
    </div>
  );
}


