"use client";

import { useState, useEffect, useCallback } from "react";
import { Button } from "@/src/components/ui/button";
import { Pagination } from "@/src/components/ui/pagination";
import { Plus, Trash2 } from "lucide-react";
import { useTranslations } from "next-intl";
import { useAuthors } from "@/src/hooks/use-authors";
import { useNotification } from "@/src/components/providers/notification-provider";
import { AuthorTable } from "@/src/components/admin/authors/author-table";
import { AuthorForm } from "@/src/components/admin/authors/author-form";
import { DeleteModal } from "@/src/components/admin/modals/delete-modal";
import { EditModal } from "@/src/components/admin/modals/edit-modal";
import { CreateModal } from "@/src/components/admin/modals/create-modal";
import { AdvancedSearchFilter } from "@/src/components/admin/advanced-search-filter";
import type { AuthorListDto, AuthorInput, UpdateAuthorCommand } from "@/src/api/database/types.gen";

export default function AuthorsManagementPage() {
    const t = useTranslations("admin.authors");
    const notification = useNotification();
    const { fetchAuthors, createAuthor, updateAuthor, deleteAuthor, loading, error } = useAuthors();

    const [authors, setAuthors] = useState<AuthorListDto[]>([]);
    const [totalCount, setTotalCount] = useState(0);
    const [page, setPage] = useState(1);
    const [pageSize] = useState(10);
    const [searchTerm, setSearchTerm] = useState("");
    const [sortKey, setSortKey] = useState<string | null>(null);
    const [sortDirection, setSortDirection] = useState<"asc" | "desc" | null>(null);

    const [createModalOpen, setCreateModalOpen] = useState(false);
    const [editModalOpen, setEditModalOpen] = useState(false);
    const [deleteModalOpen, setDeleteModalOpen] = useState(false);
    const [deleteAllModalOpen, setDeleteAllModalOpen] = useState(false);
    const [selectedAuthor, setSelectedAuthor] = useState<AuthorListDto | null>(null);
    const [formLoading, setFormLoading] = useState(false);

    const loadAuthors = useCallback(async () => {
        try {
            const response = await fetchAuthors({
                SearchTerm: searchTerm || undefined,
                Page: page,
                PageSize: pageSize,
            });

            if (response) {
                setAuthors(response.items || []);
                setTotalCount(response.totalCount || 0);
            }
        } catch (err) {
            console.error("Error loading authors:", err);
        }
    }, [fetchAuthors, searchTerm, page, pageSize]);

    useEffect(() => {
        loadAuthors();
    }, [loadAuthors]);

    const handleCreate = async (data: AuthorInput) => {
        setFormLoading(true);
        try {
            await createAuthor(data);
            await loadAuthors();
            setCreateModalOpen(false);
            notification.success(t("notifications.createSuccess"));
        } catch (err) {
            console.error("Error creating author:", err);
            notification.error(t("notifications.createError"));
        } finally {
            setFormLoading(false);
        }
    };

    const handleEdit = async (data: UpdateAuthorCommand) => {
        if (!selectedAuthor?.id) return;
        setFormLoading(true);
        try {
            await updateAuthor(selectedAuthor.id, data);
            await loadAuthors();
            setEditModalOpen(false);
            setSelectedAuthor(null);
            notification.success(t("notifications.updateSuccess"));
        } catch (err) {
            console.error("Error updating author:", err);
            notification.error(t("notifications.updateError"));
        } finally {
            setFormLoading(false);
        }
    };

    const handleDelete = async () => {
        if (!selectedAuthor?.id) return;
        setFormLoading(true);
        try {
            await deleteAuthor(selectedAuthor.id);
            await loadAuthors();
            setDeleteModalOpen(false);
            setSelectedAuthor(null);
            notification.success(t("notifications.deleteSuccess"));
        } catch (err) {
            console.error("Error deleting author:", err);
            notification.error(t("notifications.deleteError"));
        } finally {
            setFormLoading(false);
        }
    };

    const handleBulkDelete = async (ids: string[]) => {
        setFormLoading(true);
        try {
            await Promise.all(ids.map((id) => deleteAuthor(id)));
            await loadAuthors();
            notification.success(t("notifications.bulkDeleteSuccess", { count: ids.length }));
        } catch (err) {
            console.error("Error bulk deleting authors:", err);
            notification.error(t("notifications.deleteError"));
        } finally {
            setFormLoading(false);
        }
    };

    const handleDeleteAll = async () => {
        if (authors.length === 0) return;
        setFormLoading(true);
        try {
            const ids = authors.map((a) => a.id).filter((id): id is string => !!id);
            await Promise.all(ids.map((id) => deleteAuthor(id)));
            await loadAuthors();
            setDeleteAllModalOpen(false);
            notification.success(t("notifications.bulkDeleteSuccess", { count: ids.length }));
        } catch (err) {
            console.error("Error deleting all authors:", err);
            notification.error(t("notifications.deleteError"));
        } finally {
            setFormLoading(false);
        }
    };

    const handleReset = () => {
        setSearchTerm("");
        setSortKey(null);
        setSortDirection(null);
        setPage(1);
    };

    const handleSort = (key: string, direction: "asc" | "desc" | null) => {
        setSortKey(key || null);
        setSortDirection(direction);
    };

    const totalPages = Math.ceil(totalCount / pageSize);

    return (
        <div>
            <div className="mb-6 flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
                <h1 className="text-xl md:text-2xl font-semibold text-foreground">{t("title")}</h1>
                <div className="flex gap-2">
                    <Button
                        onClick={() => setCreateModalOpen(true)}
                        size="sm"
                        className="text-xs sm:text-sm"
                        disabled={formLoading}
                    >
                        <Plus size={16} className="mr-1" />
                        {t("createTitle")}
                    </Button>
                    {authors.length > 0 && (
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
                    filters={[]}
                    onFilterChange={() => { }}
                    onReset={handleReset}
                />
            </div>

            {error && (
                <div className="mb-4 p-4 bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 ">
                    <p className="text-sm text-red-600 dark:text-red-400">{error}</p>
                </div>
            )}

            {authors.length > 0 && (
                <div className="mb-2 text-sm text-slate-600 dark:text-slate-400">
                    {t("foundCount", { count: totalCount })}
                </div>
            )}

            <div className="mt-4">
                <AuthorTable
                    authors={authors}
                    loading={loading}
                    onEdit={(author) => {
                        setSelectedAuthor(author);
                        setEditModalOpen(true);
                    }}
                    onDelete={(author) => {
                        setSelectedAuthor(author);
                        setDeleteModalOpen(true);
                    }}
                    onBulkDelete={handleBulkDelete}
                    onSort={handleSort}
                    sortKey={sortKey}
                    sortDirection={sortDirection}
                    enableClientSort={true}
                />
            </div>
            <Pagination currentPage={page} totalPages={totalPages} totalItems={totalCount} pageSize={pageSize} onPageChange={setPage} loading={loading} className="mt-4" />
            <CreateModal
                open={createModalOpen}
                onOpenChange={setCreateModalOpen}
                title={t("createTitle")}
                onSubmit={async () => {
                    const form = document.getElementById("author-form") as HTMLFormElement;
                    if (form) {
                        form.requestSubmit();
                    }
                }}
                loading={formLoading}
            >
                <AuthorForm
                    onSubmit={(data) => handleCreate(data as AuthorInput)}
                    loading={formLoading}
                />
            </CreateModal>

            <EditModal
                open={editModalOpen}
                onOpenChange={(open) => {
                    setEditModalOpen(open);
                    if (!open) setSelectedAuthor(null);
                }}
                title={t("editTitle")}
                onSubmit={async () => {
                    const form = document.getElementById("author-form") as HTMLFormElement;
                    if (form) {
                        form.requestSubmit();
                    }
                }}
                loading={formLoading}
            >
                <AuthorForm
                    initialData={selectedAuthor as import("@/src/components/admin/authors/author-form").AuthorFormData || undefined}
                    onSubmit={handleEdit}
                    loading={formLoading}
                />
            </EditModal>

            <DeleteModal
                open={deleteModalOpen}
                onOpenChange={(open) => {
                    setDeleteModalOpen(open);
                    if (!open) setSelectedAuthor(null);
                }}
                onConfirm={handleDelete}
                title={t("deleteModal.title")}
                description={t("deleteModal.description")}
                loading={formLoading}
                itemName={selectedAuthor?.fullName || ""}
            />

            <DeleteModal
                open={deleteAllModalOpen}
                onOpenChange={setDeleteAllModalOpen}
                onConfirm={handleDeleteAll}
                title={t("deleteAllModal.title")}
                description={t("deleteAllModal.description", { count: authors.length })}
                loading={formLoading}
                itemName={`${authors.length} ${t("items")}`}
            />
        </div>
    );
}

