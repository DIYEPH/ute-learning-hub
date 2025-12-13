"use client";

import { useState, useEffect, useCallback } from "react";
import { Button } from "@/src/components/ui/button";
import { Pagination } from "@/src/components/ui/pagination";
import { Plus, Upload, Trash2 } from "lucide-react";
import { useUsers } from "@/src/hooks/use-users";
import { useMajors } from "@/src/hooks/use-majors";
import { useFaculties } from "@/src/hooks/use-faculties";
import { useUserProfile } from "@/src/hooks/use-user-profile";
import { UserTable } from "@/src/components/admin/users/user-table";
import { UserForm } from "@/src/components/admin/users/user-form";
import { CreateModal } from "@/src/components/admin/modals/create-modal";
import { EditModal } from "@/src/components/admin/modals/edit-modal";
import { DeleteModal } from "@/src/components/admin/modals/delete-modal";
import { ImportModal } from "@/src/components/admin/modals/import-modal";
import { BanModal } from "@/src/components/admin/modals/ban-modal";
import { AdvancedSearchFilter, type FilterOption } from "@/src/components/admin/advanced-search-filter";
import type { UserDto, UpdateUserCommand, MajorDto2, FacultyDto2 } from "@/src/api/database/types.gen";
import { useTranslations } from "next-intl";

