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
import { Badge } from "@/src/components/ui/badge";
import { CheckCircle, XCircle, Ban, Shield, Star } from "lucide-react";
import type { UserDto } from "@/src/api/database/types.gen";
import { useTranslations } from "next-intl";
import { cn } from "@/lib/utils";

interface UserTableProps {
  users: UserDto[];
  onEdit?: (user: UserDto) => void;

  onBan?: (user: UserDto) => void;
  onUnban?: (user: UserDto) => void;

  loading?: boolean;
  currentUserId?: string | null;
}

export function UserTable({
  users,
  onEdit,
  onBan,
  onUnban,
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
        <p className="text-muted-foreground">{t("table.loading")}</p>
      </div>
    );
  }

  if (users.length === 0) {
    return (
      <div className="flex items-center justify-center py-8">
        <p className="text-muted-foreground">{t("table.noData")}</p>
      </div>
    );
  }

  return (

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
            <TableHead className="min-w-[180px]">{t("table.fullName")}</TableHead>
            <TableHead className="min-w-[180px]">{t("table.email")}</TableHead>
            <TableHead className="min-w-[100px]">{t("table.major")}</TableHead>
            <TableHead className="min-w-[100px]">{t("table.trustLevel")}</TableHead>
            <TableHead className="min-w-[80px]">{t("table.roles")}</TableHead>
            <TableHead className="min-w-[80px] text-center">{t("table.status")}</TableHead>
            <TableHead className="min-w-[100px]">{t("table.createdAt")}</TableHead>
            <TableHead className="text-right min-w-[200px]">{t("table.actions")}</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {users.map((user) => {
            const isSelected = user.id ? selectedIds.has(user.id) : false;
            const isBanned = user.lockoutEnd && new Date(user.lockoutEnd) > new Date();
            const isDeleted = user.isDeleted;

            return (
              <TableRow key={user.id} className={cn(isDeleted && "opacity-50 bg-muted/30")}>
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
                    <Avatar className="h-8 w-8 shrink-0">
                      <AvatarImage src={user.avatarUrl || undefined} alt={user.fullName} />
                      <AvatarFallback className="text-xs">
                        {getInitials(user.fullName)}
                      </AvatarFallback>
                    </Avatar>
                    <div className="min-w-0">
                      <div className="font-medium text-foreground truncate">{user.fullName}</div>
                      {user.username && (
                        <div className="text-xs text-muted-foreground">@{user.username}</div>
                      )}
                    </div>
                  </div>
                </TableCell>
                <TableCell>
                  <div className="flex items-center gap-1">
                    <span className="text-sm text-foreground">{user.email}</span>
                    {user.emailConfirmed ? (
                      <CheckCircle className="h-3.5 w-3.5 text-emerald-500" xlinkTitle="Email đã xác thực" />
                    ) : (
                      <XCircle className="h-3.5 w-3.5 text-amber-500" xlinkTitle="Chưa xác thực email" />
                    )}
                  </div>
                </TableCell>
                <TableCell>
                  {user.major ? (
                    <div className="text-sm text-foreground truncate max-w-[120px]" title={user.major.majorName}>
                      {user.major.majorName}
                    </div>
                  ) : (
                    <span className="text-muted-foreground">-</span>
                  )}
                </TableCell>
                <TableCell>
                  <div className="flex items-center gap-1">
                    <Star className="h-3.5 w-3.5 text-amber-500" />
                    <span className="text-sm font-medium">{user.trustScore ?? 0}</span>
                    <Badge variant="secondary" className="text-[10px] px-1.5">
                      {user.trustLevel || "None"}
                    </Badge>
                  </div>
                </TableCell>
                <TableCell>
                  <div className="flex gap-1 flex-wrap">
                    {user.roles?.map((role) => (
                      <Badge
                        key={role}
                        variant={role === "Admin" ? "default" : "secondary"}
                        className="text-[10px] px-1.5"
                      >
                        {role === "Admin" && <Shield className="h-3 w-3 mr-0.5" />}
                        {role}
                      </Badge>
                    ))}
                  </div>
                </TableCell>
                <TableCell className="text-center">
                  <div className="flex flex-col items-center gap-0.5">
                    {isBanned ? (
                      <Badge variant="destructive" className="text-[10px] px-1.5">
                        <Ban className="h-3 w-3 mr-0.5" />
                        Bị cấm
                      </Badge>
                    ) : isDeleted ? (
                      <Badge variant="outline" className="text-[10px] px-1.5 text-muted-foreground">
                        Đã xóa
                      </Badge>
                    ) : (
                      <Badge variant="secondary" className="text-[10px] px-1.5 text-emerald-600">
                        Hoạt động
                      </Badge>
                    )}
                  </div>
                </TableCell>
                <TableCell>
                  <div className="text-xs text-muted-foreground">
                    {user.createdAt ? new Date(user.createdAt).toLocaleDateString("vi-VN") : "-"}
                  </div>
                  {user.lastLoginAt && (
                    <div className="text-[10px] text-muted-foreground/70">
                      Đăng nhập: {new Date(user.lastLoginAt).toLocaleDateString("vi-VN")}
                    </div>
                  )}
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
                    {isBanned ? (
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

                  </div>
                </TableCell>
              </TableRow>
            );
          })}
        </TableBody>
      </Table>
    </div>
  );
}



