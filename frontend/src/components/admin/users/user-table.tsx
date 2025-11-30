"use client";

import { useState, useEffect } from "react";
import { Button } from "@/src/components/ui/button";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/src/components/ui/table";
import { Avatar, AvatarImage, AvatarFallback } from "@/src/components/ui/avatar";
import { Trash2 } from "lucide-react";
import type { UserDto } from "@/src/api/database/types.gen";
import { useTranslations } from "next-intl";

interface UserTableProps {
  users: UserDto[];
  onEdit?: (user: UserDto) => void;
  onDelete?: (user: UserDto) => void;
  onBan?: (user: UserDto) => void;
  onUnban?: (user: UserDto) => void;
  onBulkDelete?: (ids: string[]) => void | Promise<void>;
  loading?: boolean;
  currentUserId?: string | null;
}

export function UserTable({
  users,
  onEdit,
  onDelete,
  onBan,
  onUnban,
  onBulkDelete,
  loading,
  currentUserId,
}: UserTableProps) {
  const t = useTranslations("admin.users");
  const tCommon = useTranslations("common");
  const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set());

  useEffect(() => {
    setSelectedIds(new Set());
  }, [users]);

  const handleSelectAll = (checked: boolean) => {
    if (checked) {
      const allIds = users
        .filter((u): u is UserDto & { id: string } => !!u?.id)
        .map((u) => u.id);
      setSelectedIds(new Set(allIds));
    } else {
      setSelectedIds(new Set());
    }
  };

  const handleSelectOne = (id: string, checked: boolean) => {
    const newSelected = new Set(selectedIds);
    if (checked) {
      newSelected.add(id);
    } else {
      newSelected.delete(id);
    }
    setSelectedIds(newSelected);
  };

  const handleBulkDelete = async () => {
    if (selectedIds.size > 0 && onBulkDelete) {
      await onBulkDelete(Array.from(selectedIds));
      setSelectedIds(new Set());
    }
  };

  const getInitials = (name?: string) => {
    if (!name) return "U";
    return name
      .split(" ")
      .map((n) => n[0])
      .join("")
      .toUpperCase()
      .slice(0, 2);
  };

  const isAllSelected = users.length > 0 && selectedIds.size === users.filter((u) => u?.id).length;
  const isIndeterminate = selectedIds.size > 0 && selectedIds.size < users.filter((u) => u?.id).length;

  if (loading) {
    return (
      <div className="flex items-center justify-center py-8">
        <p className="text-slate-600 dark:text-slate-400">{t("table.loading")}</p>
      </div>
    );
  }

  if (users.length === 0) {
    return (
      <div className="flex items-center justify-center py-8">
        <p className="text-slate-600 dark:text-slate-400">{t("table.noData")}</p>
      </div>
    );
  }

  return (
    <div className="space-y-2">
      {selectedIds.size > 0 && onBulkDelete && (
        <div className="flex items-center justify-between p-2 bg-blue-50 dark:bg-blue-950 rounded border border-blue-200 dark:border-blue-800">
          <span className="text-sm text-blue-900 dark:text-blue-100">
            {t("table.selectedCount", { count: selectedIds.size })}
          </span>
          <Button
            variant="destructive"
            size="sm"
            onClick={handleBulkDelete}
            className="h-7 px-2 text-xs sm:text-sm"
          >
            <Trash2 size={14} className="mr-1" />
            {t("table.deleteSelected")}
          </Button>
        </div>
      )}
      <div className="border rounded overflow-x-auto">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead className="w-12">
                <input
                  type="checkbox"
                  checked={isAllSelected}
                  ref={(input) => {
                    if (input) input.indeterminate = isIndeterminate;
                  }}
                  onChange={(e) => handleSelectAll(e.target.checked)}
                  className="cursor-pointer"
                />
              </TableHead>
              <TableHead className="min-w-[150px]">{t("table.fullName")}</TableHead>
              <TableHead className="min-w-[180px]">{t("table.email")}</TableHead>
              <TableHead className="min-w-[120px]">{t("table.major")}</TableHead>
              <TableHead className="min-w-[80px]">{t("table.roles")}</TableHead>
              <TableHead className="text-right min-w-[200px]">{t("table.actions")}</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {users.map((user) => {
              const isSelected = user.id ? selectedIds.has(user.id) : false;
              return (
                <TableRow key={user.id}>
                  <TableCell>
                    {user.id && (
                      <input
                        type="checkbox"
                        checked={isSelected}
                        onChange={(e) => handleSelectOne(user.id!, e.target.checked)}
                        className="cursor-pointer"
                      />
                    )}
                  </TableCell>
                  <TableCell>
                    <div className="flex items-center gap-2">
                      <Avatar className="h-8 w-8 flex-shrink-0">
                        <AvatarImage src={user.avatarUrl || undefined} alt={user.fullName} />
                        <AvatarFallback className="text-xs">
                          {getInitials(user.fullName)}
                        </AvatarFallback>
                      </Avatar>
                      <div className="min-w-0">
                        <div className="font-medium text-foreground truncate">{user.fullName}</div>
                        {user.username && (
                          <div className="text-xs text-slate-500 dark:text-slate-400">@{user.username}</div>
                        )}
                      </div>
                    </div>
                  </TableCell>
                  <TableCell>
                    <div className="text-sm text-foreground">{user.email}</div>
                  </TableCell>
                  <TableCell>
                    {user.major ? (
                      <div className="text-sm text-foreground truncate">{user.major.majorName}</div>
                    ) : (
                      <span className="text-slate-400">-</span>
                    )}
                  </TableCell>
                  <TableCell>
                    <div className="flex gap-1 flex-wrap">
                      {user.roles?.map((role) => (
                        <span
                          key={role}
                          className="inline-flex items-center px-2 py-0.5 rounded text-xs bg-slate-100 dark:bg-slate-800 text-slate-700 dark:text-slate-300"
                        >
                          {role}
                        </span>
                      ))}
                    </div>
                  </TableCell>
                  <TableCell className="text-right">
                    <div className="flex items-center justify-end gap-1 flex-wrap">
                      {onEdit && (
                        <Button
                          variant="ghost"
                          size="sm"
                          onClick={() => onEdit(user)}
                          className="h-7 px-2 text-xs sm:text-sm"
                        >
                          {t("table.edit")}
                        </Button>
                      )}
                      {user.lockoutEnd && new Date(user.lockoutEnd) > new Date() ? (
                        onUnban && (
                          <Button
                            variant="ghost"
                            size="sm"
                            onClick={() => onUnban(user)}
                            className="h-7 px-2 text-xs sm:text-sm"
                          >
                            <span className="hidden sm:inline">{t("table.unban")}</span>
                            <span className="sm:hidden">{t("table.unban")}</span>
                          </Button>
                        )
                      ) : (
                        onBan && (
                          <Button
                            variant="ghost"
                            size="sm"
                            onClick={() => onBan(user)}
                            disabled={user.id === currentUserId}
                            className="h-7 px-2 text-xs sm:text-sm"
                            title={user.id === currentUserId ? t("table.cannotBanSelf") : undefined}
                          >
                            {t("table.ban")}
                          </Button>
                        )
                      )}
                      {onDelete && (
                        <Button
                          variant="ghost"
                          size="sm"
                          onClick={() => onDelete(user)}
                          className="h-7 px-2 text-xs sm:text-sm text-red-600 hover:text-red-700 dark:text-red-400 dark:hover:text-red-300"
                        >
                          {t("table.delete")}
                        </Button>
                      )}
                    </div>
                  </TableCell>
                </TableRow>
              );
            })}
          </TableBody>
        </Table>
      </div>
    </div>
  );
}
