"use client";

import { Pencil, Trash2, AlertTriangle, MoreVertical } from "lucide-react";

import { Button } from "@/src/components/ui/button";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
  DropdownMenuSeparator,
} from "@/src/components/ui/dropdown-menu";

export interface DocumentMenuProps {
  onEdit?: () => void;
  onDelete?: () => void;
  onReport?: () => void;
  disabled?: boolean;
}

export function DocumentMenu({
  onEdit,
  onDelete,
  onReport,
  disabled = false,
}: DocumentMenuProps) {
  const handleSelect = (
    event: Event,
    callback?: () => void
  ) => {
    event.preventDefault();
    if (disabled || !callback) return;
    callback();
  };

  return (
    <DropdownMenu>
      <DropdownMenuTrigger asChild>
        <Button
          type="button"
          variant="ghost"
          size="icon"
          className="h-8 w-8 rounded-full border border-slate-200 bg-white/80 text-slate-600 shadow-sm transition hover:border-sky-200 hover:text-sky-600 dark:border-slate-700 dark:bg-slate-900/70 dark:text-slate-200"
          disabled={disabled && !onEdit && !onDelete && !onReport}
        >
          <MoreVertical className="h-4 w-4" />
        </Button>
      </DropdownMenuTrigger>
      <DropdownMenuContent align="end" className="w-40">
        <DropdownMenuItem
          onSelect={(event) => handleSelect(event, onEdit)}
          disabled={!onEdit || disabled}
        >
          <Pencil className="h-4 w-4" />
          Chỉnh sửa
        </DropdownMenuItem>

        <DropdownMenuItem
          onSelect={(event) => handleSelect(event, onDelete)}
          disabled={!onDelete || disabled}
          variant="destructive"
        >
          <Trash2 className="h-4 w-4" />
          Xóa
        </DropdownMenuItem>

        <DropdownMenuSeparator />

        <DropdownMenuItem
          onSelect={(event) => handleSelect(event, onReport)}
          disabled={!onReport || disabled}
        >
          <AlertTriangle className="h-4 w-4" />
          Báo cáo
        </DropdownMenuItem>
      </DropdownMenuContent>
    </DropdownMenu>
  );
}



