"use client";

import Link from "next/link";
import type { DocumentFileDto, DocumentDetailDto } from "@/src/api/database/types.gen";
import { DocumentFileItem } from "@/src/components/documents/document-file-item";

interface DocumentFileListProps {
  files: DocumentFileDto[];
  document: DocumentDetailDto;
}

export function DocumentFileList({ files, document: doc }: DocumentFileListProps) {
  const documentId = doc.id;

  return (
    <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-4 xl:grid-cols-5 gap-2">
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
            documentName={doc.documentName}
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


