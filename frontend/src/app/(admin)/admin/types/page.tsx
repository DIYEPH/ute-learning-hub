"use client";

import { useState, useEffect, useCallback } from "react";
import { Button } from "@/src/components/ui/button";
import { Pagination } from "@/src/components/ui/pagination";
import { Plus, Upload, Trash2 } from "lucide-react";
import { useTranslations } from "next-intl";
import { useTypes } from "@/src/hooks/use-types";
import { TypeTable } from "@/src/components/admin/types/type-table";
import { TypeForm } from "@/src/components/admin/types/type-form";
import { CreateModal } from "@/src/components/admin/modals/create-modal";
import { EditModal } from "@/src/components/admin/modals/edit-modal";
import { DeleteModal } from "@/src/components/admin/modals/delete-modal";
import { ImportModal } from "@/src/components/admin/modals/import-modal";
import { AdvancedSearchFilter } from "@/src/components/admin/advanced-search-filter";
import type {
  TypeDto,
  CreateTypeCommand,
  UpdateTypeCommand,
} from "@/src/api/database/types.gen";

export default function TypesManagementPage() {
  const t = useTranslations("admin.types");
  const tCommon = useTranslations("common");
  const {
    fetchTypes,
    createType,
    updateType,
    deleteType,
    loading,
    error,
  } = useTypes();

  const [types, setTypes] = useState<TypeDto[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [page, setPage] = useState(1);
  const [pageSize] = useState(10);
  const [searchTerm, setSearchTerm] = useState("");
  const [sortKey, setSortKey] = useState<string | null>(null);
  const [sortDirection, setSortDirection] = useState<"asc" | "desc" | null>(null);
  const [createModalOpen, setCreateModalOpen] = useState(false);
  const [editModalOpen, setEditModalOpen] = useState(false);
  const [deleteModalOpen, setDeleteModalOpen] = useState(false);
  const [importModalOpen, setImportModalOpen] = useState(false);
  const [selectedType, setSelectedType] = useState<TypeDto | null>(null);
  const [formLoading, setFormLoading] = useState(false);
  const [importLoading, setImportLoading] = useState(false);

  const loadTypes = useCallback(async () => {
    try {
      const response = await fetchTypes({
        SearchTerm: searchTerm || undefined,
        Page: page,
        PageSize: pageSize,
      });

      if (response) {
        setTypes(response.items || []);
        setTotalCount(response.totalCount || 0);
      }
    } catch (err) {
      console.error("Error loading types:", err);
    }
  }, [fetchTypes, searchTerm, page, pageSize]);

  useEffect(() => {
    loadTypes();
  }, [loadTypes]);

  const handleCreate = async (command: CreateTypeCommand | UpdateTypeCommand) => {
    setFormLoading(true);
    try {
      await createType(command as CreateTypeCommand);
      await loadTypes();
      setCreateModalOpen(false);
    } catch (err) {
      console.error("Error creating type:", err);
    } finally {
      setFormLoading(false);
    }
  };

  const handleEdit = async (command: CreateTypeCommand | UpdateTypeCommand) => {
    if (!selectedType?.id) return;
    setFormLoading(true);
    try {
      await updateType(selectedType.id, command as UpdateTypeCommand);
      await loadTypes();
      setEditModalOpen(false);
      setSelectedType(null);
    } catch (err) {
      console.error("Error updating type:", err);
    } finally {
      setFormLoading(false);
    }
  };

  const handleDelete = async () => {
    if (!selectedType?.id) return;
    setFormLoading(true);
    try {
      await deleteType(selectedType.id);
      await loadTypes();
      setDeleteModalOpen(false);
      setSelectedType(null);
    } catch (err) {
      console.error("Error deleting type:", err);
    } finally {
      setFormLoading(false);
    }
  };

  const handleBulkDelete = async (ids: string[]) => {
    setFormLoading(true);
    try {
      await Promise.all(ids.map((id) => deleteType(id)));
      await loadTypes();
    } catch (err) {
      console.error("Error bulk deleting types:", err);
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
      await loadTypes();
    } catch (err) {
      console.error("Error importing types:", err);
    } finally {
      setImportLoading(false);
    }
  };

  const handleDeleteAll = async () => {
    if (types.length === 0) return;
    if (!confirm(t("deleteAllConfirm", { count: types.length }))) return;
    
    setFormLoading(true);
    try {
      const ids = types.map((t) => t.id).filter((id): id is string => !!id);
      await Promise.all(ids.map((id) => deleteType(id)));
      await loadTypes();
    } catch (err) {
      console.error("Error deleting all types:", err);
    } finally {
      setFormLoading(false);
    }
  };

  const handleSearch = () => {
    setPage(1);
    loadTypes();
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
    <div className="p-4 md:p-6">
      <div className="mb-6 flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <h1 className="text-xl md:text-2xl font-semibold text-foreground">{t("title")}</h1>
        <div className="flex gap-2">
          <Button onClick={() => setImportModalOpen(true)} variant="outline" size="sm" className="text-xs sm:text-sm">
            <Upload size={16} className="mr-1" />
            {t("import")}
          </Button>
          {types.length > 0 && (
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
          filters={[]}
          onReset={handleReset}
          placeholder={t("searchPlaceholder")}
        />
      </div>

      {error && (
        <div className="mb-4 p-2 text-sm text-red-600 dark:text-red-400 bg-red-50 dark:bg-red-950 rounded">
          {error}
        </div>
      )}

      {types.length > 0 && (
        <div className="mb-2 text-sm text-slate-600 dark:text-slate-400">
          {t("foundCount", { count: totalCount })}
        </div>
      )}

      <TypeTable
        types={types}
        onEdit={(type) => {
          setSelectedType(type);
          setEditModalOpen(true);
        }}
        onDelete={(type) => {
          setSelectedType(type);
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
          const form = document.getElementById("type-form") as HTMLFormElement;
          if (form) {
            form.requestSubmit();
          }
        }}
        loading={formLoading}
      >
        <TypeForm
          onSubmit={handleCreate}
          loading={formLoading}
        />
      </CreateModal>

      <EditModal
        open={editModalOpen}
        onOpenChange={(open) => {
          setEditModalOpen(open);
          if (!open) setSelectedType(null);
        }}
        title={t("editTitle")}
        onSubmit={async () => {
          const form = document.getElementById("type-form") as HTMLFormElement;
          if (form) {
            form.requestSubmit();
          }
        }}
        loading={formLoading}
      >
        <TypeForm
          initialData={selectedType || undefined}
          onSubmit={handleEdit}
          loading={formLoading}
        />
      </EditModal>

      <DeleteModal
        open={deleteModalOpen}
        onOpenChange={(open) => {
          setDeleteModalOpen(open);
          if (!open) setSelectedType(null);
        }}
        itemName={selectedType?.typeName}
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

