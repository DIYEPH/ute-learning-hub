"use client";

import { useState, useEffect, useCallback } from "react";
import { Button } from "@/src/components/ui/button";
import { Pagination } from "@/src/components/ui/pagination";
import { Plus, Upload, Trash2 } from "lucide-react";
import { useTranslations } from "next-intl";
import { useFaculties } from "@/src/hooks/use-faculties";
import { useNotification } from "@/src/components/providers/notification-provider";
import { FacultyTable } from "@/src/components/admin/faculties/faculty-table";
import { FacultyForm } from "@/src/components/admin/faculties/faculty-form";
import { CreateModal } from "@/src/components/admin/modals/create-modal";
import { EditModal } from "@/src/components/admin/modals/edit-modal";
import { DeleteModal } from "@/src/components/admin/modals/delete-modal";
import { ImportModal } from "@/src/components/admin/modals/import-modal";
import { AdvancedSearchFilter } from "@/src/components/admin/advanced-search-filter";
import type {
  FacultyDto2,
  CreateFacultyCommand,
  UpdateFacultyCommand,
} from "@/src/api/database/types.gen";

export default function FacultiesManagementPage() {
  const t = useTranslations("admin.faculties");
  const tCommon = useTranslations("common");
  const notification = useNotification();
  const {
    fetchFaculties,
    createFaculty,
    updateFaculty,
    deleteFaculty,
    uploadLogo,
    loading,
    error,
    uploadingLogo,
    uploadError,
  } = useFaculties();

  const [faculties, setFaculties] = useState<FacultyDto2[]>([]);
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
  const [importModalOpen, setImportModalOpen] = useState(false);
  const [selectedFaculty, setSelectedFaculty] = useState<FacultyDto2 | null>(null);
  const [formLoading, setFormLoading] = useState(false);
  const [importLoading, setImportLoading] = useState(false);

  const loadFaculties = useCallback(async () => {
    try {
      const response = await fetchFaculties({
        SearchTerm: searchTerm || undefined,
        IsDeleted: deletedFilter === "true" ? true : deletedFilter === "false" ? false : undefined,
        Page: page,
        PageSize: pageSize,
      });

      if (response) {
        setFaculties(response.items || []);
        setTotalCount(response.totalCount || 0);
      }
    } catch (err) {
      console.error("Error loading faculties:", err);
    }
  }, [fetchFaculties, searchTerm, deletedFilter, page, pageSize]);

  useEffect(() => {
    loadFaculties();
  }, [loadFaculties]);

  const handleCreate = async (command: CreateFacultyCommand | UpdateFacultyCommand) => {
    setFormLoading(true);
    try {
      await createFaculty(command as CreateFacultyCommand);
      await loadFaculties();
      setCreateModalOpen(false);
      notification.success(t("notifications.createSuccess"));
    } catch (err) {
      console.error("Error creating faculty:", err);
      notification.error(t("notifications.createError"));
    } finally {
      setFormLoading(false);
    }
  };

  const handleEdit = async (command: CreateFacultyCommand | UpdateFacultyCommand) => {
    if (!selectedFaculty?.id) return;
    setFormLoading(true);
    try {
      await updateFaculty(selectedFaculty.id, command as UpdateFacultyCommand);
      await loadFaculties();
      setEditModalOpen(false);
      setSelectedFaculty(null);
      notification.success(t("notifications.updateSuccess"));
    } catch (err) {
      console.error("Error updating faculty:", err);
      notification.error(t("notifications.updateError"));
    } finally {
      setFormLoading(false);
    }
  };

  const handleDelete = async () => {
    if (!selectedFaculty?.id) return;
    setFormLoading(true);
    try {
      await deleteFaculty(selectedFaculty.id);
      await loadFaculties();
      setDeleteModalOpen(false);
      setSelectedFaculty(null);
      notification.success(t("notifications.deleteSuccess"));
    } catch (err) {
      console.error("Error deleting faculty:", err);
      notification.error(t("notifications.deleteError"));
    } finally {
      setFormLoading(false);
    }
  };

  const handleBulkDelete = async (ids: string[]) => {
    setFormLoading(true);
    try {
      await Promise.all(ids.map((id) => deleteFaculty(id)));
      await loadFaculties();
    } catch (err) {
      console.error("Error bulk deleting faculties:", err);
    } finally {
      setFormLoading(false);
    }
  };

  const handleImport = async (file: File) => {
    setImportLoading(true);
    try {
      setImportModalOpen(false);
      await loadFaculties();
    } catch (err) {
      console.error("Error importing faculties:", err);
    } finally {
      setImportLoading(false);
    }
  };

  const handleDeleteAll = async () => {
    if (faculties.length === 0) return;
    if (!confirm(t("deleteAllConfirm", { count: faculties.length }))) return;

    setFormLoading(true);
    try {
      const ids = faculties.map((f) => f.id).filter((id): id is string => !!id);
      await Promise.all(ids.map((id) => deleteFaculty(id)));
      await loadFaculties();
    } catch (err) {
      console.error("Error deleting all faculties:", err);
    } finally {
      setFormLoading(false);
    }
  };

  const handleSearch = () => {
    setPage(1);
    loadFaculties();
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
          <Button onClick={() => setImportModalOpen(true)} variant="outline" size="sm" className="text-xs sm:text-sm">
            <Upload size={16} className="mr-1" />
            {t("import")}
          </Button>
          {faculties.length > 0 && (
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

      {faculties.length > 0 && (
        <div className="mb-2 text-sm text-slate-600 dark:text-slate-400">
          {t("foundCount", { count: totalCount })}
        </div>
      )}

      <FacultyTable
        faculties={faculties}
        onEdit={(faculty) => {
          setSelectedFaculty(faculty);
          setEditModalOpen(true);
        }}
        onDelete={(faculty) => {
          setSelectedFaculty(faculty);
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
          const form = document.getElementById("faculty-form") as HTMLFormElement;
          if (form) {
            form.requestSubmit();
          }
        }}
        loading={formLoading}
      >
        <FacultyForm
          onSubmit={handleCreate}
          loading={formLoading}
          onUploadLogo={uploadLogo}
          uploadingLogo={uploadingLogo}
          uploadError={uploadError}
        />
      </CreateModal>

      <EditModal
        open={editModalOpen}
        onOpenChange={(open) => {
          setEditModalOpen(open);
          if (!open) setSelectedFaculty(null);
        }}
        title={t("editTitle")}
        onSubmit={async () => {
          const form = document.getElementById("faculty-form") as HTMLFormElement;
          if (form) {
            form.requestSubmit();
          }
        }}
        loading={formLoading}
      >
        <FacultyForm
          initialData={selectedFaculty || undefined}
          onSubmit={handleEdit}
          loading={formLoading}
          onUploadLogo={uploadLogo}
          uploadingLogo={uploadingLogo}
          uploadError={uploadError}
        />
      </EditModal>

      <DeleteModal
        open={deleteModalOpen}
        onOpenChange={(open) => {
          setDeleteModalOpen(open);
          if (!open) setSelectedFaculty(null);
        }}
        itemName={selectedFaculty?.facultyName}
        onConfirm={handleDelete}
        loading={formLoading}
      />

      <ImportModal
        open={importModalOpen}
        onOpenChange={setImportModalOpen}
        title={t("importTitle")}
        description={t("importDescription")}
        onImport={handleImport}
        loading={importLoading}
      />
    </div>
  );
}


