"use client";

import { useState, useEffect, useCallback } from "react";
import { Button } from "@/src/components/ui/button";
import { Pagination } from "@/src/components/ui/pagination";
import { Trash2 } from "lucide-react";
import { useTranslations } from "next-intl";
import { useConversations } from "@/src/hooks/use-conversations";
import { useNotification } from "@/src/components/ui/notification-center";
import { ConversationTable } from "@/src/components/admin/conversations/conversation-table";
import { DeleteModal } from "@/src/components/admin/modals/delete-modal";
import { AdvancedSearchFilter } from "@/src/components/admin/advanced-search-filter";
import type { ConversationDto } from "@/src/api/database/types.gen";

export default function ConversationsManagementPage() {
    const t = useTranslations("admin.conversations");
    const notification = useNotification();
    const { fetchConversations, deleteConversation, loading, error } = useConversations();

    const [conversations, setConversations] = useState<ConversationDto[]>([]);
    const [totalCount, setTotalCount] = useState(0);
    const [page, setPage] = useState(1);
    const [pageSize] = useState(10);
    const [searchTerm, setSearchTerm] = useState("");
    const [typeFilter, setTypeFilter] = useState<string | null>(null);
    const [statusFilter, setStatusFilter] = useState<string | null>(null);
    const [deletedFilter, setDeletedFilter] = useState<string | null>(null);
    const [sortKey, setSortKey] = useState<string | null>(null);
    const [sortDirection, setSortDirection] = useState<"asc" | "desc" | null>(null);
    const [deleteModalOpen, setDeleteModalOpen] = useState(false);
    const [deleteAllModalOpen, setDeleteAllModalOpen] = useState(false);
    const [selectedConversation, setSelectedConversation] = useState<ConversationDto | null>(null);
    const [formLoading, setFormLoading] = useState(false);

    const loadConversations = useCallback(async () => {
        try {
            const response = await fetchConversations({
                SearchTerm: searchTerm || undefined,
                ConversationType: typeFilter || undefined,
                ConversationStatus: statusFilter || undefined,
                IsDeleted: deletedFilter === "true" ? true : deletedFilter === "false" ? false : undefined,
                Page: page,
                PageSize: pageSize,
            });

            if (response) {
                setConversations(response.items || []);
                setTotalCount(response.totalCount || 0);
            }
        } catch (err) {
            console.error("Error loading conversations:", err);
        }
    }, [fetchConversations, searchTerm, typeFilter, statusFilter, deletedFilter, page, pageSize]);

    useEffect(() => {
        loadConversations();
    }, [loadConversations]);

    const handleDelete = async () => {
        if (!selectedConversation?.id) return;
        setFormLoading(true);
        try {
            await deleteConversation(selectedConversation.id);
            await loadConversations();
            setDeleteModalOpen(false);
            setSelectedConversation(null);
            notification.success(t("notifications.deleteSuccess"));
        } catch (err) {
            console.error("Error deleting conversation:", err);
            notification.error(t("notifications.deleteError"));
        } finally {
            setFormLoading(false);
        }
    };

    const handleBulkDelete = async (ids: string[]) => {
        setFormLoading(true);
        try {
            await Promise.all(ids.map((id) => deleteConversation(id)));
            await loadConversations();
            notification.success(t("notifications.bulkDeleteSuccess", { count: ids.length }));
        } catch (err) {
            console.error("Error bulk deleting conversations:", err);
            notification.error(t("notifications.deleteError"));
        } finally {
            setFormLoading(false);
        }
    };

    const handleDeleteAll = async () => {
        if (conversations.length === 0) return;
        setFormLoading(true);
        try {
            const ids = conversations.map((c) => c.id).filter((id): id is string => !!id);
            await Promise.all(ids.map((id) => deleteConversation(id)));
            await loadConversations();
            setDeleteAllModalOpen(false);
            notification.success(t("notifications.bulkDeleteSuccess", { count: ids.length }));
        } catch (err) {
            console.error("Error deleting all conversations:", err);
            notification.error(t("notifications.deleteError"));
        } finally {
            setFormLoading(false);
        }
    };

    const handleReset = () => {
        setSearchTerm("");
        setTypeFilter(null);
        setStatusFilter(null);
        setDeletedFilter(null);
        setSortKey(null);
        setSortDirection(null);
        setPage(1);
    };

    const handleFilterChange = (key: string, value: string | null) => {
        if (key === "type") {
            setTypeFilter(value);
        } else if (key === "status") {
            setStatusFilter(value);
        } else if (key === "deleted") {
            setDeletedFilter(value);
        }
        setPage(1);
    };

    const handleSort = (key: string, direction: "asc" | "desc" | null) => {
        setSortKey(key || null);
        setSortDirection(direction);
    };

    const totalPages = Math.ceil(totalCount / pageSize);

    return (
        <div className="p-4 md:p-6">
            <div className="mb-6 flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
                <h1 className="text-xl md:text-2xl font-semibold text-foreground">{t("title")}</h1>
                <div className="flex gap-2">
                    {conversations.length > 0 && (
                        <Button
                            onClick={() => setDeleteAllModalOpen(true)}
                            variant="destructive"
                            size="sm"
                            className="text-xs sm:text-sm"
                            disabled={formLoading}
                        >
                            <Trash2 size={16} className="mr-1" />
                            {t("deleteAll")}
                        </Button>
                    )}
                </div>
            </div>

            <div className="mb-4">
                <AdvancedSearchFilter
                    searchTerm={searchTerm}
                    onSearchChange={setSearchTerm}
                    placeholder={t("searchPlaceholder")}
                    filters={[
                        {
                            key: "type",
                            label: t("filter.type"),
                            type: "select",
                            value: typeFilter,
                            options: [
                                { value: "0", label: "Private" },
                                { value: "1", label: "Group" },
                                { value: "2", label: "AI" },
                            ],
                        },
                        {
                            key: "status",
                            label: t("filter.status"),
                            type: "select",
                            value: statusFilter,
                            options: [
                                { value: "0", label: t("filter.active") },
                                { value: "1", label: t("filter.inactive") },
                                { value: "2", label: t("filter.archived") },
                            ],
                        },
                        {
                            key: "deleted",
                            label: t("filter.deleted"),
                            type: "select",
                            value: deletedFilter,
                            options: [
                                { value: "false", label: t("filter.activeItems") },
                                { value: "true", label: t("filter.deletedItems") },
                            ],
                        },
                    ]}
                    onFilterChange={handleFilterChange}
                    onReset={handleReset}
                />
            </div>

            {error && (
                <div className="mb-4 p-4 bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-md">
                    <p className="text-sm text-red-600 dark:text-red-400">{error}</p>
                </div>
            )}

            {conversations.length > 0 && (
                <div className="mb-2 text-sm text-slate-600 dark:text-slate-400">
                    {t("foundCount", { count: totalCount })}
                </div>
            )}

            <div className="mt-4">
                <ConversationTable
                    conversations={conversations}
                    loading={loading}
                    onDelete={(conversation) => {
                        setSelectedConversation(conversation);
                        setDeleteModalOpen(true);
                    }}
                    onBulkDelete={handleBulkDelete}
                    onSort={handleSort}
                    sortKey={sortKey}
                    sortDirection={sortDirection}
                    enableClientSort={true}
                />
            </div>

            <Pagination
                currentPage={page}
                totalPages={totalPages}
                totalItems={totalCount}
                pageSize={pageSize}
                onPageChange={setPage}
                loading={loading}
                className="mt-4"
            />

            {/* Delete Modal */}
            <DeleteModal
                open={deleteModalOpen}
                onOpenChange={(open) => {
                    setDeleteModalOpen(open);
                    if (!open) setSelectedConversation(null);
                }}
                onConfirm={handleDelete}
                title={t("deleteModal.title")}
                description={t("deleteModal.description")}
                loading={formLoading}
                itemName={selectedConversation?.conversationName || ""}
            />

            {/* Delete All Modal */}
            <DeleteModal
                open={deleteAllModalOpen}
                onOpenChange={setDeleteAllModalOpen}
                onConfirm={handleDeleteAll}
                title={t("deleteAllModal.title")}
                description={t("deleteAllModal.description", { count: conversations.length })}
                loading={formLoading}
                itemName={`${conversations.length} ${t("items")}`}
            />
        </div>
    );
}
