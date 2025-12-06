"use client";

import { useState, useEffect, useCallback } from "react";
import { Button } from "@/src/components/ui/button";
import { Pagination } from "@/src/components/ui/pagination";
import { Plus, Upload, Trash2 } from "lucide-react";
import { useTranslations } from "next-intl";
import { useMajors } from "@/src/hooks/use-majors";
import { useFaculties } from "@/src/hooks/use-faculties";
import { useNotification } from "@/src/components/ui/notification-center";
import { MajorTable } from "@/src/components/admin/majors/major-table";
import { MajorForm } from "@/src/components/admin/majors/major-form";
import { CreateModal } from "@/src/components/admin/modals/create-modal";
import { EditModal } from "@/src/components/admin/modals/edit-modal";
import { DeleteModal } from "@/src/components/admin/modals/delete-modal";
import { ImportModal } from "@/src/components/admin/modals/import-modal";
import { AdvancedSearchFilter, type FilterOption } from "@/src/components/admin/advanced-search-filter";
import type {
  MajorDto2,
  CreateMajorCommand,
  UpdateMajorCommand,
  FacultyDto2,
} from "@/src/api/database/types.gen";

export default function MajorsManagementPage() {
  const t = useTranslations("admin.majors");
  const tCommon = useTranslations("common");
  const notification = useNotification();
  const {
    fetchMajors,
    createMajor,
    updateMajor,
    deleteMajor,
    loading,
    error,
  } = useMajors();
  const { fetchFaculties } = useFaculties();

  const [majors, setMajors] = useState<MajorDto2[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [page, setPage] = useState(1);
  const [pageSize] = useState(10);
  const [searchTerm, setSearchTerm] = useState("");

  // Filter states
  const [facultyId, setFacultyId] = useState<string | null>(null);
  const [deletedFilter, setDeletedFilter] = useState<string | null>(null);
  const [faculties, setFaculties] = useState<FacultyDto2[]>([]);
  const [sortKey, setSortKey] = useState<string | null>(null);
  const [sortDirection, setSortDirection] = useState<"asc" | "desc" | null>(null);
  const [createModalOpen, setCreateModalOpen] = useState(false);
  const [editModalOpen, setEditModalOpen] = useState(false);
  const [deleteModalOpen, setDeleteModalOpen] = useState(false);
  const [importModalOpen, setImportModalOpen] = useState(false);
  const [selectedMajor, setSelectedMajor] = useState<MajorDto2 | null>(null);
  const [formLoading, setFormLoading] = useState(false);
  const [importLoading, setImportLoading] = useState(false);

  const loadMajors = useCallback(async () => {
    try {
      const response = await fetchMajors({
        SearchTerm: searchTerm || undefined,
        FacultyId: facultyId || undefined,
        IsDeleted: deletedFilter === "true" ? true : deletedFilter === "false" ? false : undefined,
        Page: page,
        PageSize: pageSize,
      });

      if (response) {
        setMajors(response.items || []);
        setTotalCount(response.totalCount || 0);
      }
    } catch (err) {
      console.error("Error loading majors:", err);
    }
  }, [fetchMajors, searchTerm, facultyId, deletedFilter, page, pageSize]);

  useEffect(() => {
    const loadFacultiesForFilter = async () => {
      try {
        const response = await fetchFaculties({ Page: 1, PageSize: 1000 });
        if (response?.items) {
          setFaculties(response.items);
        }
      } catch (err) {
        console.error("Error loading faculties:", err);
      }
    };
    loadFacultiesForFilter();
  }, [fetchFaculties]);

  useEffect(() => {
    loadMajors();
  }, [loadMajors]);

  useEffect(() => {
    const timer = setTimeout(() => {
      loadMajors();
    }, 300); // Debounce 300ms
    return () => clearTimeout(timer);
  }, [facultyId]);

  const handleCreate = async (command: CreateMajorCommand | UpdateMajorCommand) => {
    setFormLoading(true);
    try {
      await createMajor(command as CreateMajorCommand);
      await loadMajors();
      setCreateModalOpen(false);
      notification.success(t("notifications.createSuccess"));
    } catch (err) {
      console.error("Error creating major:", err);
      notification.error(t("notifications.createError"));
    } finally {
      setFormLoading(false);
    }
  };

  const handleEdit = async (command: CreateMajorCommand | UpdateMajorCommand) => {
    if (!selectedMajor?.id) return;
    setFormLoading(true);
    try {
      await updateMajor(selectedMajor.id, command as UpdateMajorCommand);
      await loadMajors();
      setEditModalOpen(false);
      setSelectedMajor(null);
      notification.success(t("notifications.updateSuccess"));
    } catch (err) {
      console.error("Error updating major:", err);
      notification.error(t("notifications.updateError"));
    } finally {
      setFormLoading(false);
    }
  };

  const handleDelete = async () => {
    if (!selectedMajor?.id) return;
    setFormLoading(true);
    try {
      await deleteMajor(selectedMajor.id);
      await loadMajors();
      setDeleteModalOpen(false);
      setSelectedMajor(null);
      notification.success(t("notifications.deleteSuccess"));
    } catch (err) {
      console.error("Error deleting major:", err);
      notification.error(t("notifications.deleteError"));
    } finally {
      setFormLoading(false);
    }
  };

  const handleBulkDelete = async (ids: string[]) => {
    setFormLoading(true);
    try {
      await Promise.all(ids.map((id) => deleteMajor(id)));
      await loadMajors();
    } catch (err) {
      console.error("Error bulk deleting majors:", err);
    } finally {
      setFormLoading(false);
    }
  };

  const handleDeleteAll = async () => {
    if (majors.length === 0) return;
    if (!confirm(t("deleteAllConfirm", { count: majors.length }))) return;

    setFormLoading(true);
    try {
      const ids = majors.map((m) => m.id).filter((id): id is string => !!id);
      await Promise.all(ids.map((id) => deleteMajor(id)));
      await loadMajors();
    } catch (err) {
      console.error("Error deleting all majors:", err);
    } finally {
      setFormLoading(false);
    }
  };

  const handleImport = async (file: File) => {
    setImportLoading(true);
    try {
      // TODO: Implement import logic
      console.log("Import file:", file);
      setImportModalOpen(false);
      await loadMajors();
    } catch (err) {
      console.error("Error importing majors:", err);
    } finally {
      setImportLoading(false);
    }
  };

  const handleSearch = () => {
    setPage(1);
    loadMajors();
  };

  const handleFilterChange = (key: string, value: any) => {
    if (key === "facultyId") {
      setFacultyId(value);
    } else if (key === "deleted") {
      setDeletedFilter(value);
    }
    setPage(1);
  };

  const handleReset = () => {
    setSearchTerm("");
    setFacultyId(null);
    setDeletedFilter(null);
    setSortKey(null);
    setSortDirection(null);
    setPage(1);
  };

  const handleSort = (key: string, direction: "asc" | "desc" | null) => {
    setSortKey(key || null);
    setSortDirection(direction);
  };

  const filters: FilterOption[] = [
    {
      key: "facultyId",
      label: t("form.faculty"),
      type: "select",
      options: faculties
        .filter((f): f is FacultyDto2 & { id: string } => !!f?.id)
        .map((faculty) => ({
          value: faculty.id,
          label: `${faculty.facultyName || ""} (${faculty.facultyCode || ""})`
        })),
      value: facultyId,
    },
    {
      key: "deleted",
      label: t("filter.deleted"),
      type: "select",
      options: [
        { value: "false", label: t("filter.activeItems") },
        { value: "true", label: t("filter.deletedItems") },
      ],
      value: deletedFilter,
    },
  ];

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
          {majors.length > 0 && (
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
          filters={filters}
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

      {majors.length > 0 && (
        <div className="mb-2 text-sm text-slate-600 dark:text-slate-400">
          {t("foundCount", { count: totalCount })}
        </div>
      )}

      <MajorTable
        majors={majors}
        onEdit={(major) => {
          setSelectedMajor(major);
          setEditModalOpen(true);
        }}
        onDelete={(major) => {
          setSelectedMajor(major);
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
          const form = document.getElementById("major-form") as HTMLFormElement;
          if (form) {
            form.requestSubmit();
          }
        }}
        loading={formLoading}
      >
        <MajorForm
          onSubmit={handleCreate}
          loading={formLoading}
        />
      </CreateModal>

      <EditModal
        open={editModalOpen}
        onOpenChange={(open) => {
          setEditModalOpen(open);
          if (!open) setSelectedMajor(null);
        }}
        title={t("editTitle")}
        onSubmit={async () => {
          const form = document.getElementById("major-form") as HTMLFormElement;
          if (form) {
            form.requestSubmit();
          }
        }}
        loading={formLoading}
      >
        <MajorForm
          initialData={{
            majorName: selectedMajor?.majorName || null,
            majorCode: selectedMajor?.majorCode || null,
            facultyId: selectedMajor?.faculty?.id || null,
          }}
          onSubmit={handleEdit}
          loading={formLoading}
        />
      </EditModal>

      <DeleteModal
        open={deleteModalOpen}
        onOpenChange={(open) => {
          setDeleteModalOpen(open);
          if (!open) setSelectedMajor(null);
        }}
        itemName={selectedMajor?.majorName}
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

