"use client";

import { useTranslations } from "next-intl";
import { BaseTable, BaseTableColumn } from "@/src/components/admin/tables/base-table";
import type { TagDto } from "@/src/api/database/types.gen";

interface TagTableProps {
    tags: TagDto[];
    onEdit?: (tag: TagDto) => void;
    onDelete?: (tag: TagDto) => void;
    onBulkDelete?: (ids: string[]) => void | Promise<void>;
    loading?: boolean;
    onSort?: (sortKey: string, direction: "asc" | "desc" | null) => void;
    sortKey?: string | null;
    sortDirection?: "asc" | "desc" | null;
    enableClientSort?: boolean;
}

export function TagTable({
    tags,
    onEdit,
    onDelete,
    onBulkDelete,
    loading,
    onSort,
    sortKey,
    sortDirection,
    enableClientSort,
}: TagTableProps) {
    const t = useTranslations("admin.tags");

    const columns: BaseTableColumn<TagDto>[] = [
        {
            key: "tagName",
            header: t("table.tagName"),
            className: "min-w-[200px]",
            sortable: true,
            render: (tag) => (
                <div className="font-medium text-foreground">{tag.tagName}</div>
            ),
        },
    ];

    return (
        <BaseTable
            data={tags}
            columns={columns}
            loading={loading}
            onEdit={onEdit}
            onDelete={onDelete}
            onBulkDelete={onBulkDelete}
            editLabel={t("table.edit")}
            deleteLabel={t("table.delete")}
            loadingLabel={t("table.loading")}
            noDataLabel={t("table.noData")}
            selectedCountLabel={t("table.selectedCount", { count: 0 })}
            deleteSelectedLabel={t("table.deleteSelected")}
            onSort={onSort}
            sortKey={sortKey}
            sortDirection={sortDirection}
            enableClientSort={enableClientSort}
        />
    );
}

