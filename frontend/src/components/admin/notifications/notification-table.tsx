"use client";

import { useTranslations } from "next-intl";
import { BaseTable, BaseTableColumn } from "@/src/components/admin/tables/base-table";
import type { NotificationDto } from "@/src/api/database/types.gen";
import { Badge } from "@/src/components/ui/badge";

const PriorityLabels: Record<number, string> = { 0: "Low", 1: "Normal", 2: "High" };
const PriorityColors: Record<number, string> = { 0: "bg-secondary text-secondary-foreground", 1: "bg-primary/10 text-primary", 2: "bg-red-100 text-red-700 dark:bg-red-900 dark:text-red-300" };

interface NotificationTableProps {
    notifications: NotificationDto[];
    onEdit?: (notification: NotificationDto) => void;
    onDelete?: (notification: NotificationDto) => void;
    onBulkDelete?: (ids: string[]) => void | Promise<void>;
    loading?: boolean;
    onSort?: (sortKey: string, direction: "asc" | "desc" | null) => void;
    sortKey?: string | null;
    sortDirection?: "asc" | "desc" | null;
    enableClientSort?: boolean;
}

export function NotificationTable({ notifications, onEdit, onDelete, onBulkDelete, loading, onSort, sortKey, sortDirection, enableClientSort }: NotificationTableProps) {
    const t = useTranslations("admin.notifications");
    const formatDate = (date?: string | null) => date ? new Date(date).toLocaleString("vi-VN", { day: "2-digit", month: "2-digit", year: "numeric", hour: "2-digit", minute: "2-digit" }) : "-";

    const columns: BaseTableColumn<NotificationDto>[] = [
        { key: "title", header: t("table.title"), className: "min-w-[200px]", sortable: true, render: n => <div className="font-medium text-foreground">{n.title}</div> },
        { key: "notificationPriorityType", header: t("table.priority"), className: "min-w-[80px]", sortable: true, render: n => <span className={`px-2 py-1 rounded-full text-xs font-medium ${PriorityColors[n.notificationPriorityType ?? 0]}`}>{PriorityLabels[n.notificationPriorityType ?? 0]}</span> },
        { key: "isGlobal", header: t("table.isGlobal"), className: "min-w-[80px]", render: n => <Badge variant={n.isGlobal ? "default" : "secondary"}>{n.isGlobal ? t("table.yes") : t("table.no")}</Badge> },
        { key: "createdAt", header: t("table.createdAt"), className: "min-w-[140px]", sortable: true, render: n => <span className="text-sm text-muted-foreground">{formatDate(n.createdAt)}</span> },
        { key: "expiredAt", header: t("table.expiredAt"), className: "min-w-[140px]", sortable: true, render: n => <span className="text-sm text-muted-foreground">{formatDate(n.expiredAt)}</span> },
    ];

    return (
        <BaseTable
            data={notifications}
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
        />
    );
}