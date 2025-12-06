"use client";

import React, { useState, useEffect, useCallback, ReactNode } from "react";
import { Button } from "@/src/components/ui/button";
import { Pagination } from "@/src/components/ui/pagination";
import { Plus, Upload, Trash2 } from "lucide-react";
import { useTranslations } from "next-intl";
import { CreateModal } from "./modals/create-modal";
import { EditModal } from "./modals/edit-modal";
import { DeleteModal } from "./modals/delete-modal";
import { ImportModal } from "./modals/import-modal";
import { AdvancedSearchFilter, type FilterOption } from "./advanced-search-filter";

// ============ Types ============

export interface AdminCrudPageProps<TItem, TCreate, TUpdate> {
    /** Translation namespace for this page */
    translationNamespace: string;

    /** Unique identifier for the form (used for form submission) */
    formId: string;

    /** Page size for pagination */
    pageSize?: number;

    /** Items to display */
    items: TItem[];

    /** Total count for pagination */
    totalCount: number;

    /** Loading state */
    loading: boolean;

    /** Error message */
    error: string | null;

    /** Filters configuration */
    filters?: FilterOption[];

    /** Function to get item ID */
    getItemId: (item: TItem) => string | undefined;

    /** Function to get item name for delete confirmation */
    getItemName: (item: TItem) => string | undefined;

    /** Table component to render items */
    renderTable: (props: {
        items: TItem[];
        loading: boolean;
        onEdit: (item: TItem) => void;
        onDelete: (item: TItem) => void;
        onBulkDelete: (ids: string[]) => void;
        onSort?: (key: string, direction: "asc" | "desc" | null) => void;
        sortKey?: string | null;
        sortDirection?: "asc" | "desc" | null;
    }) => ReactNode;

    /** Form component for create/edit */
    renderForm: (props: {
        initialData?: TItem;
        onSubmit: (data: TCreate | TUpdate) => void;
        loading: boolean;
    }) => ReactNode;

    /** Optional import form component */
    renderImportForm?: (props: {
        onImport: (file: File) => void;
        loading: boolean;
    }) => ReactNode;

    // CRUD callbacks
    onFetch: (params: { page: number; pageSize: number; searchTerm?: string } & Record<string, unknown>) => Promise<void>;
    onCreate: (data: TCreate) => Promise<void>;
    onUpdate: (id: string, data: TUpdate) => Promise<void>;
    onDelete: (id: string) => Promise<void>;
    onBulkDelete?: (ids: string[]) => Promise<void>;
    onImport?: (file: File) => Promise<void>;

    // Filter change callback
    onFilterChange?: (key: string, value: unknown) => void;

    /** Modal size for create/edit */
    modalSize?: "sm" | "md" | "lg" | "xl";

    /** Show import button */
    showImport?: boolean;

    /** Show delete all button */
    showDeleteAll?: boolean;

    /** Custom header actions */
    headerActions?: ReactNode;
}

// ============ Component ============

