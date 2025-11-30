"use client";

import { useState, useEffect, useCallback } from "react";
import { Button } from "@/src/components/ui/button";
import { Pagination } from "@/src/components/ui/pagination";
import { Plus, Upload, Trash2 } from "lucide-react";
import { useTranslations } from "next-intl";
import { useSubjects } from "@/src/hooks/use-subjects";
import { useMajors } from "@/src/hooks/use-majors";
import { SubjectTable } from "@/src/components/admin/subjects/subject-table";
import { SubjectForm } from "@/src/components/admin/subjects/subject-form";
import { CreateModal } from "@/src/components/admin/modals/create-modal";
import { EditModal } from "@/src/components/admin/modals/edit-modal";
import { DeleteModal } from "@/src/components/admin/modals/delete-modal";
import { ImportModal } from "@/src/components/admin/modals/import-modal";
import { AdvancedSearchFilter, type FilterOption } from "@/src/components/admin/advanced-search-filter";
import type {
  SubjectDto2,
  CreateSubjectCommand,
  UpdateSubjectCommand,
  MajorDto2,
} from "@/src/api/database/types.gen";

export default function SubjectsManagementPage() {
  const t = useTranslations("admin.subjects");
  const tCommon = useTranslations("common");
  const {
    fetchSubjects,
    createSubject,
    updateSubject,
    deleteSubject,
    loading,
    error,
  } = useSubjects();
  const { fetchMajors } = useMajors();

  const [subjects, setSubjects] = useState<SubjectDto2[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [page, setPage] = useState(1);
  const [pageSize] = useState(10);
  const [searchTerm, setSearchTerm] = useState("");
  
  // Filter states
  const [majorIds, setMajorIds] = useState<string[]>([]);
  const [majors, setMajors] = useState<MajorDto2[]>([]);
  const [sortKey, setSortKey] = useState<string | null>(null);
  const [sortDirection, setSortDirection] = useState<"asc" | "desc" | null>(null);
  const [createModalOpen, setCreateModalOpen] = useState(false);
  const [editModalOpen, setEditModalOpen] = useState(false);
  const [deleteModalOpen, setDeleteModalOpen] = useState(false);
  const [importModalOpen, setImportModalOpen] = useState(false);
  const [selectedSubject, setSelectedSubject] = useState<SubjectDto2 | null>(null);
  const [formLoading, setFormLoading] = useState(false);
  const [importLoading, setImportLoading] = useState(false);

  const loadSubjects = useCallback(async () => {
    try {
      const response = await fetchSubjects({
        SearchTerm: searchTerm || undefined,
        MajorIds: majorIds.length > 0 ? majorIds : undefined,
        Page: page,
        PageSize: pageSize,
      });

      if (response) {
        setSubjects(response.items || []);
        setTotalCount(response.totalCount || 0);
      }
    } catch (err) {
      console.error("Error loading subjects:", err);
    }
  }, [fetchSubjects, searchTerm, majorIds, page, pageSize]);

  useEffect(() => {
    const loadMajorsForFilter = async () => {
      try {
        const response = await fetchMajors({ Page: 1, PageSize: 1000 });
        if (response?.items) {
          setMajors(response.items);
        }
      } catch (err) {
        console.error("Error loading majors:", err);
      }
    };
    loadMajorsForFilter();
  }, [fetchMajors]);

  useEffect(() => {
    loadSubjects();
  }, [loadSubjects]);

  const handleCreate = async (command: CreateSubjectCommand | UpdateSubjectCommand) => {
    setFormLoading(true);
    try {
      await createSubject(command as CreateSubjectCommand);
      await loadSubjects();
      setCreateModalOpen(false);
    } catch (err) {
      console.error("Error creating subject:", err);
    } finally {
      setFormLoading(false);
    }
  };

  const handleEdit = async (command: CreateSubjectCommand | UpdateSubjectCommand) => {
    if (!selectedSubject?.id) return;
    setFormLoading(true);
    try {
      await updateSubject(selectedSubject.id, command as UpdateSubjectCommand);
      await loadSubjects();
      setEditModalOpen(false);
      setSelectedSubject(null);
    } catch (err) {
      console.error("Error updating subject:", err);
    } finally {
      setFormLoading(false);
    }
  };

  const handleDelete = async () => {
    if (!selectedSubject?.id) return;
    setFormLoading(true);
    try {
      await deleteSubject(selectedSubject.id);
      await loadSubjects();
      setDeleteModalOpen(false);
      setSelectedSubject(null);
    } catch (err) {
      console.error("Error deleting subject:", err);
    } finally {
      setFormLoading(false);
    }
  };

  const handleBulkDelete = async (ids: string[]) => {
    setFormLoading(true);
    try {
      await Promise.all(ids.map((id) => deleteSubject(id)));
      await loadSubjects();
    } catch (err) {
      console.error("Error bulk deleting subjects:", err);
    } finally {
      setFormLoading(false);
    }
  };

  const handleDeleteAll = async () => {
    if (subjects.length === 0) return;
    if (!confirm(t("deleteAllConfirm", { count: subjects.length }))) return;
    
    setFormLoading(true);
    try {
      const ids = subjects.map((s) => s.id).filter((id): id is string => !!id);
      await Promise.all(ids.map((id) => deleteSubject(id)));
      await loadSubjects();
    } catch (err) {
      console.error("Error deleting all subjects:", err);
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
      await loadSubjects();
    } catch (err) {
      console.error("Error importing subjects:", err);
    } finally {
      setImportLoading(false);
    }
  };

  const handleSearch = () => {
    setPage(1);
    loadSubjects();
  };

  const handleFilterChange = (key: string, value: any) => {
    if (key === "majorIds") {
      // For multi-select, value should be an array
      if (Array.isArray(value)) {
        setMajorIds(value);
      } else if (value) {
        setMajorIds([value]);
      } else {
        setMajorIds([]);
      }
    }
    setPage(1);
  };

  const handleReset = () => {
    setSearchTerm("");
    setMajorIds([]);
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
      key: "majorIds",
      label: t("form.majors"),
      type: "multiselect",
      options: majors
        .filter((m): m is MajorDto2 & { id: string } => !!m?.id)
        .map((major) => ({
          value: major.id,
          label: `${major.majorName || ""} (${major.majorCode || ""})${major.faculty ? ` - ${major.faculty.facultyName}` : ""}`,
        })),
      value: majorIds,
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
          {subjects.length > 0 && (
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

      {subjects.length > 0 && (
        <div className="mb-2 text-sm text-slate-600 dark:text-slate-400">
          {t("foundCount", { count: totalCount })}
        </div>
      )}

      <SubjectTable
        subjects={subjects}
        onEdit={(subject) => {
          setSelectedSubject(subject);
          setEditModalOpen(true);
        }}
        onDelete={(subject) => {
          setSelectedSubject(subject);
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
          const form = document.getElementById("subject-form") as HTMLFormElement;
          if (form) {
            form.requestSubmit();
          }
        }}
        loading={formLoading}
      >
        <SubjectForm
          onSubmit={handleCreate}
          loading={formLoading}
        />
      </CreateModal>

      <EditModal
        open={editModalOpen}
        onOpenChange={(open) => {
          setEditModalOpen(open);
          if (!open) setSelectedSubject(null);
        }}
        title={t("editTitle")}
        onSubmit={async () => {
          const form = document.getElementById("subject-form") as HTMLFormElement;
          if (form) {
            form.requestSubmit();
          }
        }}
        loading={formLoading}
      >
        <SubjectForm
          initialData={{
            subjectName: selectedSubject?.subjectName || null,
            subjectCode: selectedSubject?.subjectCode || null,
            majorIds: selectedSubject?.majors?.map((m) => m.id || "").filter(Boolean) || [],
          }}
          onSubmit={handleEdit}
          loading={formLoading}
        />
      </EditModal>

      <DeleteModal
        open={deleteModalOpen}
        onOpenChange={(open) => {
          setDeleteModalOpen(open);
          if (!open) setSelectedSubject(null);
        }}
        itemName={selectedSubject?.subjectName}
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

