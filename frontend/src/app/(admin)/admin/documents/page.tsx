"use client";

import { useState, useEffect, useCallback } from "react";
import { Button } from "@/src/components/ui/button";
import { Pagination } from "@/src/components/ui/pagination";
import { Trash2 } from "lucide-react";
import { useTranslations } from "next-intl";
import { useDocuments } from "@/src/hooks/use-documents";
import { useSubjects } from "@/src/hooks/use-subjects";
import { useTypes } from "@/src/hooks/use-types";
import { getApiAuthor, getApiTag } from "@/src/api";
import { useNotification } from "@/src/components/providers/notification-provider";
import { DocumentTable } from "@/src/components/admin/documents/document-table";
import { DocumentForm } from "@/src/components/admin/documents/document-form";
import { DocumentDetailModal } from "@/src/components/admin/documents/document-detail-modal";
import { DeleteModal } from "@/src/components/admin/modals/delete-modal";
import { EditModal } from "@/src/components/admin/modals/edit-modal";
import { AdvancedSearchFilter } from "@/src/components/admin/advanced-search-filter";
import type { DocumentDto, UpdateDocumentCommandRequest, SubjectDetailDto, TypeDto, AuthorDto, TagDto } from "@/src/api/database/types.gen";

export default function DocumentsManagementPage() {
    const t = useTranslations("admin.documents");
    const notification = useNotification();
    const { fetchDocuments, updateDocument, deleteDocument, loading, error } = useDocuments();
    const { fetchSubjects } = useSubjects();
    const { fetchTypes } = useTypes();

    const [documents, setDocuments] = useState<DocumentDto[]>([]);
    const [totalCount, setTotalCount] = useState(0);
    const [page, setPage] = useState(1);
    const [pageSize] = useState(10);
    const [searchTerm, setSearchTerm] = useState("");
    const [visibilityFilter, setVisibilityFilter] = useState<string | null>(null);
    const [deletedFilter, setDeletedFilter] = useState<string | null>(null);
    const [subjectFilter, setSubjectFilter] = useState<string | null>(null);
    const [typeFilter, setTypeFilter] = useState<string | null>(null);
    const [authorFilter, setAuthorFilter] = useState<string | null>(null);
    const [tagFilter, setTagFilter] = useState<string[]>([]);
    const [sortKey, setSortKey] = useState<string | null>(null);
    const [sortDirection, setSortDirection] = useState<"asc" | "desc" | null>(null);

    // Filter options
    const [subjects, setSubjects] = useState<SubjectDetailDto[]>([]);
    const [types, setTypes] = useState<TypeDto[]>([]);
    const [authors, setAuthors] = useState<AuthorDto[]>([]);
    const [tags, setTags] = useState<TagDto[]>([]);

    const [editModalOpen, setEditModalOpen] = useState(false);
    const [deleteModalOpen, setDeleteModalOpen] = useState(false);
    const [deleteAllModalOpen, setDeleteAllModalOpen] = useState(false);
    const [detailModalOpen, setDetailModalOpen] = useState(false);
    const [selectedDocument, setSelectedDocument] = useState<DocumentDto | null>(null);
    const [formLoading, setFormLoading] = useState(false);

    // Fetch filter options on mount
    useEffect(() => {
        const loadFilterOptions = async () => {
            const [subjectsRes, typesRes, authorsRes, tagsRes] = await Promise.all([
                fetchSubjects({ Page: 1, PageSize: 1000 }),
                fetchTypes({ Page: 1, PageSize: 1000 }),
                getApiAuthor({ query: { Page: 1, PageSize: 1000 } }).then((res: any) => res?.data || res),
                getApiTag({ query: { Page: 1, PageSize: 1000 } }).then((res: any) => res?.data || res),
            ]);
            if (subjectsRes?.items) setSubjects(subjectsRes.items);
            if (typesRes?.items) setTypes(typesRes.items);
            if (authorsRes?.items) setAuthors(authorsRes.items);
            if (tagsRes?.items) setTags(tagsRes.items);
        };
        loadFilterOptions();
    }, [fetchSubjects, fetchTypes]);

    const loadDocuments = useCallback(async () => {
        try {
            const response = await fetchDocuments({
                SearchTerm: searchTerm || undefined,
                Visibility: visibilityFilter || undefined,
                SubjectId: subjectFilter || undefined,
                TypeId: typeFilter || undefined,
                AuthorId: authorFilter || undefined,
                TagIds: tagFilter.length > 0 ? tagFilter : undefined,
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
    }, [fetchDocuments, searchTerm, visibilityFilter, subjectFilter, typeFilter, authorFilter, tagFilter, deletedFilter, page, pageSize]);

    useEffect(() => {
        loadDocuments();
    }, [loadDocuments]);

    const handleEdit = async (command: UpdateDocumentCommandRequest) => {
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
        setDeletedFilter(null);
        setSubjectFilter(null);
        setTypeFilter(null);
        setAuthorFilter(null);
        setTagFilter([]);
        setSortKey(null);
        setSortDirection(null);
        setPage(1);
    };

    const handleFilterChange = (key: string, value: string | null) => {
        if (key === "visibility") setVisibilityFilter(value);
        else if (key === "deleted") setDeletedFilter(value);
        else if (key === "subject") setSubjectFilter(value);
        else if (key === "type") setTypeFilter(value);
        else if (key === "author") setAuthorFilter(value);
        else if (key === "tags") setTagFilter(Array.isArray(value) ? value : value ? [value] : []);
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
                            key: "subject",
                            label: t("filter.subject"),
                            type: "select",
                            value: subjectFilter,
                            options: subjects.filter(s => s.id).map(s => ({ value: s.id!, label: `${s.subjectName} (${s.subjectCode})` })),
                        },
                        {
                            key: "type",
                            label: t("filter.type"),
                            type: "select",
                            value: typeFilter,
                            options: types.filter(t => t.id).map(t => ({ value: t.id!, label: t.typeName || "" })),
                        },
                        {
                            key: "author",
                            label: t("filter.author"),
                            type: "searchable",
                            value: authorFilter,
                            options: authors.filter(a => a.id).map(a => ({ value: a.id!, label: a.fullName || "" })),
                        },
                        {
                            key: "tags",
                            label: t("filter.tags"),
                            type: "searchable-multiselect",
                            value: tagFilter,
                            options: tags.filter(tag => tag.id).map(tag => ({ value: tag.id!, label: tag.tagName || "" })),
                        },
                        {
                            key: "visibility",
                            label: t("filter.visibility"),
                            type: "select",
                            value: visibilityFilter,
                            options: [
                                { value: "0", label: t("filter.public") },
                                { value: "1", label: t("filter.internal") },
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
                <div className="mb-4 p-4 bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 ">
                    <p className="text-sm text-red-600 dark:text-red-400">{error}</p>
                </div>
            )}

            {documents.length > 0 && (
                <div className="mb-2 text-sm text-muted-foreground">
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

            {/* Document Detail Modal with Review */}
            <DocumentDetailModal
                open={detailModalOpen}
                onOpenChange={(open) => {
                    setDetailModalOpen(open);
                    if (!open) setSelectedDocument(null);
                }}
                documentId={selectedDocument?.id || null}
                onReviewSuccess={loadDocuments}
            />
        </div>
    );
}


