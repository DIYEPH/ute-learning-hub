"use client";

import { Download, Eye, ThumbsUp, ThumbsDown, MessageSquare } from "lucide-react";
import { Button } from "@/src/components/ui/button";
import type { DocumentFileDto } from "@/src/api/database/types.gen";
import { getFileUrlById } from "@/src/lib/file-url";

interface DocumentFileItemProps {
  file: DocumentFileDto;
  index: number;
  canDownload: boolean;
}

export function DocumentFileItem({ file, index, canDownload }: DocumentFileItemProps) {
  const fileUrl = getFileUrlById(file.fileId);
  const coverUrl = getFileUrlById(file.coverFileId);
  const fileSize = file.fileSize ? `${(file.fileSize / 1024 / 1024).toFixed(2)} MB` : "";
  const title = file.title || `Chương ${index + 1}`;
  const useful = file.usefulCount ?? 0;
  const notUseful = file.notUsefulCount ?? 0;
  const commentCount = file.commentCount ?? 0;

  const handleDownload = () => {
    if (!fileUrl || !canDownload) return;
    const link = window.document.createElement("a");
    link.href = fileUrl;
    link.target = "_blank";
    link.download = file.title || "file";
    window.document.body.appendChild(link);
    link.click();
    window.document.body.removeChild(link);
  };

  return (
    <div className="flex h-full flex-col rounded-xl border border-slate-200 bg-white shadow-sm dark:border-slate-700 dark:bg-slate-800 overflow-hidden">
      {coverUrl && (
        <img
          src={coverUrl}
          alt={title}
          className="w-full aspect-[4/3] object-cover"
        />
      )}
      <div className="flex flex-1 flex-col gap-2 p-3">
        <div className="flex-1 min-w-0">
          <h4 className="font-medium text-sm text-foreground line-clamp-2">
            {title}
          </h4>
          <p className="text-xs text-slate-500 dark:text-slate-400 mt-0.5 line-clamp-2">
            {fileSize}
            {file.totalPages && ` • ${file.totalPages} trang`}
          </p>
        </div>

        <div className="mt-1 flex items-center gap-3 text-[11px] text-slate-500 dark:text-slate-400">
          <div className="flex items-center gap-1" title="Hữu ích">
            <ThumbsUp className="h-3.5 w-3.5 flex-shrink-0" />
            <span>{useful}</span>
          </div>
          <div className="flex items-center gap-1" title="Không hữu ích">
            <ThumbsDown className="h-3.5 w-3.5 flex-shrink-0" />
            <span>{notUseful}</span>
          </div>
          <div className="flex items-center gap-1" title="Bình luận">
            <MessageSquare className="h-3.5 w-3.5 flex-shrink-0" />
            <span>{commentCount}</span>
          </div>
        </div>

        <div className="mt-2 flex gap-2">
          <Button type="button" size="sm" variant="outline" className="h-7 px-2 flex-1 justify-center">
            <Eye className="h-3 w-3 mr-1" />
            Xem
          </Button>
          <Button type="button" size="sm" variant="outline" onClick={handleDownload} disabled={!fileUrl || !canDownload} className="h-7 px-2 flex-1 justify-center">
            <Download className="h-3 w-3 mr-1" />
            Tải
          </Button>
        </div>
      </div>
    </div>
  );
}