export default function UsersManagementPage() {
  const t = useTranslations("admin.users");
  const tCommon = useTranslations("common");
  const { fetchUsers, fetchUserById, updateUser, banUser, unbanUser, loading, error } = useUsers();
  const { fetchMajors } = useMajors();
  const { fetchFaculties } = useFaculties();
  const { profile: currentUserProfile } = useUserProfile();

  const [users, setUsers] = useState<UserDto[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [page, setPage] = useState(1);
  const [pageSize] = useState(10);
  const [searchTerm, setSearchTerm] = useState("");
  const [sortBy, setSortBy] = useState<string>("createdAt");
  const [sortDescending, setSortDescending] = useState(true);

  // Filter states
  const [facultyId, setFacultyId] = useState<string | null>(null);
  const [majorId, setMajorId] = useState<string | null>(null);
  const [trustLevel, setTrustLevel] = useState<string | null>(null);
  const [emailConfirmed, setEmailConfirmed] = useState<boolean | null>(null);
  const [isDeleted, setIsDeleted] = useState<boolean | null>(null);
  const [majors, setMajors] = useState<MajorDto2[]>([]);
  const [faculties, setFaculties] = useState<FacultyDto2[]>([]);

  const [createModalOpen, setCreateModalOpen] = useState(false);
  const [editModalOpen, setEditModalOpen] = useState(false);
  const [deleteModalOpen, setDeleteModalOpen] = useState(false);
  const [importModalOpen, setImportModalOpen] = useState(false);
  const [banModalOpen, setBanModalOpen] = useState(false);
  const [selectedUser, setSelectedUser] = useState<UserDto | null>(null);
  const [formLoading, setFormLoading] = useState(false);

  const loadUsers = useCallback(async () => {
    try {
      const response = await fetchUsers({
        SearchTerm: searchTerm || undefined,
        MajorId: majorId || undefined,
        TrustLevel: trustLevel || undefined,
        EmailConfirmed: emailConfirmed !== null ? emailConfirmed : undefined,
        IsDeleted: isDeleted !== null ? isDeleted : undefined,
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
  }, [fetchUsers, searchTerm, majorId, trustLevel, emailConfirmed, isDeleted, sortBy, sortDescending, page, pageSize]);

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
    const loadMajorsForFilter = async () => {
      try {
        const response = await fetchMajors({
          Page: 1,
          PageSize: 1000,
          FacultyId: facultyId || undefined,
        });
        if (response?.items) {
          setMajors(response.items);
          // Reset majorId if selected faculty changed and current major doesn't belong to it
          if (facultyId && majorId) {
            const major = response.items.find((m) => m.id === majorId);
            if (!major) {
              setMajorId(null);
            }
          }
        }
      } catch (err) {
        console.error("Error loading majors:", err);
      }
    };
    loadMajorsForFilter();
  }, [fetchMajors, facultyId, majorId]);

  useEffect(() => {
    loadUsers();
  }, [loadUsers]);

  // Auto reload when filters change
  useEffect(() => {
    const timer = setTimeout(() => {
      loadUsers();
    }, 300); // Debounce 300ms
    return () => clearTimeout(timer);
  }, [facultyId, majorId, trustLevel, emailConfirmed, isDeleted]);

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
    // Check if trying to ban self
    if (currentUserProfile?.id === user.id) {
      alert(t("banModal.cannotBanSelf"));
      return;
    }
    setSelectedUser(user);
    setBanModalOpen(true);
  };

  const handleConfirmBan = async (banUntil: string | null) => {
    if (!selectedUser?.id) return;
    setFormLoading(true);
    try {
      await banUser(selectedUser.id, banUntil);
      await loadUsers();
      setBanModalOpen(false);
      setSelectedUser(null);
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

  const handleBulkDelete = async (ids: string[]) => {
    setFormLoading(true);
    try {
      await Promise.all(ids.map((id) => banUser(id)));
      await loadUsers();
    } catch (err) {
      console.error("Error bulk deleting users:", err);
    } finally {
      setFormLoading(false);
    }
  };

  const handleDeleteAll = async () => {
    if (users.length === 0) return;
    if (!confirm(t("deleteAllConfirm", { count: users.length }))) return;

    setFormLoading(true);
    try {
      const ids = users.map((u) => u.id).filter((id): id is string => !!id);
      await Promise.all(ids.map((id) => banUser(id)));
      await loadUsers();
    } catch (err) {
      console.error("Error deleting all users:", err);
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

  const handleSearch = () => {
    setPage(1);
    loadUsers();
  };

  const handleFilterChange = (key: string, value: any) => {
    switch (key) {
      case "facultyId":
        setFacultyId(value);
        // Reset majorId when faculty changes
        setMajorId(null);
        break;
      case "majorId":
        setMajorId(value);
        break;
      case "trustLevel":
        setTrustLevel(value);
        break;
      case "emailConfirmed":
        setEmailConfirmed(value);
        break;
      case "isDeleted":
        setIsDeleted(value);
        break;
    }
    setPage(1);
  };

  const handleReset = () => {
    setSearchTerm("");
    setFacultyId(null);
    setMajorId(null);
    setTrustLevel(null);
    setEmailConfirmed(null);
    setIsDeleted(null);
    setPage(1);
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
          label: `${faculty.facultyName || ""} (${faculty.facultyCode || ""})`,
        })),
      value: facultyId,
    },
    {
      key: "majorId",
      label: t("table.major"),
      type: "select",
      options: majors
        .filter((m): m is MajorDto2 & { id: string } => !!m?.id)
        .map((major) => ({
          value: major.id,
          label: `${major.majorName || ""} (${major.majorCode || ""})`,
        })),
      value: majorId,
    },
    {
      key: "trustLevel",
      label: t("table.trustLevel"),
      type: "select",
      options: [
        { value: "None", label: t("table.trustLevelNone") },
        { value: "Newbie", label: t("table.trustLevelNewbie") },
        { value: "Contributor", label: t("table.trustLevelContributor") },
        { value: "TrustedMember", label: t("table.trustLevelTrustedMember") },
        { value: "Moderator", label: t("table.trustLevelModerator") },
        { value: "Master", label: t("table.trustLevelMaster") },
      ],
      value: trustLevel,
    },
    {
      key: "emailConfirmed",
      label: t("table.emailConfirmed"),
      type: "checkbox",
      value: emailConfirmed === true,
    },
    {
      key: "isDeleted",
      label: t("table.isDeleted"),
      type: "checkbox",
      value: isDeleted === true,
    },
  ];

  const totalPages = Math.ceil(totalCount / pageSize);

  return (
    <div className="p-4 md:p-6">
      <div className="mb-6 flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <h1 className="text-xl md:text-2xl font-semibold text-foreground">{t("title")}</h1>
        <div className="flex gap-2">
          <Button variant="outline" onClick={() => setImportModalOpen(true)} size="sm" className="text-xs sm:text-sm">
            <Upload size={16} className="mr-1" />
            {t("import")}
          </Button>
          {users.length > 0 && (
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

      {users.length > 0 && (
        <div className="mb-2 text-sm text-slate-600 dark:text-slate-400">
          {t("foundCount", { count: totalCount })}
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
        currentUserId={currentUserProfile?.id}
        onBulkDelete={handleBulkDelete}
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
        title={t("createTitle")}
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
        title={t("editTitle")}
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
          initialData={selectedUser as import("@/src/components/admin/users/user-form").UserFormData || undefined}
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
        title={t("importTitle")}
        description={t("importDescription")}
        onImport={handleImport}
        loading={formLoading}
        size="lg"
        templateContent={`Email,FullName,Username,Password,MajorCode,Gender`}
        templateFileName="user_import_template.csv"
        helpText="Chấp nhận: CSV, Excel (.csv, .xlsx, .xls)"
        requiredColumns="Bắt buộc: Email, FullName,Password"
      />

      <BanModal
        open={banModalOpen}
        onOpenChange={(open) => {
          setBanModalOpen(open);
          if (!open) setSelectedUser(null);
        }}
        userName={selectedUser?.fullName || selectedUser?.email}
        onConfirm={handleConfirmBan}
        loading={formLoading}
      />
    </div>
  );
}



