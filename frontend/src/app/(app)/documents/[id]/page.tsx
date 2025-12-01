"use client";

import { useEffect, useState } from "react";
import { useParams, useRouter } from "next/navigation";
import { ArrowLeft, Loader2, Download, FileText, Eye } from "lucide-react";

import { getApiDocumentById } from "@/src/api/database/sdk.gen";
import type { DocumentDetailDto } from "@/src/api/database/types.gen";
import { Button } from "@/src/components/ui/button";
import { Badge } from "@/src/components/ui/badge";
import { ScrollArea } from "@/src/components/ui/scroll-area";

export default function DocumentDetailPage() {
  const params = useParams<{ id: string }>();
  const router = useRouter();
  const documentId = params?.id;

  const [data, setData] = useState<DocumentDetailDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

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
        if (!cancelled && payload) {
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

  const handleOpenFile = () => {
    const url = data?.file?.fileUrl;
    if (!url) return;
    window.open(url, "_blank");
  };

  const handleDownloadFile = () => {
    const url = data?.file?.fileUrl;
    if (!url) return;

    const link = document.createElement("a");
    link.href = url;
    link.target = "_blank";
    link.download = data?.file?.fileName || "document";
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
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

  const doc = data!;
  const file = doc.file;
  const tags = doc.tags ?? [];
  const fileUrl = file?.fileUrl ?? "";
  const mimeType = file?.mimeType ?? "";

  const isPdf = mimeType.includes("pdf");
  const isImage = mimeType.startsWith("image/");

  return (
    <div className="space-y-6">
      <Button
        variant="ghost"
        size="sm"
        onClick={() => router.back()}
        className="inline-flex items-center gap-2"
      >
        <ArrowLeft className="h-4 w-4" />
        Quay lại
      </Button>

      <div className="grid gap-6 lg:grid-cols-[minmax(0,4fr)_minmax(0,1fr)]">
        {/* Cột trái: Viewer */}
        <div className="space-y-4">
          <div className="rounded-2xl border border-slate-200 bg-white shadow-sm dark:border-slate-700 dark:bg-slate-900">
            <div className="flex items-center justify-between border-b border-slate-200 px-4 py-2 text-sm dark:border-slate-700">
              <div className="flex items-center gap-2">
                <FileText className="h-4 w-4 text-slate-600 dark:text-slate-300" />
                <span className="font-medium text-foreground">
                  {file?.fileName ?? "Tệp tài liệu"}
                </span>
              </div>
              <div className="flex gap-2">
                <Button
                  type="button"
                  size="sm"
                  variant="outline"
                  onClick={handleOpenFile}
                  disabled={!fileUrl}
                  className="inline-flex items-center gap-1"
                >
                  <Eye className="h-4 w-4" />
                  Xem tab mới
                </Button>
                <Button
                  type="button"
                  size="sm"
                  onClick={handleDownloadFile}
                  disabled={!fileUrl || doc.isDownload === false}
                  className="inline-flex items-center gap-1"
                >
                  <Download className="h-4 w-4" />
                  Tải xuống
                </Button>
              </div>
            </div>

            <div className="h-[70vh] w-full bg-slate-50 dark:bg-slate-950">
              {fileUrl ? (
                isPdf ? (
                  <iframe
                    src={fileUrl}
                    className="h-full w-full"
                    title={doc.documentName ?? "Tài liệu PDF"}
                  />
                ) : isImage ? (
                  <img
                    src={fileUrl}
                    alt={doc.documentName ?? "Hình ảnh tài liệu"}
                    className="h-full w-full object-contain bg-black/5"
                  />
                ) : (
                  <div className="flex h-full flex-col items-center justify-center gap-3 text-sm text-slate-600 dark:text-slate-300">
                    <p>Không hỗ trợ xem trực tiếp loại tệp này.</p>
                    <Button
                      type="button"
                      size="sm"
                      onClick={handleOpenFile}
                      disabled={!fileUrl}
                      className="inline-flex items-center gap-1"
                    >
                      <Eye className="h-4 w-4" />
                      Mở trong tab mới
                    </Button>
                  </div>
                )
              ) : (
                <div className="flex h-full items-center justify-center text-sm text-slate-500 dark:text-slate-400">
                  Không tìm thấy tệp tài liệu.
                </div>
              )}
            </div>

            {doc.isDownload === false && (
              <p className="px-4 py-2 text-xs text-amber-600 dark:text-amber-400 border-t border-slate-200 dark:border-slate-700">
                Tác giả đã tắt tính năng tải xuống cho tài liệu này.
              </p>
            )}
          </div>
        </div>

        {/* Cột phải: thông tin chi tiết */}
        <div className="space-y-4">
          <div className="space-y-2">
            <h1 className="text-2xl font-semibold text-foreground">
              {doc.documentName ?? "Tài liệu"}
            </h1>
            {doc.authorName && (
              <p className="text-sm text-slate-500 dark:text-slate-400">
                Tác giả: <span className="font-medium">{doc.authorName}</span>
              </p>
            )}
          </div>

          <div className="flex flex-wrap gap-2">
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
            <div className="flex flex-wrap gap-2">
              {tags.map((tag) => (
                <Badge
                  key={tag.id}
                  variant="secondary"
                  className="bg-slate-100 text-slate-700 dark:bg-slate-800 dark:text-slate-200 text-xs"
                >
                  #{tag.tagName}
                </Badge>
              ))}
            </div>
          )}

          {doc.description && (
            <div className="space-y-1">
              <h2 className="text-sm font-semibold text-foreground">Mô tả</h2>
              <ScrollArea className="max-h-40 rounded-md border bg-slate-50 p-3 text-sm text-slate-700 dark:bg-slate-900 dark:text-slate-200">
                {doc.description}
              </ScrollArea>
            </div>
          )}

          {doc.descriptionAuthor && (
            <div className="space-y-1">
              <h2 className="text-sm font-semibold text-foreground">
                Giới thiệu tác giả
              </h2>
              <p className="text-sm text-slate-700 dark:text-slate-200">
                {doc.descriptionAuthor}
              </p>
            </div>
          )}

        </div>
      </div>

      {/* Khu vực bình luận phía dưới */}
      <div className="space-y-3 rounded-2xl border border-slate-200 bg-white p-4 shadow-sm dark:border-slate-700 dark:bg-slate-900">
        <h2 className="text-sm font-semibold text-foreground">Bình luận</h2>
        <p className="text-xs text-slate-500 dark:text-slate-400">
         Chưa làm 
        </p>
      </div>
    </div>
  );
}


