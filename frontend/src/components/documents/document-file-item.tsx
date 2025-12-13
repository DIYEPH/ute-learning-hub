"use client";

import { ThumbsUp, ThumbsDown, MessageSquare, FileText, MoreVertical, Flag } from "lucide-react";
import type { DocumentFileDto } from "@/src/api/database/types.gen";
import { getFileUrlById } from "@/src/lib/file-url";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/src/components/ui/dropdown-menu";
import { Button } from "@/src/components/ui/button";

interface DocumentFileItemProps {
  file: DocumentFileDto;
  index: number;
  documentName?: string;
  onReport?: (fileId: string) => void;
}

export function DocumentFileItem({ file, index, documentName, onReport }: DocumentFileItemProps) {
  const coverUrl = getFileUrlById(file.coverFileId);
  const fileSize = file.fileSize ? `${(file.fileSize / 1024 / 1024).toFixed(2)} MB` : "";
  const title = file.title || `${documentName || "Tài liệu"} (${index + 1})`;
  const useful = file.usefulCount ?? 0;
  const notUseful = file.notUsefulCount ?? 0;
  const commentCount = file.commentCount ?? 0;

  const handleReport = (e: React.MouseEvent) => {
    e.preventDefault();
    e.stopPropagation();
    if (file.id && onReport) {
      onReport(file.id);
    }
  };

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
        <h4 className="font-medium text-sm text-foreground line-clamp-1 leading-tight">
          {title}
        </h4>
        <p className="text-xs text-muted-foreground mt-0.5">
          {fileSize}
          {file.totalPages && ` • ${file.totalPages} trang`}
        </p>
      </div>

      {/* Stats */}
      <div className="flex items-center gap-3 text-xs text-muted-foreground flex-shrink-0">
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
          <DropdownMenuItem onClick={handleReport} className="text-red-600 focus:text-red-600">
            <Flag className="h-4 w-4 mr-2" />
            Báo cáo
          </DropdownMenuItem>
        </DropdownMenuContent>
      </DropdownMenu>
    </div>
  );
}

