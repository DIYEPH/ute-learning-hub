"use client";

import { useState, useEffect, useCallback } from "react";
import { Button } from "@/src/components/ui/button";
import { Pagination } from "@/src/components/ui/pagination";
import { Trash2 } from "lucide-react";
import { useTranslations } from "next-intl";
import { useDocuments } from "@/src/hooks/use-documents";
import { useNotification } from "@/src/components/ui/notification-center";
import { DocumentTable } from "@/src/components/admin/documents/document-table";
import { DocumentForm } from "@/src/components/admin/documents/document-form";
import { DocumentDetailModal } from "@/src/components/admin/documents/document-detail-modal";
import { ReviewModal } from "@/src/components/admin/documents/review-modal";
import { DeleteModal } from "@/src/components/admin/modals/delete-modal";
import { EditModal } from "@/src/components/admin/modals/edit-modal";
import { AdvancedSearchFilter } from "@/src/components/admin/advanced-search-filter";
import type { DocumentDto, UpdateDocumentCommand, ReviewDocumentCommand } from "@/src/api/database/types.gen";

export default function DocumentsManagementPage() {
    const t = useTranslations("admin.documents");
    const notification = useNotification();
    const { fetchDocuments, updateDocument, deleteDocument, reviewDocument, loading, error } = useDocuments();

    const [documents, setDocuments] = useState<DocumentDto[]>([]);
    const [totalCount, setTotalCount] = useState(0);
    const [page, setPage] = useState(1);
    const [pageSize] = useState(10);
    const [searchTerm, setSearchTerm] = useState("");
    const [visibilityFilter, setVisibilityFilter] = useState<string | null>(null);
    const [reviewStatusFilter, setReviewStatusFilter] = useState<string | null>(null);
    const [deletedFilter, setDeletedFilter] = useState<string | null>(null);
    const [sortKey, setSortKey] = useState<string | null>(null);
    const [sortDirection, setSortDirection] = useState<"asc" | "desc" | null>(null);

    const [detailModalOpen, setDetailModalOpen] = useState(false);
    const [editModalOpen, setEditModalOpen] = useState(false);
    const [deleteModalOpen, setDeleteModalOpen] = useState(false);
    const [reviewModalOpen, setReviewModalOpen] = useState(false);
    const [deleteAllModalOpen, setDeleteAllModalOpen] = useState(false);
    const [selectedDocument, setSelectedDocument] = useState<DocumentDto | null>(null);
    const [formLoading, setFormLoading] = useState(false);

    const loadDocuments = useCallback(async () => {
        try {
            const response = await fetchDocuments({
                SearchTerm: searchTerm || undefined,
                IsDeleted: deletedFilter === "true" ? true : deletedFilter === "false" ? false : undefined,
                Page: page,
                PageSize: pageSize,
            });

            if (response) {
                setDocuments(response.items || []);
                setTotalCount(response.totalCount || 0);
            }
        } catch (err) {
            console.error("Error loading documents:", err);
        }
    }, [fetchDocuments, searchTerm, visibilityFilter, reviewStatusFilter, deletedFilter, page, pageSize]);

    useEffect(() => {
        loadDocuments();
    }, [loadDocuments]);

    const handleEdit = async (command: UpdateDocumentCommand) => {
        if (!selectedDocument?.id) return;
        setFormLoading(true);
        try {
            await updateDocument(selectedDocument.id, command);
            await loadDocuments();
            setEditModalOpen(false);
            setSelectedDocument(null);
            notification.success(t("notifications.updateSuccess"));
        } catch (err) {
            console.error("Error updating document:", err);
            notification.error(t("notifications.updateError"));
        } finally {
            setFormLoading(false);
        }
    };

    const handleReview = async (command: ReviewDocumentCommand) => {
        setFormLoading(true);
        try {
            const success = await reviewDocument(command);
            if (success) {
                await loadDocuments();
                setReviewModalOpen(false);
                setSelectedDocument(null);
                notification.success(t("notifications.reviewSuccess"));
            } else {
                notification.error(t("notifications.reviewError"));
            }
        } catch (err) {
            console.error("Error reviewing document:", err);
            notification.error(t("notifications.reviewError"));
        } finally {
            setFormLoading(false);
        }
    };

    const handleDelete = async () => {
        if (!selectedDocument?.id) return;
        setFormLoading(true);
        try {
            await deleteDocument(selectedDocument.id);
            await loadDocuments();
            setDeleteModalOpen(false);
            setSelectedDocument(null);
            notification.success(t("notifications.deleteSuccess"));
        } catch (err) {
            console.error("Error deleting document:", err);
            notification.error(t("notifications.deleteError"));
        } finally {
            setFormLoading(false);
        }
    };

    const handleBulkDelete = async (ids: string[]) => {
        setFormLoading(true);
        try {
            await Promise.all(ids.map((id) => deleteDocument(id)));
            await loadDocuments();
            notification.success(t("notifications.bulkDeleteSuccess", { count: ids.length }));
        } catch (err) {
            console.error("Error bulk deleting documents:", err);
            notification.error(t("notifications.deleteError"));
        } finally {
            setFormLoading(false);
        }
    };

    const handleDeleteAll = async () => {
        if (documents.length === 0) return;
        setFormLoading(true);
        try {
            const ids = documents.map((d) => d.id).filter((id): id is string => !!id);
            await Promise.all(ids.map((id) => deleteDocument(id)));
            await loadDocuments();
            setDeleteAllModalOpen(false);
            notification.success(t("notifications.bulkDeleteSuccess", { count: ids.length }));
        } catch (err) {
            console.error("Error deleting all documents:", err);
            notification.error(t("notifications.deleteError"));
        } finally {
            setFormLoading(false);
        }
    };

    const handleReset = () => {
        setSearchTerm("");
        setVisibilityFilter(null);
        setReviewStatusFilter(null);
        setDeletedFilter(null);
        setSortKey(null);
        setSortDirection(null);
        setPage(1);
    };

    const handleFilterChange = (key: string, value: string | null) => {
        if (key === "visibility") {
            setVisibilityFilter(value);
        } else if (key === "reviewStatus") {
            setReviewStatusFilter(value);
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
                    {documents.length > 0 && (
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
                            key: "visibility",
                            label: t("filter.visibility"),
                            type: "select",
                            value: visibilityFilter,
                            options: [
                                { value: "0", label: t("filter.private") },
                                { value: "1", label: t("filter.public") },
                            ],
                        },
                        {
                            key: "reviewStatus",
                            label: t("filter.reviewStatus"),
                            type: "select",
                            value: reviewStatusFilter,
                            options: [
                                { value: "0", label: t("filter.pending") },
                                { value: "1", label: t("filter.hidden") },
                                { value: "2", label: t("filter.approved") },
                                { value: "3", label: t("filter.rejected") },
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

            {documents.length > 0 && (
                <div className="mb-2 text-sm text-slate-600 dark:text-slate-400">
                    {t("foundCount", { count: totalCount })}
                </div>
            )}

            <div className="mt-4">
                <DocumentTable
                    documents={documents}
                    loading={loading}
                    onViewDetail={(doc) => {
                        setSelectedDocument(doc);
                        setDetailModalOpen(true);
                    }}
                    onEdit={(doc) => {
                        setSelectedDocument(doc);
                        setEditModalOpen(true);
                    }}
                    onDelete={(doc) => {
                        setSelectedDocument(doc);
                        setDeleteModalOpen(true);
                    }}
                    onReview={(doc) => {
                        setSelectedDocument(doc);
                        setReviewModalOpen(true);
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

            {/* Detail Modal */}
            <DocumentDetailModal
                open={detailModalOpen}
                onOpenChange={(open) => {
                    setDetailModalOpen(open);
                    if (!open) setSelectedDocument(null);
                }}
                documentId={selectedDocument?.id || null}
            />

            {/* Edit Modal */}
            <EditModal
                open={editModalOpen}
                onOpenChange={(open) => {
                    setEditModalOpen(open);
                    if (!open) setSelectedDocument(null);
                }}
                title={t("editTitle")}
                onSubmit={async () => {
                    const form = document.getElementById("document-form") as HTMLFormElement;
                    if (form) {
                        form.requestSubmit();
                    }
                }}
                loading={formLoading}
                size="lg"
            >
                <DocumentForm
                    initialData={selectedDocument as import("@/src/components/admin/documents/document-form").DocumentFormData || undefined}
                    onSubmit={handleEdit}
                    loading={formLoading}
                />
            </EditModal>

            {/* Review Modal */}
            <ReviewModal
                open={reviewModalOpen}
                onOpenChange={(open) => {
                    setReviewModalOpen(open);
                    if (!open) setSelectedDocument(null);
                }}
                documentId={selectedDocument?.id || null}
                documentName={selectedDocument?.documentName || ""}
                onSubmit={handleReview}
                loading={formLoading}
            />

            {/* Delete Modal */}
            <DeleteModal
                open={deleteModalOpen}
                onOpenChange={(open) => {
                    setDeleteModalOpen(open);
                    if (!open) setSelectedDocument(null);
                }}
                onConfirm={handleDelete}
                title={t("deleteModal.title")}
                description={t("deleteModal.description")}
                loading={formLoading}
                itemName={selectedDocument?.documentName || ""}
            />

            {/* Delete All Modal */}
            <DeleteModal
                open={deleteAllModalOpen}
                onOpenChange={setDeleteAllModalOpen}
                onConfirm={handleDeleteAll}
                title={t("deleteAllModal.title")}
                description={t("deleteAllModal.description", { count: documents.length })}
                loading={formLoading}
                itemName={`${documents.length} ${t("items")}`}
            />
        </div>
    );
}
