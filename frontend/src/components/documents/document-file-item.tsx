"use client";

import { ThumbsUp, ThumbsDown, MessageSquare, FileText, MoreVertical, Flag, Pencil, Trash2, RefreshCw, EyeOff, Eye } from "lucide-react";
import type { DocumentFileDto } from "@/src/api/database/types.gen";
import { getFileUrlById } from "@/src/lib/file-url";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/src/components/ui/dropdown-menu";
import { Button } from "@/src/components/ui/button";
import { Badge } from "@/src/components/ui/badge";

// ContentStatus enum from backend
const ContentStatus = {
  PendingReview: 0,
  Approved: 1,
  Hidden: 2,
} as const;

interface DocumentFileItemProps {
  file: DocumentFileDto;
  index: number;
  documentName?: string;
  isOwner?: boolean;
  onEdit?: (file: DocumentFileDto) => void;
  onDelete?: (fileId: string) => void;
  onReport?: (fileId: string) => void;
  onResubmit?: (fileId: string) => void;
}

export function DocumentFileItem({
  file,
  index,
  documentName,
  isOwner = false,
  onEdit,
  onDelete,
  onReport,
  onResubmit,
}: DocumentFileItemProps) {
  const coverUrl = getFileUrlById(file.coverFileId);
  const fileSize = file.fileSize ? `${(file.fileSize / 1024 / 1024).toFixed(2)} MB` : "";
  const title = file.title || `${documentName || "Tài liệu"} (${index + 1})`;
  const useful = file.usefulCount ?? 0;
  const notUseful = file.notUsefulCount ?? 0;
  const commentCount = file.commentCount ?? 0;
  const viewCount = file.viewCount ?? 0;

  const handleEdit = (e: React.MouseEvent) => {
    e.preventDefault();
    e.stopPropagation();
    if (onEdit) {
      onEdit(file);
    }
  };

  const handleDelete = (e: React.MouseEvent) => {
    e.preventDefault();
    e.stopPropagation();
    if (file.id && onDelete) {
      onDelete(file.id);
    }
  };

  const handleReport = (e: React.MouseEvent) => {
    e.preventDefault();
    e.stopPropagation();
    if (file.id && onReport) {
      onReport(file.id);
    }
  };

  const handleResubmit = (e: React.MouseEvent) => {
    e.preventDefault();
    e.stopPropagation();
    if (file.id && onResubmit) {
      onResubmit(file.id);
    }
  };

  // Get status badge for owner
  const getStatusBadge = () => {
    if (!isOwner) return null;

    switch (file.status) {
      case ContentStatus.PendingReview:
        return (
          <Badge variant="outline" className="border-amber-300 bg-amber-50 text-amber-700 dark:border-amber-700 dark:bg-amber-950 dark:text-amber-300 text-[10px] gap-1">
            {/* <Clock className="h-3 w-3" /> */}
            Chờ duyệt
          </Badge>
        );
      case ContentStatus.Approved:
        return (
          <Badge variant="outline" className="border-green-300 bg-green-50 text-green-700 dark:border-green-700 dark:bg-green-950 dark:text-green-300 text-[10px] gap-1">
            {/* <CheckCircle className="h-3 w-3" /> */}
            Đã duyệt
          </Badge>
        );
      case ContentStatus.Hidden:
        return (
          <Badge variant="outline" className="border-red-300 bg-red-50 text-red-700 dark:border-red-700 dark:bg-red-950 dark:text-red-300 text-[10px] gap-1">
            {/* <EyeOff className="h-3 w-3" /> */}
            Bị ẩn
          </Badge>
        );
      default:
        return null;
    }
  };

  // Check if menu has any items to show
  const hasMenuItems = isOwner || (!isOwner && onReport);

  return (
    <div className="flex gap-3 p-3 border border-slate-200 bg-white dark:border-slate-700 dark:bg-slate-800 hover:border-sky-400 hover:shadow-md transition-all cursor-pointer ">
      {/* Thumbnail */}
      {coverUrl ? (
        <img
          src={coverUrl}
          alt={title}
          className="w-20 h-14 object-contain flex-shrink-0 bg-slate-100 dark:bg-slate-700"
        />
      ) : (
        <div className="w-20 h-14 bg-slate-600 dark:bg-slate-700 flex items-center justify-center flex-shrink-0">
          <FileText className="h-6 w-6 text-white/80" />
        </div>
      )}

      {/* Content */}
      <div className="flex-1 min-w-0 flex flex-col justify-center">
        <div className="flex items-center gap-2">
          <h4 className="font-medium text-sm text-foreground line-clamp-1 leading-tight">
            {title}
          </h4>
          {getStatusBadge()}
        </div>
        <p className="text-xs text-muted-foreground mt-0.5">
          {fileSize}
          {file.totalPages && ` • ${file.totalPages} trang`}
        </p>
      </div>

      {/* Stats */}
      <div className="flex items-center gap-3 text-xs text-muted-foreground flex-shrink-0">
        <div className="flex items-center gap-1" title="Lượt xem">
          <Eye className="h-3.5 w-3.5" />
          <span>{viewCount}</span>
        </div>
        <div className="flex items-center gap-1" title="Hữu ích">
          <ThumbsUp className="h-3.5 w-3.5" />
          <span>{useful}</span>
        </div>
        <div className="flex items-center gap-1" title="Không hữu ích">
          <ThumbsDown className="h-3.5 w-3.5" />
          <span>{notUseful}</span>
        </div>
        <div className="flex items-center gap-1" title="Bình luận">
          <MessageSquare className="h-3.5 w-3.5" />
          <span>{commentCount}</span>
        </div>
      </div>

      {/* Dropdown Menu */}
      {hasMenuItems && (
        <DropdownMenu>
          <DropdownMenuTrigger asChild onClick={(e) => e.preventDefault()}>
            <Button
              variant="ghost"
              size="icon"
              className="h-8 w-8 flex-shrink-0"
              onClick={(e) => e.stopPropagation()}
            >
              <MoreVertical className="h-4 w-4" />
            </Button>
          </DropdownMenuTrigger>
          <DropdownMenuContent align="end">
            {/* Xem lý do ẩn - hiển thị đầu tiên nếu file bị ẩn và có lý do */}
            {isOwner && file.status === ContentStatus.Hidden && file.reviewNote && (
              <DropdownMenuItem
                onClick={(e) => {
                  e.preventDefault();
                  e.stopPropagation();
                  alert(`Lý do ẩn file:\n\n${file.reviewNote}`);
                }}
                className="text-amber-600 focus:text-amber-600"
              >
                <EyeOff className="h-4 w-4 mr-2" />
                Xem lý do ẩn
              </DropdownMenuItem>
            )}
            {/* Owner actions */}
            {isOwner && onEdit && (
              <DropdownMenuItem onClick={handleEdit}>
                <Pencil className="h-4 w-4 mr-2" />
                Chỉnh sửa
              </DropdownMenuItem>
            )}
            {/* Resubmit - only for owner when file is hidden */}
            {isOwner && file.status === ContentStatus.Hidden && onResubmit && (
              <DropdownMenuItem onClick={handleResubmit} className="text-blue-600 focus:text-blue-600">
                <RefreshCw className="h-4 w-4 mr-2" />
                Gửi duyệt lại
              </DropdownMenuItem>
            )}
            {isOwner && onDelete && (
              <DropdownMenuItem onClick={handleDelete} className="text-red-600 focus:text-red-600">
                <Trash2 className="h-4 w-4 mr-2" />
                Xóa
              </DropdownMenuItem>
            )}
            {/* Report - only for non-owner */}
            {!isOwner && onReport && (
              <DropdownMenuItem onClick={handleReport} className="text-red-600 focus:text-red-600">
                <Flag className="h-4 w-4 mr-2" />
                Báo cáo
              </DropdownMenuItem>
            )}
          </DropdownMenuContent>
        </DropdownMenu>
      )}
    </div>
  );
}
