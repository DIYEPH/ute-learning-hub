"use client";

import { useState, useEffect, useCallback } from "react";
import { Button } from "@/src/components/ui/button";
import { Pagination } from "@/src/components/ui/pagination";
import { Plus, Trash2 } from "lucide-react";
import { useTranslations } from "next-intl";
import { useTags, CreateTagCommand, UpdateTagCommand } from "@/src/hooks/use-tags";
import { useNotification } from "@/src/components/providers/notification-provider";
import { TagTable } from "@/src/components/admin/tags/tag-table";
import { TagForm } from "@/src/components/admin/tags/tag-form";
import { CreateModal } from "@/src/components/admin/modals/create-modal";
import { EditModal } from "@/src/components/admin/modals/edit-modal";
import { DeleteModal } from "@/src/components/admin/modals/delete-modal";
import { AdvancedSearchFilter } from "@/src/components/admin/advanced-search-filter";
import type { TagDto } from "@/src/api/database/types.gen";

export default function TagsManagementPage() {
    const t = useTranslations("admin.tags");
    const tCommon = useTranslations("common");
    const notification = useNotification();
    const {
        fetchTags,
        createTag,
        updateTag,
        deleteTag,
        loading,
        error,
    } = useTags();

    const [tags, setTags] = useState<TagDto[]>([]);
    const [totalCount, setTotalCount] = useState(0);
    const [page, setPage] = useState(1);
    const [pageSize] = useState(10);
    const [searchTerm, setSearchTerm] = useState("");
    const [deletedFilter, setDeletedFilter] = useState<string | null>(null);
    const [sortKey, setSortKey] = useState<string | null>(null);
    const [sortDirection, setSortDirection] = useState<"asc" | "desc" | null>(null);
    const [createModalOpen, setCreateModalOpen] = useState(false);
    const [editModalOpen, setEditModalOpen] = useState(false);
    const [deleteModalOpen, setDeleteModalOpen] = useState(false);
    const [selectedTag, setSelectedTag] = useState<TagDto | null>(null);
    const [formLoading, setFormLoading] = useState(false);

    const loadTags = useCallback(async () => {
        try {
            const response = await fetchTags({
                SearchTerm: searchTerm || undefined,
                IsDeleted: deletedFilter === "true" ? true : deletedFilter === "false" ? false : undefined,
                Page: page,
                PageSize: pageSize,
            });

            if (response) {
                setTags(response.items || []);
                setTotalCount(response.totalCount || 0);
            }
        } catch (err) {
            console.error("Error loading tags:", err);
        }
    }, [fetchTags, searchTerm, deletedFilter, page, pageSize]);

    useEffect(() => {
        loadTags();
    }, [loadTags]);

    const handleCreate = async (command: CreateTagCommand | UpdateTagCommand) => {
        setFormLoading(true);
        try {
            await createTag(command as CreateTagCommand);
            await loadTags();
            setCreateModalOpen(false);
            notification.success(t("notifications.createSuccess"));
        } catch (err) {
            console.error("Error creating tag:", err);
            notification.error(t("notifications.createError"));
        } finally {
            setFormLoading(false);
        }
    };

    const handleEdit = async (command: CreateTagCommand | UpdateTagCommand) => {
        if (!selectedTag?.id) return;
        setFormLoading(true);
        try {
            await updateTag(selectedTag.id, command as UpdateTagCommand);
            await loadTags();
            setEditModalOpen(false);
            setSelectedTag(null);
            notification.success(t("notifications.updateSuccess"));
        } catch (err) {
            console.error("Error updating tag:", err);
            notification.error(t("notifications.updateError"));
        } finally {
            setFormLoading(false);
        }
    };

    const handleDelete = async () => {
        if (!selectedTag?.id) return;
        setFormLoading(true);
        try {
            await deleteTag(selectedTag.id);
            await loadTags();
            setDeleteModalOpen(false);
            setSelectedTag(null);
            notification.success(t("notifications.deleteSuccess"));
        } catch (err) {
            console.error("Error deleting tag:", err);
            notification.error(t("notifications.deleteError"));
        } finally {
            setFormLoading(false);
        }
    };

    const handleBulkDelete = async (ids: string[]) => {
        setFormLoading(true);
        try {
            await Promise.all(ids.map((id) => deleteTag(id)));
            await loadTags();
            notification.success(t("notifications.bulkDeleteSuccess", { count: ids.length }));
        } catch (err) {
            console.error("Error bulk deleting tags:", err);
            notification.error(t("notifications.deleteError"));
        } finally {
            setFormLoading(false);
        }
    };

    const handleDeleteAll = async () => {
        if (tags.length === 0) return;
        if (!confirm(t("deleteAllConfirm", { count: tags.length }))) return;

        setFormLoading(true);
        try {
            const ids = tags.map((t) => t.id).filter((id): id is string => !!id);
            await Promise.all(ids.map((id) => deleteTag(id)));
            await loadTags();
        } catch (err) {
            console.error("Error deleting all tags:", err);
        } finally {
            setFormLoading(false);
        }
    };

    const handleSearch = () => {
        setPage(1);
        loadTags();
    };

    const handleReset = () => {
        setSearchTerm("");
        setDeletedFilter(null);
        setSortKey(null);
        setSortDirection(null);
        setPage(1);
    };

    const handleFilterChange = (key: string, value: string | null) => {
        if (key === "deleted") {
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
                    {tags.length > 0 && (
                        <Button
                            onClick={handleDeleteAll}
                            variant="destructive"
                            size="sm"
                            className="text-xs sm:text-sm"
                            disabled={formLoading}
                        >
                            <Trash2 size={16} className="mr-1" />
                            {t("deleteAll")}
                        </Button>
                    )}
                    <Button onClick={() => setCreateModalOpen(true)} size="sm" className="text-xs sm:text-sm">
                        <Plus size={16} className="mr-1" />
                        {tCommon("create")}
                    </Button>
                </div>
            </div>

            <div className="mb-4">
                <AdvancedSearchFilter
                    searchTerm={searchTerm}
                    onSearchChange={setSearchTerm}
                    onSearchSubmit={handleSearch}
                    filters={[
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
                    placeholder={t("searchPlaceholder")}
                />
            </div>

            {error && (
                <div className="mb-4 p-2 text-sm text-red-600 dark:text-red-400 bg-red-50 dark:bg-red-950 rounded">
                    {error}
                </div>
            )}

            {tags.length > 0 && (
                <div className="mb-2 text-sm text-slate-600 dark:text-slate-400">
                    {t("foundCount", { count: totalCount })}
                </div>
            )}

            <TagTable
                tags={tags}
                onEdit={(tag) => {
                    setSelectedTag(tag);
                    setEditModalOpen(true);
                }}
                onDelete={(tag) => {
                    setSelectedTag(tag);
                    setDeleteModalOpen(true);
                }}
                onBulkDelete={handleBulkDelete}
                loading={loading}
                onSort={handleSort}
                sortKey={sortKey}
                sortDirection={sortDirection}
                enableClientSort={true}
            />

            <Pagination
                currentPage={page}
                totalPages={totalPages}
                totalItems={totalCount}
                pageSize={pageSize}
                onPageChange={setPage}
                loading={loading}
                className="mt-4"
            />

            <CreateModal
                open={createModalOpen}
                onOpenChange={setCreateModalOpen}
                title={t("createTitle")}
                onSubmit={async () => {
                    const form = document.getElementById("tag-form") as HTMLFormElement;
                    if (form) {
                        form.requestSubmit();
                    }
                }}
                loading={formLoading}
            >
                <TagForm
                    onSubmit={handleCreate}
                    loading={formLoading}
                />
            </CreateModal>

            <EditModal
                open={editModalOpen}
                onOpenChange={(open) => {
                    setEditModalOpen(open);
                    if (!open) setSelectedTag(null);
                }}
                title={t("editTitle")}
                onSubmit={async () => {
                    const form = document.getElementById("tag-form") as HTMLFormElement;
                    if (form) {
                        form.requestSubmit();
                    }
                }}
                loading={formLoading}
            >
                <TagForm
                    initialData={selectedTag || undefined}
                    onSubmit={handleEdit}
                    loading={formLoading}
                />
            </EditModal>

            <DeleteModal
                open={deleteModalOpen}
                onOpenChange={(open) => {
                    setDeleteModalOpen(open);
                    if (!open) setSelectedTag(null);
                }}
                itemName={selectedTag?.tagName}
                onConfirm={handleDelete}
                loading={formLoading}
            />
        </div>
    );
}