export function AdminCrudPage<TItem, TCreate, TUpdate>({
    translationNamespace,
    formId,
    pageSize = 10,
    items,
    totalCount,
    loading,
    error,
    filters = [],
    getItemId,
    getItemName,
    renderTable,
    renderForm,
    renderImportForm,
    onFetch,
    onCreate,
    onUpdate,
    onDelete,
    onBulkDelete,
    onImport,
    onFilterChange,
    modalSize = "md",
    showImport = true,
    showDeleteAll = true,
    headerActions,
}: AdminCrudPageProps<TItem, TCreate, TUpdate>) {
    const t = useTranslations(translationNamespace);
    const tCommon = useTranslations("common");

    // Pagination & Search state
    const [page, setPage] = useState(1);
    const [searchTerm, setSearchTerm] = useState("");

    // Sort state
    const [sortKey, setSortKey] = useState<string | null>(null);
    const [sortDirection, setSortDirection] = useState<"asc" | "desc" | null>(null);

    // Modal states
    const [createModalOpen, setCreateModalOpen] = useState(false);
    const [editModalOpen, setEditModalOpen] = useState(false);
    const [deleteModalOpen, setDeleteModalOpen] = useState(false);
    const [importModalOpen, setImportModalOpen] = useState(false);

    // Selected item for edit/delete
    const [selectedItem, setSelectedItem] = useState<TItem | null>(null);

    // Form loading state (separate from list loading)
    const [formLoading, setFormLoading] = useState(false);

    // Load data
    const loadData = useCallback(async () => {
        await onFetch({
            page,
            pageSize,
            searchTerm: searchTerm || undefined,
        });
    }, [onFetch, page, pageSize, searchTerm]);

    useEffect(() => {
        loadData();
    }, [loadData]);

    // Handlers
    const handleCreate = async (data: TCreate | TUpdate) => {
        setFormLoading(true);
        try {
            await onCreate(data as TCreate);
            await loadData();
            setCreateModalOpen(false);
        } catch (err) {
            console.error("Error creating:", err);
        } finally {
            setFormLoading(false);
        }
    };

    const handleEdit = async (data: TCreate | TUpdate) => {
        const id = selectedItem ? getItemId(selectedItem) : undefined;
        if (!id) return;

        setFormLoading(true);
        try {
            await onUpdate(id, data as TUpdate);
            await loadData();
            setEditModalOpen(false);
            setSelectedItem(null);
        } catch (err) {
            console.error("Error updating:", err);
        } finally {
            setFormLoading(false);
        }
    };

    const handleDelete = async () => {
        const id = selectedItem ? getItemId(selectedItem) : undefined;
        if (!id) return;

        setFormLoading(true);
        try {
            await onDelete(id);
            await loadData();
            setDeleteModalOpen(false);
            setSelectedItem(null);
        } catch (err) {
            console.error("Error deleting:", err);
        } finally {
            setFormLoading(false);
        }
    };

    const handleBulkDelete = async (ids: string[]) => {
        if (!onBulkDelete) {
            // Fallback: delete one by one
            setFormLoading(true);
            try {
                await Promise.all(ids.map((id) => onDelete(id)));
                await loadData();
            } catch (err) {
                console.error("Error bulk deleting:", err);
            } finally {
                setFormLoading(false);
            }
            return;
        }

        setFormLoading(true);
        try {
            await onBulkDelete(ids);
            await loadData();
        } catch (err) {
            console.error("Error bulk deleting:", err);
        } finally {
            setFormLoading(false);
        }
    };

    const handleDeleteAll = async () => {
        if (items.length === 0) return;

        const confirmMessage = t("deleteAllConfirm", { count: items.length });
        if (!confirm(confirmMessage)) return;

        const ids = items.map(getItemId).filter((id): id is string => !!id);
        await handleBulkDelete(ids);
    };

    const handleImport = async (file: File) => {
        if (!onImport) return;

        setFormLoading(true);
        try {
            await onImport(file);
            await loadData();
            setImportModalOpen(false);
        } catch (err) {
            console.error("Error importing:", err);
        } finally {
            setFormLoading(false);
        }
    };

    const handleSearch = () => {
        setPage(1);
        loadData();
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

    const handleFilterChange = (key: string, value: unknown) => {
        onFilterChange?.(key, value);
        setPage(1);
    };

    const totalPages = Math.ceil(totalCount / pageSize);

    return (
        <div className="p-4 md:p-6">
            {/* Header */}
            <div className="mb-6 flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
                <h1 className="text-xl md:text-2xl font-semibold text-foreground">
                    {t("title")}
                </h1>
                <div className="flex gap-2">
                    {showImport && onImport && (
                        <Button
                            onClick={() => setImportModalOpen(true)}
                            variant="outline"
                            size="sm"
                            className="text-xs sm:text-sm"
                        >
                            <Upload size={16} className="mr-1" />
                            {t("import")}
                        </Button>
                    )}
                    {showDeleteAll && items.length > 0 && (
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
                    <Button
                        onClick={() => setCreateModalOpen(true)}
                        size="sm"
                        className="text-xs sm:text-sm"
                    >
                        <Plus size={16} className="mr-1" />
                        {tCommon("create")}
                    </Button>
                    {headerActions}
                </div>
            </div>

            {/* Search & Filters */}
            <div className="mb-4">
                <AdvancedSearchFilter
                    searchTerm={searchTerm}
                    onSearchChange={setSearchTerm}
                    onSearchSubmit={handleSearch}
                    filters={filters}
                    onFilterChange={handleFilterChange}
                    onReset={handleReset}
                    placeholder={t("searchPlaceholder")}
                />
            </div>

            {/* Error */}
            {error && (
                <div className="mb-4 p-2 text-sm text-red-600 dark:text-red-400 bg-red-50 dark:bg-red-950 rounded">
                    {error}
                </div>
            )}

            {/* Count */}
            {items.length > 0 && (
                <div className="mb-2 text-sm text-slate-600 dark:text-slate-400">
                    {t("foundCount", { count: totalCount })}
                </div>
            )}

            {/* Table */}
            {renderTable({
                items,
                loading,
                onEdit: (item) => {
                    setSelectedItem(item);
                    setEditModalOpen(true);
                },
                onDelete: (item) => {
                    setSelectedItem(item);
                    setDeleteModalOpen(true);
                },
                onBulkDelete: handleBulkDelete,
                onSort: handleSort,
                sortKey,
                sortDirection,
            })}

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
                title={t("createTitle")}
                onSubmit={async () => {
                    const form = document.getElementById(formId) as HTMLFormElement;
                    if (form) form.requestSubmit();
                }}
                loading={formLoading}
                size={modalSize}
            >
                {renderForm({
                    onSubmit: handleCreate,
                    loading: formLoading,
                })}
            </CreateModal>

            {/* Edit Modal */}
            <EditModal
                open={editModalOpen}
                onOpenChange={(open) => {
                    setEditModalOpen(open);
                    if (!open) setSelectedItem(null);
                }}
                title={t("editTitle")}
                onSubmit={async () => {
                    const form = document.getElementById(formId) as HTMLFormElement;
                    if (form) form.requestSubmit();
                }}
                loading={formLoading}
                size={modalSize}
            >
                {renderForm({
                    initialData: selectedItem || undefined,
                    onSubmit: handleEdit,
                    loading: formLoading,
                })}
            </EditModal>

            {/* Delete Modal */}
            <DeleteModal
                open={deleteModalOpen}
                onOpenChange={(open) => {
                    setDeleteModalOpen(open);
                    if (!open) setSelectedItem(null);
                }}
                itemName={selectedItem ? getItemName(selectedItem) : undefined}
                onConfirm={handleDelete}
                loading={formLoading}
            />

            {/* Import Modal */}
            {showImport && onImport && (
                <ImportModal
                    open={importModalOpen}
                    onOpenChange={setImportModalOpen}
                    title={t("importTitle")}
                    description={t("importDescription")}
                    onImport={handleImport}
                    loading={formLoading}
                >
                    {renderImportForm?.({
                        onImport: handleImport,
                        loading: formLoading,
                    })}
                </ImportModal>
            )}
        </div>
    );
}
