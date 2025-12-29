"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { Loader2, RefreshCcw, FileText } from "lucide-react";

import { getApiDocumentMy } from "@/src/api/database/sdk.gen";
import type { DocumentDto, PagedResponseOfDocumentDto } from "@/src/api/database/types.gen";
import { DocumentCard } from "@/src/components/documents/document-card";
import { Input } from "@/src/components/ui/input";
import { Button } from "@/src/components/ui/button";

const PAGE_SIZE = 12;

export default function LibraryPage() {
  const router = useRouter();
  const [docs, setDocs] = useState<DocumentDto[]>([]);
  const [page, setPage] = useState(1);
  const [hasNext, setHasNext] = useState(false);
  const [search, setSearch] = useState("");
  const [debounced, setDebounced] = useState("");
  const [loading, setLoading] = useState(true);
  const [loadingMore, setLoadingMore] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const t = setTimeout(() => setDebounced(search.trim()), 400);
    return () => clearTimeout(t);
  }, [search]);

  useEffect(() => {
    fetchDocs(1, debounced, false);
  }, [debounced]);

  const fetchDocs = async (p: number, q: string, append: boolean) => {
    append ? setLoadingMore(true) : setLoading(true);
    setError(null);
    try {
      const res = await getApiDocumentMy({
        query: { Page: p, PageSize: PAGE_SIZE, SearchTerm: q || undefined },
      });
      const data = (res.data ?? res) as PagedResponseOfDocumentDto;
      setDocs(v => (append ? [...v, ...(data.items ?? [])] : data.items ?? []));
      setPage(p);
      setHasNext(!!data.hasNextPage);
    } catch (e: any) {
      setError(e?.response?.data?.message || e?.message || "Không thể tải tài liệu");
    } finally {
      append ? setLoadingMore(false) : setLoading(false);
    }
  };

  if (loading && docs.length === 0)
    return (
      <div className="flex justify-center py-12">
        <Loader2 className="animate-spin text-primary" />
      </div>
    );

  return (
    <div className="space-y-6">

      <div className="flex flex-col gap-4 md:flex-row md:justify-between">
        <div>
          <h1 className="text-2xl font-semibold">Thư viện của tôi</h1>
          <p className="text-sm text-muted-foreground">Những tài liệu bạn đã chia sẻ</p>
        </div>
        <Input
          className="md:w-80"
          placeholder="Tìm kiếm tài liệu..."
          value={search}
          onChange={e => setSearch(e.target.value)}
        />
      </div>

      {error && (
        <div className="flex justify-between items-center bg-red-50 px-4 py-3 text-sm text-red-600">
          <span>{error}</span>
          <button onClick={() => fetchDocs(1, debounced, false)} className="flex gap-1 underline">
            <RefreshCcw size={16} /> Thử lại
          </button>
        </div>
      )}

      {docs.length === 0 ? (
        <div className="text-center py-12 text-muted-foreground">
          <FileText className="h-12 w-12 mx-auto mb-3 opacity-40" />
          <p>Chưa có tài liệu nào</p>
        </div>
      ) : (
        <div className="grid gap-3 grid-cols-2 sm:grid-cols-3 lg:grid-cols-4 xl:grid-cols-5 2xl:grid-cols-6">
          {docs.map(d => (
            <DocumentCard
              key={d.id}
              id={d.id}
              title={d.documentName ?? "Tài liệu"}
              subjectName={d.subject?.subjectName}
              tags={d.tags?.map(t => t.tagName || "").filter(Boolean)}
              thumbnailFileId={d.thumbnailFileId}
              fileCount={d.fileCount}
              commentCount={d.commentCount}
              usefulCount={d.usefulCount}
              notUsefulCount={d.notUsefulCount}
              totalViewCount={d.totalViewCount}
              href={d.id ? `/documents/${d.id}` : undefined}
            />
          ))}
        </div>
      )}

      {hasNext && (
        <div className="flex justify-center">
          <Button
            variant="outline"
            disabled={loadingMore}
            onClick={() => fetchDocs(page + 1, debounced, true)}
          >
            {loadingMore && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
            Tải thêm
          </Button>
        </div>
      )}

    </div>
  );
}
