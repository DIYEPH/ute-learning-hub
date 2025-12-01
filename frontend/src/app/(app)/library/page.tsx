"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { Loader2, RefreshCcw } from "lucide-react";

import { getApiDocumentMy } from "@/src/api/database/sdk.gen";
import type {
  DocumentDto,
  PagedResponseOfDocumentDto,
} from "@/src/api/database/types.gen";
import { DocumentCard } from "@/src/components/documents/document-card";
import { Input } from "@/src/components/ui/input";
import { Button } from "@/src/components/ui/button";

const PAGE_SIZE = 12;

export default function LibraryPage() {
  const router = useRouter();
  const [documents, setDocuments] = useState<DocumentDto[]>([]);
  const [page, setPage] = useState(1);
  const [hasNextPage, setHasNextPage] = useState(false);
  const [searchValue, setSearchValue] = useState("");
  const [debouncedSearch, setDebouncedSearch] = useState("");
  const [isLoading, setIsLoading] = useState(true);
  const [isLoadingMore, setIsLoadingMore] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const timer = setTimeout(() => {
      setDebouncedSearch(searchValue.trim());
    }, 400);

    return () => clearTimeout(timer);
  }, [searchValue]);

  useEffect(() => {
    void fetchDocuments(1, debouncedSearch, false);
  }, [debouncedSearch]);

  const fetchDocuments = async (
    pageNumber: number,
    query: string,
    append: boolean
  ) => {
    if (append) {
      setIsLoadingMore(true);
    } else {
      setIsLoading(true);
    }
    setError(null);

    try {
      const response = await getApiDocumentMy({
        query: {
          Page: pageNumber,
          PageSize: PAGE_SIZE,
          SearchTerm: query || undefined,
        },
      });

      const payload = (response.data ??
        response) as PagedResponseOfDocumentDto | undefined;
      const items = payload?.items ?? [];

      setDocuments((prev) =>
        append ? [...prev, ...items] : items
      );
      setPage(pageNumber);
      setHasNextPage(Boolean(payload?.hasNextPage));
    } catch (err: any) {
      const message =
        err?.response?.data?.message ||
        err?.message ||
        "Không thể tải danh sách tài liệu";
      setError(message);
    } finally {
      if (append) {
        setIsLoadingMore(false);
      } else {
        setIsLoading(false);
      }
    }
  };

  const handleLoadMore = () => {
    if (hasNextPage && !isLoadingMore) {
      void fetchDocuments(page + 1, debouncedSearch, true);
    }
  };

  const handleRetry = () => {
    void fetchDocuments(1, debouncedSearch, false);
  };

  const showSkeleton = isLoading && documents.length === 0;

  const handleEditDocument = (documentId?: string) => {
    if (!documentId) return;
    router.push(`/documents/${documentId}/edit`);
  };

  const handleDeleteDocument = (documentId?: string) => {
    if (!documentId) return;
    console.info("Delete document", documentId);
  };

  const handleReportDocument = (documentId?: string) => {
    if (!documentId) return
    console.info("Report document", documentId);
  };

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-4 md:flex-row md:items-center md:justify-between">
        <div>
          <h1 className="text-2xl font-semibold text-foreground">Thư viện của tôi</h1>
          <p className="text-sm text-slate-500 dark:text-slate-400">
            Những tài liệu bạn đã chia sẻ. Tìm kiếm hoặc chỉnh sửa tại đây.
          </p>
        </div>
        <div className="w-full md:w-80">
          <Input
            placeholder="Tìm kiếm theo tên, mô tả hoặc tác giả..."
            value={searchValue}
            onChange={(event) => setSearchValue(event.target.value)}
          />
        </div>
      </div>

      {error && (
        <div className="flex items-center justify-between rounded-md border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-600 dark:border-red-900 dark:bg-red-950 dark:text-red-300">
          <span>{error}</span>
          <button
            type="button"
            onClick={handleRetry}
            className="inline-flex items-center gap-1 text-red-600 underline-offset-4 hover:underline dark:text-red-300"
          >
            <RefreshCcw size={16} />
            Thử lại
          </button>
        </div>
      )}

      {showSkeleton ? (
        <div className="flex items-center justify-center py-12">
          <Loader2 className="h-6 w-6 animate-spin text-sky-500" />
        </div>
      ) : documents.length === 0 ? (
        <div className="rounded-2xl border border-dashed border-slate-300 bg-white p-8 text-center dark:border-slate-700 dark:bg-slate-900">
          <h2 className="text-lg font-semibold text-foreground">
            Chưa có tài liệu nào
          </h2>
        </div>
      ) : (
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
          {documents.map((doc) => (
            <DocumentCard
              key={doc.id}
              id={doc.id}
              title={doc.documentName ?? "Tài liệu"}
              subjectName={doc.subject?.subjectName}
              tags={doc.tags?.map((tag) => tag.tagName ?? "").filter(Boolean)}
              thumbnailUrl={doc.thumbnailUrl}
              fileMimeType={doc.fileMimeType ?? undefined}
              onEdit={() => handleEditDocument(doc.id)}
              onDelete={() => handleDeleteDocument(doc.id)}
              onReport={() => handleReportDocument(doc.id)}
              href={doc.id ? `/documents/${doc.id}` : undefined}
            />
          ))}
        </div>
      )}

      {hasNextPage && (
        <div className="flex justify-center">
          <Button
            type="button"
            variant="outline"
            onClick={handleLoadMore}
            disabled={isLoadingMore}
          >
            {isLoadingMore && (
              <Loader2 className="mr-2 h-4 w-4 animate-spin" />
            )}
            Tải thêm
          </Button>
        </div>
      )}
    </div>
  );
}


