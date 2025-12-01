"use client";

import { Download, Eye, FileText } from "lucide-react";
import { Button } from "@/src/components/ui/button";
import type { DocumentFileDto, DocumentDetailDto } from "@/src/api/database/types.gen";

interface DocumentFileListProps {
  files: DocumentFileDto[];
  document: DocumentDetailDto;
}

export function DocumentFileList({ files, document }: DocumentFileListProps) {
  if (files.length === 0) {
    return (
      <div className="text-center py-8 text-slate-500 dark:text-slate-400">
        <FileText className="h-8 w-8 mx-auto mb-2 opacity-50" />
        <p className="text-sm">Chưa có chương/file nào</p>
      </div>
    );
  }

  return (
    <div className="space-y-2">
      {files.map((file, index) => {
        const fileUrl = file.fileUrl ?? "";
        const fileSize = file.fileSize ? `${(file.fileSize / 1024 / 1024).toFixed(2)} MB` : "";
        
        return (
          <div
            key={file.id}
            className="rounded-lg border border-slate-200 bg-white p-3 dark:border-slate-700 dark:bg-slate-800"
          >
            <div className="flex items-start gap-3">
              {file.coverUrl && (
                <img
                  src={file.coverUrl}
                  alt={file.title || "Cover"}
                  className="w-16 h-20 object-cover rounded flex-shrink-0"
                />
              )}
              <div className="flex-1 min-w-0">
                <div className="flex items-start justify-between gap-2">
                  <div className="flex-1 min-w-0">
                    <h4 className="font-medium text-sm text-foreground truncate">
                      {file.title || file.fileName || `Chương ${index + 1}`}
                    </h4>
                    <p className="text-xs text-slate-500 dark:text-slate-400 mt-0.5">
                      {file.fileName}
                      {fileSize && ` • ${fileSize}`}
                      {file.totalPages && ` • ${file.totalPages} trang`}
                    </p>
                  </div>
                  <div className="flex gap-1 flex-shrink-0">
                    <Button
                      type="button"
                      size="sm"
                      variant="outline"
                      onClick={() => window.open(fileUrl, "_blank")}
                      disabled={!fileUrl}
                      className="h-7 px-2"
                    >
                      <Eye className="h-3 w-3" />
                    </Button>
                    <Button
                      type="button"
                      size="sm"
                      variant="outline"
                      onClick={() => {
                        const link = document.createElement("a");
                        link.href = fileUrl;
                        link.target = "_blank";
                        link.download = file.fileName || "file";
                        document.body.appendChild(link);
                        link.click();
                        document.body.removeChild(link);
                      }}
                      disabled={!fileUrl || document.isDownload === false}
                      className="h-7 px-2"
                    >
                      <Download className="h-3 w-3" />
                    </Button>
                  </div>
                </div>
              </div>
            </div>
          </div>
        );
      })}
    </div>
  );
}

