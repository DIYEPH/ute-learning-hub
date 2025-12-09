"use client";

import { useTranslations } from "next-intl";
import { BaseTable, BaseTableColumn } from "@/src/components/admin/tables/base-table";
import { Button } from "@/src/components/ui/button";
import { Badge } from "@/src/components/ui/badge";
import { Eye, CheckCircle } from "lucide-react";
import type { DocumentDto } from "@/src/api/database/types.gen";

// ReviewStatus enum mapping: PendingReview=0, Hidden=1, Approved=2, Rejected=3
const ReviewStatusLabels: Record<number, string> = {
    0: "Chờ duyệt",
    1: "Ẩn",
    2: "Đã duyệt",
    3: "Bị từ chối",
};

const ReviewStatusColors: Record<number, string> = {
    0: "bg-amber-100 text-amber-700 dark:bg-amber-900 dark:text-amber-300",
    1: "bg-slate-100 text-slate-700 dark:bg-slate-800 dark:text-slate-300",
    2: "bg-green-100 text-green-700 dark:bg-green-900 dark:text-green-300",
    3: "bg-red-100 text-red-700 dark:bg-red-900 dark:text-red-300",
};

// VisibilityStatus enum mapping
const VisibilityLabels: Record<number, string> = {
    0: "Private",
    1: "Public",
};

interface DocumentTableProps {
    documents: DocumentDto[];
    onViewDetail?: (document: DocumentDto) => void;
    onEdit?: (document: DocumentDto) => void;
    onDelete?: (document: DocumentDto) => void;
    onReview?: (document: DocumentDto) => void;
    onBulkDelete?: (ids: string[]) => void | Promise<void>;
    loading?: boolean;
    onSort?: (sortKey: string, direction: "asc" | "desc" | null) => void;
    sortKey?: string | null;
    sortDirection?: "asc" | "desc" | null;
    enableClientSort?: boolean;
}

export function DocumentTable({
    documents,
    onViewDetail,
    onEdit,
    onDelete,
    onReview,
    onBulkDelete,
    loading,
    onSort,
    sortKey,
    sortDirection,
    enableClientSort,
}: DocumentTableProps) {
    const t = useTranslations("admin.documents");

    const formatDate = (date?: string | null) => {
        if (!date) return "-";
        return new Date(date).toLocaleString("vi-VN", {
            day: "2-digit",
            month: "2-digit",
            year: "numeric",
        });
    };

    const columns: BaseTableColumn<DocumentDto>[] = [
        {
            key: "documentName",
            header: t("table.documentName"),
            className: "min-w-[200px]",
            sortable: true,
            render: (doc) => (
                <div className="font-medium text-foreground line-clamp-2">{doc.documentName}</div>
            ),
        },
        {
            key: "subject",
            header: t("table.subject"),
            className: "min-w-[120px]",
            render: (doc) => (
                <span className="text-sm text-slate-600 dark:text-slate-400">
                    {doc.subject?.subjectName || "-"}
                </span>
            ),
        },
        {
            key: "type",
            header: t("table.type"),
            className: "min-w-[100px]",
            render: (doc) => (
                <Badge variant="outline">{doc.type?.typeName || "-"}</Badge>
            ),
        },
        {
            key: "visibility",
            header: t("table.visibility"),
            className: "min-w-[80px]",
            render: (doc) => (
                <span className="text-sm">
                    {VisibilityLabels[doc.visibility ?? 0]}
                </span>
            ),
        },
        {
            key: "reviewStatus",
            header: t("table.reviewStatus"),
            className: "min-w-[100px]",
            sortable: true,
            render: (doc) => (
                <span className={`px-2 py-1 rounded-full text-xs font-medium ${ReviewStatusColors[doc.reviewStatus ?? 0]}`}>
                    {ReviewStatusLabels[doc.reviewStatus ?? 0]}
                </span>
            ),
        },
        {
            key: "fileCount",
            header: t("table.fileCount"),
            className: "min-w-[80px]",
            sortable: true,
            render: (doc) => (
                <Badge variant="secondary">{doc.fileCount || 0}</Badge>
            ),
        },
        {
            key: "createdAt",
            header: t("table.createdAt"),
            className: "min-w-[100px]",
            sortable: true,
            render: (doc) => (
                <span className="text-sm text-slate-600 dark:text-slate-400">
                    {formatDate(doc.createdAt)}
                </span>
            ),
        },
    ];

    // Add quick actions column if needed
    if (onViewDetail || onReview) {
        columns.push({
            key: "quickActions",
            header: "",
            className: "min-w-[80px]",
            render: (doc) => (
                <div className="flex items-center gap-1">
                    {onViewDetail && (
                        <Button
                            variant="ghost"
                            size="sm"
                            onClick={() => onViewDetail(doc)}
                            title={t("table.viewDetail")}
                        >
                            <Eye size={16} />
                        </Button>
                    )}
                    {onReview && doc.reviewStatus === 0 && (
                        <Button
                            variant="ghost"
                            size="sm"
                            onClick={() => onReview(doc)}
                            title={t("table.review")}
                            className="text-amber-600 hover:text-amber-700"
                        >
                            <CheckCircle size={16} />
                        </Button>
                    )}
                </div>
            ),
        });
    }

    return (
        <BaseTable
            data={documents}
            columns={columns}
            loading={loading}
            onEdit={onEdit}
            onDelete={onDelete}
            onBulkDelete={onBulkDelete}
            editLabel={t("table.edit")}
            deleteLabel={t("table.delete")}
            loadingLabel={t("table.loading")}
            noDataLabel={t("table.noData")}
            deleteSelectedLabel={t("table.deleteSelected")}
            onSort={onSort}
            sortKey={sortKey}
            sortDirection={sortDirection}
            enableClientSort={enableClientSort}
            showActions={!!onEdit || !!onDelete}
        />
    );
}

