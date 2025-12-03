"use client";

import Link from "next/link";
import { FileText } from "lucide-react";
import type { DocumentFileDto, DocumentDetailDto } from "@/src/api/database/types.gen";
import { DocumentFileItem } from "@/src/components/documents/document-file-item";

interface DocumentFileListProps {
  files: DocumentFileDto[];
  document: DocumentDetailDto;
}

export function DocumentFileList({ files, document: doc }: DocumentFileListProps) {
  if (files.length === 0) {
    return (
      <div className="text-center py-8 text-slate-500 dark:text-slate-400">
        <FileText className="h-8 w-8 mx-auto mb-2 opacity-50" />
        <p className="text-sm">Chưa có chương/file nào</p>
      </div>
    );
  }

  const canDownload = doc.isDownload !== false;
  const documentId = doc.id;

  return (
    <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-3">
      {files.map((file, index) => {
        const detailHref =
          documentId && file.id
            ? `/documents/${documentId}/files/${file.id}`
            : undefined;

        const content = (
          <DocumentFileItem
            key={file.id}
            file={file}
            index={index}
            canDownload={canDownload}
          />
        );

        if (!detailHref) {
          return content;
        }

        return (
          <Link key={file.id} href={detailHref} className="block h-full">
            {content}
          </Link>
        );
      })}
    </div>
  );
}


