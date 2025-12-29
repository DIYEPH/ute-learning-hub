"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { Loader2, Clock, FileText } from "lucide-react";

import { getApiDocumentReadingHistory } from "@/src/api/database/sdk.gen";
import type {
    ReadingHistoryItemDto,
    PagedResponseOfReadingHistoryItemDto,
} from "@/src/api/database/types.gen";
import { Button } from "@/src/components/ui/button";
import { getFileUrlById } from "@/src/lib/file-url";

const PAGE_SIZE = 20;

function formatRelativeTime(dateString?: string | null): string {
    if (!dateString) return "";
    const date = new Date(dateString);
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffMinutes = Math.floor(diffMs / 60000);
    const diffHours = Math.floor(diffMs / 3600000);
    const diffDays = Math.floor(diffMs / 86400000);

    if (diffMinutes < 1) return "Vừa xong";
    if (diffMinutes < 60) return `${diffMinutes} phút trước`;
    if (diffHours < 24) return `${diffHours} giờ trước`;
    if (diffDays < 7) return `${diffDays} ngày trước`;
    return date.toLocaleDateString("vi-VN");
}

export default function RecentPage() {
    const [items, setItems] = useState<ReadingHistoryItemDto[]>([]);
    const [page, setPage] = useState(1);
    const [hasNextPage, setHasNextPage] = useState(false);
    const [isLoading, setIsLoading] = useState(true);
    const [isLoadingMore, setIsLoadingMore] = useState(false);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        void fetchHistory(1, false);
    }, []);

    const fetchHistory = async (pageNumber: number, append: boolean) => {
        if (append) {
            setIsLoadingMore(true);
        } else {
            setIsLoading(true);
        }
        setError(null);

        try {
            const response = await getApiDocumentReadingHistory({
                query: {
                    Page: pageNumber,
                    PageSize: PAGE_SIZE,
                },
            });

            const payload = (response.data ??
                response) as PagedResponseOfReadingHistoryItemDto | undefined;
            const fetchedItems = payload?.items ?? [];

            setItems((prev) => (append ? [...prev, ...fetchedItems] : fetchedItems));
            setPage(pageNumber);
            setHasNextPage(Boolean(payload?.hasNextPage));
        } catch (err: any) {
            const message =
                err?.response?.data?.message ||
                err?.message ||
                "Không thể tải lịch sử đọc";
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
            void fetchHistory(page + 1, true);
        }
    };

    const showSkeleton = isLoading && items.length === 0;

    return (
        <div className="space-y-6">
            <div>
                <h1 className="text-2xl font-semibold text-foreground">
                    Gần đây
                </h1>
                <p className="text-sm text-muted-foreground mt-1">
                    Những tài liệu bạn đã đọc gần đây
                </p>
            </div>

            {error && (
                <div className=" border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-600 dark:border-red-900 dark:bg-red-950 dark:text-red-300">
                    {error}
                </div>
            )}

            {showSkeleton ? (
                <div className="flex items-center justify-center py-12">
                    <Loader2 className="h-6 w-6 animate-spin text-primary" />
                </div>
            ) : items.length === 0 ? (
                <div className="text-center py-12 text-muted-foreground">
                    <p>Chưa có lịch sử đọc</p>
                </div>
            ) : (
                <div className="space-y-3">
                    {items.map((item) => {
                        const coverUrl = getFileUrlById(item.coverFileId);
                        const progress = item.totalPages
                            ? Math.round(((item.lastPage ?? 0) / item.totalPages) * 100)
                            : 0;

                        return (
                            <Link
                                key={`${item.documentId}-${item.documentFileId}`}
                                href={`/documents/${item.documentId}`}
                                className="flex items-center gap-4 p-4 border border-border bg-card hover:border-primary hover:shadow-sm transition-all"
                            >
                                {/* Thumbnail */}
                                <div className="flex-shrink-0 w-16 h-16 bg-muted overflow-hidden flex items-center justify-center">
                                    {coverUrl ? (
                                        <img
                                            src={coverUrl}
                                            alt={item.documentName}
                                            className="w-full h-full object-contain"
                                        />
                                    ) : (
                                        <FileText className="h-8 w-8 text-muted-foreground" />
                                    )}
                                </div>

                                {/* Content */}
                                <div className="flex-1 min-w-0">
                                    <h3 className="font-medium text-foreground truncate">
                                        {item.documentName}
                                    </h3>
                                    <div className="flex items-center gap-2 text-xs text-muted-foreground mt-1">
                                        {item.subjectName && <span>{item.subjectName}</span>}
                                        {item.fileTitle && (
                                            <>
                                                <span>•</span>
                                                <span className="truncate">{item.fileTitle}</span>
                                            </>
                                        )}
                                    </div>
                                    {/* Progress bar */}
                                    {item.totalPages && item.totalPages > 0 && (
                                        <div className="mt-2 flex items-center gap-2">
                                            <div className="flex-1 h-1.5 bg-muted rounded-full overflow-hidden">
                                                <div
                                                    className="h-full bg-primary rounded-full transition-all"
                                                    style={{ width: `${progress}%` }}
                                                />
                                            </div>
                                            <span className="text-xs text-muted-foreground whitespace-nowrap">
                                                {item.lastPage}/{item.totalPages} trang
                                            </span>
                                        </div>
                                    )}
                                </div>

                                {/* Time & Continue reading */}
                                <div className="flex-shrink-0 flex flex-col items-end gap-2">
                                    <span className="text-xs text-muted-foreground whitespace-nowrap">
                                        {formatRelativeTime(item.lastAccessedAt)}
                                    </span>
                                    {item.documentFileId && (
                                        <Button
                                            variant="outline"
                                            size="sm"
                                            onClick={(e: React.MouseEvent) => {
                                                e.preventDefault();
                                                e.stopPropagation();
                                                window.location.href = `/documents/${item.documentId}/files/${item.documentFileId}`;
                                            }}
                                            className="text-xs h-7 px-2"
                                        >
                                            Đọc tiếp
                                        </Button>
                                    )}
                                </div>
                            </Link>
                        );
                    })}
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

