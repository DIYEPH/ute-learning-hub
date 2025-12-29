"use client";

import { useTranslations } from "next-intl";
import { BaseTable, BaseTableColumn } from "@/src/components/admin/tables/base-table";
import type { ConversationDto } from "@/src/api/database/types.gen";
import { Badge } from "@/src/components/ui/badge";

// ConversationType enum mapping
const ConversationTypeLabels: Record<number, string> = {
    0: "Private",
    1: "Group",
    2: "AI",
};

const ConversationTypeColors: Record<number, string> = {
    0: "bg-purple-100 text-purple-700 dark:bg-purple-900 dark:text-purple-300",
    1: "bg-primary/20 text-primary dark:bg-primary/30 dark:text-primary",
    2: "bg-green-100 text-green-700 dark:bg-green-900 dark:text-green-300",
};

// ConversationStatus enum mapping
const ConversationStatusLabels: Record<number, string> = {
    0: "Active",
    1: "Inactive",
    2: "Archived",
};

const ConversationStatusColors: Record<number, string> = {
    0: "bg-green-100 text-green-700 dark:bg-green-900 dark:text-green-300",
    1: "bg-secondary text-secondary-foreground",
    2: "bg-amber-100 text-amber-700 dark:bg-amber-900 dark:text-amber-300",
};

interface ConversationTableProps {
    conversations: ConversationDto[];
    onEdit?: (conversation: ConversationDto) => void;
    onDelete?: (conversation: ConversationDto) => void;
    onBulkDelete?: (ids: string[]) => void | Promise<void>;
    loading?: boolean;
    onSort?: (sortKey: string, direction: "asc" | "desc" | null) => void;
    sortKey?: string | null;
    sortDirection?: "asc" | "desc" | null;
    enableClientSort?: boolean;
}

export function ConversationTable({
    conversations,
    onEdit,
    onDelete,
    onBulkDelete,
    loading,
    onSort,
    sortKey,
    sortDirection,
    enableClientSort,
}: ConversationTableProps) {
    const t = useTranslations("admin.conversations");

    const formatDate = (date?: string | null) => {
        if (!date) return "-";
        return new Date(date).toLocaleString("vi-VN", {
            day: "2-digit",
            month: "2-digit",
            year: "numeric",
            hour: "2-digit",
            minute: "2-digit",
        });
    };

    const columns: BaseTableColumn<ConversationDto>[] = [
        {
            key: "conversationName",
            header: t("table.name"),
            className: "min-w-[200px]",
            sortable: true,
            render: (conversation) => (
                <div className="font-medium text-foreground">{conversation.conversationName}</div>
            ),
        },
        {
            key: "conversationType",
            header: t("table.type"),
            className: "min-w-[100px]",
            sortable: true,
            render: (conversation) => (
                <span className={`px-2 py-1 rounded-full text-xs font-medium ${ConversationTypeColors[conversation.conversationType ?? 0]}`}>
                    {ConversationTypeLabels[conversation.conversationType ?? 0]}
                </span>
            ),
        },
        {
            key: "conversationStatus",
            header: t("table.status"),
            className: "min-w-[100px]",
            sortable: true,
            render: (conversation) => (
                <span className={`px-2 py-1 rounded-full text-xs font-medium ${ConversationStatusColors[conversation.conversationStatus ?? 0]}`}>
                    {ConversationStatusLabels[conversation.conversationStatus ?? 0]}
                </span>
            ),
        },
        {
            key: "subject",
            header: t("table.subject"),
            className: "min-w-[150px]",
            render: (conversation) => (
                <span className="text-sm text-muted-foreground">
                    {conversation.subject?.subjectName || "-"}
                </span>
            ),
        },
        {
            key: "memberCount",
            header: t("table.members"),
            className: "min-w-[80px]",
            sortable: true,
            render: (conversation) => (
                <Badge variant="outline">{conversation.memberCount || 0}</Badge>
            ),
        },
        {
            key: "createdAt",
            header: t("table.createdAt"),
            className: "min-w-[140px]",
            sortable: true,
            render: (conversation) => (
                <span className="text-sm text-muted-foreground">
                    {formatDate(conversation.createdAt)}
                </span>
            ),
        },
    ];

    return (
        <BaseTable
            data={conversations}
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


