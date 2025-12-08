"use client";

import { ThumbsUp, ThumbsDown, MessageSquare } from "lucide-react";
import type { DocumentFileDto } from "@/src/api/database/types.gen";
import { getFileUrlById } from "@/src/lib/file-url";

interface DocumentFileItemProps {
  file: DocumentFileDto;
  index: number;
  documentName?: string;
}

export function DocumentFileItem({ file, index, documentName }: DocumentFileItemProps) {
  const coverUrl = getFileUrlById(file.coverFileId);
  const fileSize = file.fileSize ? `${(file.fileSize / 1024 / 1024).toFixed(2)} MB` : "";
  const title = file.title || `${documentName || "Tài liệu"} (${index + 1})`;
  const useful = file.usefulCount ?? 0;
  const notUseful = file.notUsefulCount ?? 0;
  const commentCount = file.commentCount ?? 0;

  return (
    <div className="flex h-full flex-col border border-slate-200 bg-white shadow-sm dark:border-slate-700 dark:bg-slate-800 overflow-hidden hover:border-sky-400 hover:shadow-md transition-all cursor-pointer">
      {coverUrl && (
        <img
          src={coverUrl}
          alt={title}
          className="w-full aspect-[5/3] object-cover"
        />
      )}
      <div className="flex flex-1 flex-col gap-1 p-2">
        <div className="flex-1 min-w-0">
          <h4 className="font-medium text-xs text-foreground line-clamp-2 leading-tight">
            {title}
          </h4>
          <p className="text-[10px] text-slate-500 dark:text-slate-400 mt-0.5 line-clamp-1">
            {fileSize}
            {file.totalPages && ` • ${file.totalPages} trang`}
          </p>
        </div>

        <div className="flex items-center gap-2 text-[10px] text-slate-500 dark:text-slate-400">
          <div className="flex items-center gap-0.5" title="Hữu ích">
            <ThumbsUp className="h-3 w-3 flex-shrink-0" />
            <span>{useful}</span>
          </div>
          <div className="flex items-center gap-0.5" title="Không hữu ích">
            <ThumbsDown className="h-3 w-3 flex-shrink-0" />
            <span>{notUseful}</span>
          </div>
          <div className="flex items-center gap-0.5" title="Bình luận">
            <MessageSquare className="h-3 w-3 flex-shrink-0" />
            <span>{commentCount}</span>
          </div>
        </div>
      </div>
    </div>
  );
}

