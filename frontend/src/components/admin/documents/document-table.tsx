"use client";

import Link from "next/link";
import { useTranslations } from "next-intl";
import { BaseTable, BaseTableColumn } from "@/src/components/admin/tables/base-table";
import { Badge } from "@/src/components/ui/badge";
import { Eye } from "lucide-react";
import type { DocumentDto } from "@/src/api/database/types.gen";

// VisibilityStatus enum mapping
const VisibilityLabels: Record<number, string> = {
    0: "Private",
    1: "Public",
};

interface DocumentTableProps {
    documents: DocumentDto[];
    onEdit?: (document: DocumentDto) => void;
    onDelete?: (document: DocumentDto) => void;
    onBulkDelete?: (ids: string[]) => void | Promise<void>;
    loading?: boolean;
    onSort?: (sortKey: string, direction: "asc" | "desc" | null) => void;
    sortKey?: string | null;
    sortDirection?: "asc" | "desc" | null;
    enableClientSort?: boolean;
}

export function DocumentTable({
    documents,
    onEdit,
    onDelete,
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

    // Add view detail link
    columns.push({
        key: "quickActions",
        header: "",
        className: "min-w-[50px]",
        render: (doc) => (
            <Link
                href={`/documents/${doc.id}`}
                className="inline-flex items-center justify-center h-8 w-8 text-slate-600 hover:text-primary hover:bg-slate-100 dark:hover:bg-slate-800 transition-colors"
                title={t("table.viewDetail")}
            >
                <Eye size={16} />
            </Link>
        ),
    });

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
