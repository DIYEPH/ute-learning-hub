"use client";

import { useState, useEffect, useCallback } from "react";
import { Button } from "@/src/components/ui/button";
import { InputWithIcon } from "@/src/components/ui/input-with-icon";
import { Pagination } from "@/src/components/ui/pagination";
import { Search, Plus, Upload, Download } from "lucide-react";
import { useUsers } from "@/src/hooks/use-users";
import { UserTable } from "@/src/components/admin/users/user-table";
import { UserForm } from "@/src/components/admin/users/user-form";
import { UserImportForm } from "@/src/components/admin/users/user-import-form";
import { CreateModal } from "@/src/components/admin/modals/create-modal";
import { EditModal } from "@/src/components/admin/modals/edit-modal";
import { DeleteModal } from "@/src/components/admin/modals/delete-modal";
import { ImportModal } from "@/src/components/admin/modals/import-modal";
import type { UserDto, UpdateUserCommand } from "@/src/api/database/types.gen";
import { useTranslations } from "next-intl";
import { Users } from "lucide-react";

export default function UsersManagementPage() {
  const t = useTranslations("admin.users");
  const tCommon = useTranslations("common");
  const { fetchUsers, fetchUserById, updateUser, banUser, unbanUser, loading, error } = useUsers();

  const [users, setUsers] = useState<UserDto[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [page, setPage] = useState(1);
  const [pageSize] = useState(10);
  const [searchTerm, setSearchTerm] = useState("");
  const [sortBy, setSortBy] = useState<string>("createdAt");
  const [sortDescending, setSortDescending] = useState(true);

  const [createModalOpen, setCreateModalOpen] = useState(false);
  const [editModalOpen, setEditModalOpen] = useState(false);
  const [deleteModalOpen, setDeleteModalOpen] = useState(false);
  const [importModalOpen, setImportModalOpen] = useState(false);
  const [selectedUser, setSelectedUser] = useState<UserDto | null>(null);
  const [formLoading, setFormLoading] = useState(false);

  const loadUsers = useCallback(async () => {
    try {
      const response = await fetchUsers({
        SearchTerm: searchTerm || undefined,
        SortBy: sortBy,
        SortDescending: sortDescending,
        Page: page,
        PageSize: pageSize,
      });

      if (response) {
        setUsers(response.items || []);
        setTotalCount(response.totalCount || 0);
      }
    } catch (err) {
      console.error("Error loading users:", err);
    }
  }, [fetchUsers, searchTerm, sortBy, sortDescending, page, pageSize]);

  useEffect(() => {
    loadUsers();
  }, [loadUsers]);

  const handleCreate = async (command: UpdateUserCommand) => {
    setFormLoading(true);
    try {
      // TODO: Implement create user API call when backend endpoint is available
      // For now, just reload the list
      await loadUsers();
      setCreateModalOpen(false);
    } catch (err) {
      console.error("Error creating user:", err);
    } finally {
      setFormLoading(false);
    }
  };

  const handleEdit = async (command: UpdateUserCommand) => {
    if (!selectedUser?.id) return;
    setFormLoading(true);
    try {
      await updateUser(selectedUser.id, command);
      await loadUsers();
      setEditModalOpen(false);
      setSelectedUser(null);
    } catch (err) {
      console.error("Error updating user:", err);
    } finally {
      setFormLoading(false);
    }
  };

  const handleDelete = async () => {
    if (!selectedUser?.id) return;
    setFormLoading(true);
    try {
      await banUser(selectedUser.id);
      await loadUsers();
      setDeleteModalOpen(false);
      setSelectedUser(null);
    } catch (err) {
      console.error("Error deleting user:", err);
    } finally {
      setFormLoading(false);
    }
  };

  const handleBan = async (user: UserDto) => {
    if (!user.id) return;
    setFormLoading(true);
    try {
      await banUser(user.id);
      await loadUsers();
    } catch (err) {
      console.error("Error banning user:", err);
    } finally {
      setFormLoading(false);
    }
  };

  const handleUnban = async (user: UserDto) => {
    if (!user.id) return;
    setFormLoading(true);
    try {
      await unbanUser(user.id);
      await loadUsers();
    } catch (err) {
      console.error("Error unbanning user:", err);
    } finally {
      setFormLoading(false);
    }
  };

  const handleImport = async (file: File) => {
    setFormLoading(true);
    try {
      const formData = new FormData();
      formData.append("file", file);
      await fetch("/api/users/import", {
        method: "POST",
        body: formData,
      });
      await loadUsers();
      setImportModalOpen(false);
    } catch (err) {
      console.error("Error importing users:", err);
    } finally {
      setFormLoading(false);
    }
  };

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    setPage(1);
    loadUsers();
  };

  const totalPages = Math.ceil(totalCount / pageSize);

  return (
    <div className="p-4 md:p-6">
      <div className="mb-6 flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <h1 className="text-xl md:text-2xl font-semibold">Quản lý người dùng</h1>
        <div className="flex gap-2">
          <Button variant="outline" onClick={() => setImportModalOpen(true)} size="sm" className="text-xs sm:text-sm">
            Import
          </Button>
          <Button onClick={() => setCreateModalOpen(true)} size="sm" className="text-xs sm:text-sm">Thêm mới</Button>
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

      {users.length > 0 && (
        <div className="mb-2 text-sm text-slate-600">
          Tìm thấy {users.length} người dùng
        </div>
      )}
      
      <UserTable
        users={users}
        onEdit={(user) => {
          setSelectedUser(user);
          setEditModalOpen(true);
        }}
        onDelete={(user) => {
          setSelectedUser(user);
          setDeleteModalOpen(true);
        }}
        onBan={handleBan}
        onUnban={handleUnban}
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
        title="Thêm người dùng"
        onSubmit={async () => {
          const form = document.getElementById("user-form") as HTMLFormElement;
          if (form) {
            form.requestSubmit();
          }
        }}
        loading={formLoading}
        size="lg"
      >
        <UserForm onSubmit={handleCreate} loading={formLoading} />
      </CreateModal>

      <EditModal
        open={editModalOpen}
        onOpenChange={(open) => {
          setEditModalOpen(open);
          if (!open) setSelectedUser(null);
        }}
        title="Sửa người dùng"
        onSubmit={async () => {
          const form = document.getElementById("user-form") as HTMLFormElement;
          if (form) {
            form.requestSubmit();
          }
        }}
        loading={formLoading}
        size="lg"
      >
        <UserForm
          initialData={selectedUser || undefined}
          onSubmit={handleEdit}
          loading={formLoading}
        />
      </EditModal>

      <DeleteModal
        open={deleteModalOpen}
        onOpenChange={(open) => {
          setDeleteModalOpen(open);
          if (!open) setSelectedUser(null);
        }}
        itemName={selectedUser?.fullName}
        onConfirm={handleDelete}
        loading={formLoading}
      />

      <ImportModal
        open={importModalOpen}
        onOpenChange={setImportModalOpen}
        title="Import người dùng"
        onImport={handleImport}
        loading={formLoading}
        size="lg"
      >
        <UserImportForm onImport={handleImport} loading={formLoading} />
      </ImportModal>
    </div>
  );
}

