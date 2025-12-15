"use client";

import { useState, useEffect, useCallback } from "react";
import { Button } from "@/src/components/ui/button";
import { Pagination } from "@/src/components/ui/pagination";
import { Plus, Trash2 } from "lucide-react";
import { useTranslations } from "next-intl";
import { useNotifications } from "@/src/hooks/use-notifications";
import { useNotification } from "@/src/components/providers/notification-provider";
import { NotificationTable } from "@/src/components/admin/notifications/notification-table";
import { NotificationForm } from "@/src/components/admin/notifications/notification-form";
import { CreateModal } from "@/src/components/admin/modals/create-modal";
import { EditModal } from "@/src/components/admin/modals/edit-modal";
import { DeleteModal } from "@/src/components/admin/modals/delete-modal";
import { AdvancedSearchFilter } from "@/src/components/admin/advanced-search-filter";
import type {
    NotificationDto,
    CreateNotificationCommand,
    UpdateNotificationRequest,
} from "@/src/api/database/types.gen";

export default function NotificationsManagementPage() {
    const t = useTranslations("admin.notifications");
    const notification = useNotification();
    const {
        fetchNotifications,
        createNotification,
        updateNotification,
        deleteNotification,
        loading,
        error,
    } = useNotifications();

    const [notifications, setNotifications] = useState<NotificationDto[]>([]);
    const [totalCount, setTotalCount] = useState(0);
    const [page, setPage] = useState(1);
    const [pageSize] = useState(10);
    const [searchTerm, setSearchTerm] = useState("");
    const [sortKey, setSortKey] = useState<string | null>(null);
    const [sortDirection, setSortDirection] = useState<"asc" | "desc" | null>(null);
    const [createModalOpen, setCreateModalOpen] = useState(false);
    const [editModalOpen, setEditModalOpen] = useState(false);
    const [deleteModalOpen, setDeleteModalOpen] = useState(false);
    const [selectedNotification, setSelectedNotification] = useState<NotificationDto | null>(null);
    const [formLoading, setFormLoading] = useState(false);
    const [deleteAllModalOpen, setDeleteAllModalOpen] = useState(false);
    const [priorityFilter, setPriorityFilter] = useState<string | null>(null);
    const [deletedFilter, setDeletedFilter] = useState<string | null>(null);

    const loadNotifications = useCallback(async () => {
        try {
            const response = await fetchNotifications({
                IsDeleted: deletedFilter === "true" ? true : deletedFilter === "false" ? false : undefined,
                Page: page,
                PageSize: pageSize,
            });

            if (response) {
                // Filter client-side since API may not support all filters
                let items = response.items || [];

                // Filter by searchTerm
                if (searchTerm) {
                    items = items.filter(
                        (item) =>
                            item.title?.toLowerCase().includes(searchTerm.toLowerCase()) ||
                            item.content?.toLowerCase().includes(searchTerm.toLowerCase())
                    );
                }

                // Filter by priority
                if (priorityFilter) {
                    items = items.filter((item) => item.notificationPriorityType === parseInt(priorityFilter));
                }

                setNotifications(items);
                setTotalCount(items.length);
            }
        } catch (err) {
            console.error("Error loading notifications:", err);
        }
    }, [fetchNotifications, searchTerm, priorityFilter, deletedFilter, page, pageSize]);

    useEffect(() => {
        loadNotifications();
    }, [loadNotifications]);

    const handleCreate = async (command: CreateNotificationCommand | UpdateNotificationRequest) => {
        setFormLoading(true);
        try {
            await createNotification(command as CreateNotificationCommand);
            await loadNotifications();
            setCreateModalOpen(false);
            notification.success(t("notifications.createSuccess"));
        } catch (err) {
            console.error("Error creating notification:", err);
            notification.error(t("notifications.createError"));
        } finally {
            setFormLoading(false);
        }
    };

    const handleEdit = async (command: CreateNotificationCommand | UpdateNotificationRequest) => {
        if (!selectedNotification?.id) return;
        setFormLoading(true);
        try {
            await updateNotification(selectedNotification.id, command as UpdateNotificationRequest);
            await loadNotifications();
            setEditModalOpen(false);
            setSelectedNotification(null);
            notification.success(t("notifications.updateSuccess"));
        } catch (err) {
            console.error("Error updating notification:", err);
            notification.error(t("notifications.updateError"));
        } finally {
            setFormLoading(false);
        }
    };

    const handleDelete = async () => {
        if (!selectedNotification?.id) return;
        setFormLoading(true);
        try {
            await deleteNotification(selectedNotification.id);
            await loadNotifications();
            setDeleteModalOpen(false);
            setSelectedNotification(null);
            notification.success(t("notifications.deleteSuccess"));
        } catch (err) {
            console.error("Error deleting notification:", err);
            notification.error(t("notifications.deleteError"));
        } finally {
            setFormLoading(false);
        }
    };

    const handleBulkDelete = async (ids: string[]) => {
        if (ids.length === 0) return;
        setFormLoading(true);
        try {
            await Promise.all(ids.map((id) => deleteNotification(id)));
            await loadNotifications();
            notification.success(t("notifications.bulkDeleteSuccess"));
        } catch (err) {
            console.error("Error bulk deleting notifications:", err);
            notification.error(t("notifications.deleteError"));
        } finally {
            setFormLoading(false);
        }
    };

    const openEditModal = (item: NotificationDto) => {
        setSelectedNotification(item);
        setEditModalOpen(true);
    };

    const openDeleteModal = (item: NotificationDto) => {
        setSelectedNotification(item);
        setDeleteModalOpen(true);
    };

    const handleSort = (key: string) => {
        if (sortKey === key) {
            setSortDirection(sortDirection === "asc" ? "desc" : sortDirection === "desc" ? null : "asc");
            if (sortDirection === "desc") setSortKey(null);
        } else {
            setSortKey(key);
            setSortDirection("asc");
        }
    };

    const handleReset = () => {
        setSearchTerm("");
        setPriorityFilter(null);
        setDeletedFilter("false"); // Keep default to active items
        setSortKey(null);
        setSortDirection(null);
        setPage(1);
    };

    const handleDeleteAll = async () => {
        if (notifications.length === 0) return;
        setFormLoading(true);
        try {
            await Promise.all(notifications.map((item) => deleteNotification(item.id!)));
            await loadNotifications();
            setDeleteAllModalOpen(false);
            notification.success(t("notifications.bulkDeleteSuccess"));
        } catch (err) {
            console.error("Error deleting all notifications:", err);
            notification.error(t("notifications.deleteError"));
        } finally {
            setFormLoading(false);
        }
    };

    const handleFilterChange = (key: string, value: string | null) => {
        if (key === "priority") {
            setPriorityFilter(value);
        } else if (key === "deleted") {
            setDeletedFilter(value);
        }
        setPage(1);
    };

    const totalPages = Math.ceil(totalCount / pageSize);

    return (
        <div>
            <div className="mb-6 flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
                <h1 className="text-xl md:text-2xl font-semibold text-foreground">{t("title")}</h1>
                <div className="flex gap-2">
                    {notifications.length > 0 && (
                        <Button
                            onClick={() => setDeleteAllModalOpen(true)}
                            variant="destructive"
                            size="sm"
                            className="text-xs sm:text-sm"
                        >
                            <Trash2 size={16} className="mr-1" />
                            {t("deleteAll")}
                        </Button>
                    )}
                    <Button onClick={() => setCreateModalOpen(true)} size="sm" className="text-xs sm:text-sm">
                        <Plus size={16} className="mr-1" />
                        {t("addNew")}
                    </Button>
                </div>
            </div>

            <div className="mb-4">
                <AdvancedSearchFilter
                    searchTerm={searchTerm}
                    onSearchChange={setSearchTerm}
                    placeholder={t("searchPlaceholder")}
                    filters={[
                        {
                            key: "priority",
                            label: t("filter.priority"),
                            type: "select",
                            value: priorityFilter,
                            options: [
                                { value: "0", label: t("filter.low") },
                                { value: "1", label: t("filter.normal") },
                                { value: "2", label: t("filter.high") },
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

            {/* Error display */}
            {error && (
                <div className="mb-4 p-4 bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 ">
                    <p className="text-sm text-red-600 dark:text-red-400">{error}</p>
                </div>
            )}

            {notifications.length > 0 && (
                <div className="mb-2 text-sm text-slate-600 dark:text-slate-400">
                    {t("foundCount", { count: totalCount })}
                </div>
            )}

            {/* Table */}
            <div className="mt-4">
                <NotificationTable
                    notifications={notifications}
                    loading={loading}
                    sortKey={sortKey}
                    sortDirection={sortDirection}
                    onSort={handleSort}
                    onEdit={openEditModal}
                    onDelete={openDeleteModal}
                    onBulkDelete={handleBulkDelete}
                />
            </div>

            {/* Pagination */}
            <Pagination
                currentPage={page}
                totalPages={totalPages}
                totalItems={totalCount}
                pageSize={pageSize}
                onPageChange={setPage}
                loading={loading}
                className="mt-4"
            />

            {/* Create Modal */}
            <CreateModal
                open={createModalOpen}
                onOpenChange={setCreateModalOpen}
                onSubmit={() => {
                    const form = document.getElementById("notification-form") as HTMLFormElement;
                    if (form) form.requestSubmit();
                }}
                title={t("createModal.title")}
                description={t("createModal.description")}
                loading={formLoading}
                size="lg"
            >
                <NotificationForm
                    onSubmit={handleCreate}
                    loading={formLoading}
                    isEditMode={false}
                />
            </CreateModal>

            {/* Edit Modal */}
            <EditModal
                open={editModalOpen}
                onOpenChange={(open) => {
                    setEditModalOpen(open);
                    if (!open) setSelectedNotification(null);
                }}
                onSubmit={() => {
                    const form = document.getElementById("notification-form") as HTMLFormElement;
                    if (form) form.requestSubmit();
                }}
                title={t("editModal.title")}
                description={t("editModal.description")}
                loading={formLoading}
                size="lg"
            >
                {selectedNotification && (
                    <NotificationForm
                        initialData={{
                            id: selectedNotification.id || undefined,
                            objectId: selectedNotification.objectId || undefined,
                            title: selectedNotification.title,
                            content: selectedNotification.content,
                            link: selectedNotification.link,
                            isGlobal: selectedNotification.isGlobal,
                            expiredAt: selectedNotification.expiredAt,
                            notificationType: selectedNotification.notificationType,
                            notificationPriorityType: selectedNotification.notificationPriorityType,
                        }}
                        onSubmit={handleEdit}
                        loading={formLoading}
                        isEditMode={true}
                    />
                )}
            </EditModal>

            {/* Delete Modal */}
            <DeleteModal
                open={deleteModalOpen}
                onOpenChange={(open) => {
                    setDeleteModalOpen(open);
                    if (!open) setSelectedNotification(null);
                }}
                onConfirm={handleDelete}
                title={t("deleteModal.title")}
                description={t("deleteModal.description")}
                loading={formLoading}
                itemName={selectedNotification?.title || ""}
            />

            {/* Delete All Modal */}
            <DeleteModal
                open={deleteAllModalOpen}
                onOpenChange={setDeleteAllModalOpen}
                onConfirm={handleDeleteAll}
                title={t("deleteAllModal.title")}
                description={t("deleteAllModal.description", { count: notifications.length })}
                loading={formLoading}
                itemName={`${notifications.length} ${t("items")}`}
            />
        </div>
    );
}


