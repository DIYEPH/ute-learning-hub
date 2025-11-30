"use client";

import { useState } from "react";
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
import { Badge } from "@/src/components/ui/badge";
import { MoreHorizontal, Edit, Trash2, Ban, Unlock, UserCheck } from "lucide-react";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/src/components/ui/dropdown-menu";
import type { UserDto } from "@/src/api/database/types.gen";
import { useTranslations } from "next-intl";

interface UserTableProps {
  users: UserDto[];
  onEdit?: (user: UserDto) => void;
  onDelete?: (user: UserDto) => void;
  onBan?: (user: UserDto) => void;
  onUnban?: (user: UserDto) => void;
  loading?: boolean;
}

export function UserTable({
  users,
  onEdit,
  onDelete,
  onBan,
  onUnban,
  loading,
}: UserTableProps) {
  const t = useTranslations("common");

  const getInitials = (name?: string) => {
    if (!name) return "U";
    return name
      .split(" ")
      .map((n) => n[0])
      .join("")
      .toUpperCase()
      .slice(0, 2);
  };

  const getRoleBadgeColor = (role: string) => {
    switch (role) {
      case "Admin":
        return "bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-200";
      case "Student":
        return "bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-200";
      default:
        return "bg-slate-100 text-slate-800 dark:bg-slate-800 dark:text-slate-200";
    }
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center py-8">
        <p className="text-slate-600 dark:text-slate-400">{t("loading")}</p>
      </div>
    );
  }

  if (users.length === 0 && !loading) {
    return (
      <div className="flex items-center justify-center py-8">
        <p className="text-slate-600 dark:text-slate-400">Không có dữ liệu</p>
      </div>
    );
  }

  if (users.length === 0) {
    return null;
  }

  return (
    <div className="border rounded overflow-x-auto">
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead className="min-w-[150px]">Họ tên</TableHead>
            <TableHead className="min-w-[180px] hidden sm:table-cell">Email</TableHead>
            <TableHead className="min-w-[120px] hidden md:table-cell">Ngành</TableHead>
            <TableHead className="min-w-[80px] hidden lg:table-cell">Vai trò</TableHead>
            <TableHead className="text-right min-w-[200px]">Thao tác</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {users.map((user) => (
            <TableRow key={user.id}>
              <TableCell>
                <div className="flex items-center gap-2">
                  <Avatar className="h-8 w-8 flex-shrink-0">
                    <AvatarImage src={user.avatarUrl || undefined} alt={user.fullName} />
                    <AvatarFallback className="text-xs">
                      {getInitials(user.fullName)}
                    </AvatarFallback>
                  </Avatar>
                  <div className="min-w-0">
                    <div className="font-medium truncate">{user.fullName}</div>
                    <div className="text-xs text-slate-500 sm:hidden">{user.email}</div>
                    {user.username && (
                      <div className="text-xs text-slate-500 hidden sm:block">@{user.username}</div>
                    )}
                  </div>
                </div>
              </TableCell>
              <TableCell className="hidden sm:table-cell">
                <div className="text-sm">{user.email}</div>
              </TableCell>
              <TableCell className="hidden md:table-cell">
                {user.major ? (
                  <div className="text-sm truncate">{user.major.majorName}</div>
                ) : (
                  <span className="text-slate-400">-</span>
                )}
              </TableCell>
              <TableCell className="hidden lg:table-cell">
                <div className="flex gap-1">
                  {user.roles?.map((role) => (
                    <span key={role} className="text-xs">
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
                      Sửa
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
                        <span className="hidden sm:inline">Mở khóa</span>
                        <span className="sm:hidden">Mở</span>
                      </Button>
                    )
                  ) : (
                    onBan && (
                      <Button
                        variant="ghost"
                        size="sm"
                        onClick={() => onBan(user)}
                        className="h-7 px-2 text-xs sm:text-sm"
                      >
                        Khóa
                      </Button>
                    )
                  )}
                  {onDelete && (
                    <Button
                      variant="ghost"
                      size="sm"
                      onClick={() => onDelete(user)}
                      className="h-7 px-2 text-xs sm:text-sm text-red-600 hover:text-red-700"
                    >
                      Xóa
                    </Button>
                  )}
                </div>
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </div>
  );
}

