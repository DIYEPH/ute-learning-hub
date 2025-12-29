"use client";

import { useTranslations } from "next-intl";
import { BaseTable, BaseTableColumn } from "@/src/components/admin/tables/base-table";
import type { AuthorDto } from "@/src/api/database/types.gen";

interface AuthorTableProps {
    authors: AuthorDto[];
    onEdit?: (author: AuthorDto) => void;
    onDelete?: (author: AuthorDto) => void;
    onBulkDelete?: (ids: string[]) => void | Promise<void>;
    loading?: boolean;
    onSort?: (sortKey: string, direction: "asc" | "desc" | null) => void;
    sortKey?: string | null;
    sortDirection?: "asc" | "desc" | null;
    enableClientSort?: boolean;
}

export function AuthorTable({
    authors,
    onEdit,
    onDelete,
    onBulkDelete,
    loading,
    onSort,
    sortKey,
    sortDirection,
    enableClientSort,
}: AuthorTableProps) {
    const t = useTranslations("admin.authors");

    const columns: BaseTableColumn<AuthorDto>[] = [
        {
            key: "fullName",
            header: t("table.fullName"),
            className: "min-w-[200px]",
            sortable: true,
            render: (author) => (
                <div className="font-medium text-foreground">{author.fullName}</div>
            ),
        },
        {
            key: "description",
            header: t("table.description"),
            className: "min-w-[300px]",
            render: (author) => (
                <span className="text-sm text-muted-foreground line-clamp-2">
                    {author.description || "-"}
                </span>
            ),
        },
    ];

    return (
        <BaseTable
            data={authors}
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

