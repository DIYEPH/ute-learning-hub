"use client";

import { useState, useEffect, useCallback } from "react";
import { Button } from "@/src/components/ui/button";
import { InputWithIcon } from "@/src/components/ui/input-with-icon";
import { Pagination } from "@/src/components/ui/pagination";
import { Search, Plus } from "lucide-react";
import { useFaculties } from "@/src/hooks/use-faculties";
import { FacultyTable } from "@/src/components/admin/faculties/faculty-table";
import { FacultyForm } from "@/src/components/admin/faculties/faculty-form";
import { CreateModal } from "@/src/components/admin/modals/create-modal";
import { EditModal } from "@/src/components/admin/modals/edit-modal";
import { DeleteModal } from "@/src/components/admin/modals/delete-modal";
import type { FacultyDto2, CreateFacultyCommand, UpdateFacultyCommand } from "@/src/api/database/types.gen";
import { Building } from "lucide-react";

export default function FacultiesManagementPage() {
  const { fetchFaculties, createFaculty, updateFaculty, deleteFaculty, loading, error } = useFaculties();

  const [faculties, setFaculties] = useState<FacultyDto2[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [page, setPage] = useState(1);
  const [pageSize] = useState(10);
  const [searchTerm, setSearchTerm] = useState("");

  const [createModalOpen, setCreateModalOpen] = useState(false);
  const [editModalOpen, setEditModalOpen] = useState(false);
  const [deleteModalOpen, setDeleteModalOpen] = useState(false);
  const [selectedFaculty, setSelectedFaculty] = useState<FacultyDto2 | null>(null);
  const [formLoading, setFormLoading] = useState(false);

  const loadFaculties = useCallback(async () => {
    try {
      const response = await fetchFaculties({
        SearchTerm: searchTerm || undefined,
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
  }, [fetchFaculties, searchTerm, page, pageSize]);

  useEffect(() => {
    loadFaculties();
  }, [loadFaculties]);

  const handleCreate = async (command: CreateFacultyCommand | UpdateFacultyCommand) => {
    setFormLoading(true);
    try {
      await createFaculty(command as CreateFacultyCommand);
      await loadFaculties();
      setCreateModalOpen(false);
    } catch (err) {
      console.error("Error creating faculty:", err);
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
    } catch (err) {
      console.error("Error updating faculty:", err);
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
    } catch (err) {
      console.error("Error deleting faculty:", err);
    } finally {
      setFormLoading(false);
    }
  };

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    setPage(1);
    loadFaculties();
  };

  const totalPages = Math.ceil(totalCount / pageSize);

  return (
    <div className="p-4 md:p-6">
      <div className="mb-6 flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <h1 className="text-xl md:text-2xl font-semibold">Quản lý khoa</h1>
        <div className="flex gap-2">
          <Button onClick={() => setCreateModalOpen(true)} size="sm" className="text-xs sm:text-sm">
            Thêm mới
          </Button>
        </div>
      </div>

      <div className="mb-4">
        <form onSubmit={handleSearch} className="w-full sm:max-w-sm">
          <InputWithIcon
            prefixIcon={Search}
            placeholder="Tìm kiếm..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
          />
        </form>
      </div>

      {error && (
        <div className="mb-4 p-2 text-sm text-red-600 bg-red-50 rounded">
          {error}
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
        loading={loading}
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
        title="Thêm khoa"
        onSubmit={async () => {
          const form = document.getElementById("faculty-form") as HTMLFormElement;
          if (form) {
            form.requestSubmit();
          }
        }}
        loading={formLoading}
      >
        <FacultyForm onSubmit={handleCreate} loading={formLoading} />
      </CreateModal>

      <EditModal
        open={editModalOpen}
        onOpenChange={(open) => {
          setEditModalOpen(open);
          if (!open) setSelectedFaculty(null);
        }}
        title="Sửa khoa"
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
    </div>
  );
}

