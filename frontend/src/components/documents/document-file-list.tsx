"use client";

import { useState } from "react";
import Link from "next/link";
import type { DocumentFileDto, DocumentDetailDto } from "@/src/api/database/types.gen";
import { DocumentFileItem } from "@/src/components/documents/document-file-item";
import { ReportModal } from "@/src/components/shared/report-modal";

interface DocumentFileListProps {
  files: DocumentFileDto[];
  document: DocumentDetailDto;
}

export function DocumentFileList({ files, document: doc }: DocumentFileListProps) {
  const documentId = doc.id;
  const [reportModalOpen, setReportModalOpen] = useState(false);
  const [reportingFile, setReportingFile] = useState<DocumentFileDto | null>(null);

  const handleReport = (fileId: string) => {
    const file = files.find((f) => f.id === fileId);
    if (file) {
      setReportingFile(file);
      setReportModalOpen(true);
    }
  };

  return (
    <>
      <div className="flex flex-col gap-2">
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
              onReport={handleReport}
            />
          );

          if (!detailHref) {
            return content;
          }

          return (
            <Link key={file.id} href={detailHref} className="block">
              {content}
            </Link>
          );
        })}
      </div>

      {/* Report Modal */}
      <ReportModal
        open={reportModalOpen}
        onOpenChange={setReportModalOpen}
        targetType="documentFile"
        targetId={reportingFile?.id || ""}
        targetTitle={reportingFile?.title || doc.documentName || "Tài liệu"}
      />
    </>
  );
}

