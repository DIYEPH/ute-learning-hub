"use client";

import { useEffect, useState } from "react";
import { useParams, useRouter } from "next/navigation";
import { ArrowLeft, Loader2, FileText, Edit } from "lucide-react";

import { getApiDocumentById } from "@/src/api/database/sdk.gen";
import type { DocumentDetailDto } from "@/src/api/database/types.gen";
import { Button } from "@/src/components/ui/button";
import { Badge } from "@/src/components/ui/badge";
import { ScrollArea } from "@/src/components/ui/scroll-area";
import { EditDocumentModal } from "@/src/components/documents/edit-document-modal";
import { DocumentFileUpload } from "@/src/components/documents/document-file-upload";
import { DocumentFileList } from "@/src/components/documents/document-file-list";

export default function DocumentDetailPage() {
  const params = useParams<{ id: string }>();
  const router = useRouter();
  const documentId = params?.id;

  const [data, setData] = useState<DocumentDetailDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [showEditModal, setShowEditModal] = useState(false);

  useEffect(() => {
    if (!documentId) return;
    let cancelled = false;

    const fetchDetail = async () => {
      setLoading(true);
      setError(null);
      try {
        const response = await getApiDocumentById({
          path: { id: documentId },
        });

        const payload = (response.data ??
          response) as DocumentDetailDto | undefined;
        // Check if payload is a valid document (has id) and not an error object
        if (!cancelled && payload && typeof payload === 'object' && 'id' in payload) {
          setData(payload);
        }
      } catch (err: any) {
        if (cancelled) return;
        const message =
          err?.response?.data?.message ||
          err?.message ||
          "Không thể tải chi tiết tài liệu";
        setError(message);
      } finally {
        if (!cancelled) {
          setLoading(false);
        }
      }
    };

    void fetchDetail();

    return () => {
      cancelled = true;
    };
  }, [documentId]);



  const refreshData = async () => {
    if (!documentId) return;
    try {
      const response = await getApiDocumentById({
        path: { id: documentId },
      });
      const payload = (response.data ?? response) as DocumentDetailDto | undefined;
      // Check if payload is a valid document (has id) and not an error object
      if (payload && typeof payload === 'object' && 'id' in payload) {
        setData(payload);
      }
    } catch (err) {
      console.error("Error refreshing data:", err);
    }
  };

  if (!documentId) {
    return (
      <div className="space-y-4">
        <p className="text-sm text-red-600 dark:text-red-400">
          Không tìm thấy mã tài liệu hợp lệ.
        </p>
      </div>
    );
  }

  if (loading && !data) {
    return (
      <div className="flex items-center justify-center py-12">
        <Loader2 className="h-6 w-6 animate-spin text-sky-500" />
      </div>
    );
  }

  if (error && !data) {
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

  // Guard against null data
  if (!data) {
    return (
      <div className="flex items-center justify-center py-12">
        <Loader2 className="h-6 w-6 animate-spin text-sky-500" />
      </div>
    );
  }

  const doc = data;
  const files = doc.files ?? [];
  const tags = doc.tags ?? [];
  const authors = doc.authors ?? [];

  return (
    <div className="flex flex-col gap-4 lg:gap-6">
      <div className="flex items-center justify-between gap-3">
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
          variant="outline"
          size="sm"
          onClick={() => setShowEditModal(true)}
          className="inline-flex items-center gap-1"
        >
          <Edit className="h-4 w-4" />
          Sửa thông tin
        </Button>
      </div>

      {/* Thông tin document + upload + danh sách file */}
      <div className="grid gap-4 lg:gap-6 lg:grid-cols-[minmax(260px,320px)_minmax(0,1fr)] items-start">
        {/* Bên trái: thông tin + upload */}
        <div className="space-y-4">
          <div className="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm dark:border-slate-700 dark:bg-slate-900">
            <div className="space-y-3">
              <div>
                <h1 className="text-xl font-semibold text-foreground">
                  {doc.documentName ?? "Tài liệu"}
                </h1>
                {authors.length > 0 && (
                  <p className="text-xs text-slate-500 dark:text-slate-400 mt-1">
                    Tác giả:{" "}
                    <span className="font-medium">
                      {authors.map((a) => a.fullName).join(", ")}
                    </span>
                  </p>
                )}
              </div>

              <div className="flex flex-wrap gap-2 text-xs">
                {doc.type?.typeName && (
                  <Badge variant="outline" className="border-sky-200 text-sky-700">
                    {doc.type.typeName}
                  </Badge>
                )}
                {doc.subject?.subjectName && (
                  <Badge
                    variant="outline"
                    className="border-emerald-200 text-emerald-700"
                  >
                    {doc.subject.subjectName}
                  </Badge>
                )}
                {doc.visibility === 0 && (
                  <Badge
                    variant="outline"
                    className="border-blue-200 text-blue-700"
                  >
                    Công khai
                  </Badge>
                )}
                {doc.visibility === 1 && (
                  <Badge
                    variant="outline"
                    className="border-slate-200 text-slate-700"
                  >
                    Riêng tư
                  </Badge>
                )}
                {doc.visibility === 2 && (
                  <Badge
                    variant="outline"
                    className="border-amber-200 text-amber-700"
                  >
                    Nội bộ
                  </Badge>
                )}
              </div>

              {tags.length > 0 && (
                <div className="flex flex-wrap gap-1.5">
                  {tags.map((tag) => (
                    <Badge
                      key={tag.id}
                      variant="secondary"
                      className="bg-slate-100 text-slate-700 dark:bg-slate-800 dark:text-slate-200 text-[11px]"
                    >
                      #{tag.tagName}
                    </Badge>
                  ))}
                </div>
              )}

              {doc.description && (
                <div className="space-y-1">
                  <h2 className="text-xs font-semibold text-foreground">Mô tả</h2>
                  <ScrollArea className="max-h-40 rounded-md border bg-slate-50 p-3 text-xs text-slate-700 dark:bg-slate-900 dark:text-slate-200">
                    {doc.description}
                  </ScrollArea>
                </div>
              )}
            </div>
          </div>

          {/* Form upload chương/file */}
          <div className="rounded-2xl border border-slate-200 bg-white p-4 shadow-sm dark:border-slate-700 dark:bg-slate-900">
            <DocumentFileUpload
              documentId={documentId}
              onUploadSuccess={refreshData}
            />
          </div>
        </div>

        {/* Bên phải: danh sách chương/file */}
        <div className="rounded-2xl border border-slate-200 bg-white shadow-sm dark:border-slate-700 dark:bg-slate-900 flex flex-col max-h-[calc(100vh-220px)]">
          <div className="border-b border-slate-200 px-4 py-3 dark:border-slate-700 flex items-center justify-between">
            <div className="flex items-center gap-2 text-sm font-semibold text-foreground">
              <FileText className="h-4 w-4" />
              <span>Chương / file ({files.length})</span>
            </div>
          </div>
          <ScrollArea className="flex-1 px-4 py-3">
            <DocumentFileList files={files} document={doc} />
          </ScrollArea>
        </div>
      </div>

      {/* Modal chỉnh sửa document */}
      {data && (
        <EditDocumentModal
          open={showEditModal}
          onOpenChange={setShowEditModal}
          document={data}
          onSuccess={refreshData}
        />
      )}
    </div>
  );
}

