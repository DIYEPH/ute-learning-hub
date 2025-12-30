"use client";

import { useState, useCallback } from "react";
import Link from "next/link";
import { GripVertical } from "lucide-react";
import type { DocumentFileDto, DocumentDetailDto } from "@/src/api/database/types.gen";
import { postApiDocumentByDocumentIdFilesByFileIdResubmit } from "@/src/api";
import { DocumentFileItem } from "@/src/components/documents/document-file-item";
import { ReportModal } from "@/src/components/shared/report-modal";
import { useUserProfile } from "@/src/hooks/use-user-profile";
import { useNotification } from "@/src/components/providers/notification-provider";

interface DocumentFileListProps {
  files: DocumentFileDto[];
  document: DocumentDetailDto;
  onEdit?: (file: DocumentFileDto) => void;
  onDelete?: (fileId: string) => void;
  onReorder?: (files: DocumentFileDto[]) => void;
  onRefresh?: () => void;
}

export function DocumentFileList({
  files,
  document: doc,
  onEdit,
  onDelete,
  onReorder,
  onRefresh,
}: DocumentFileListProps) {
  const documentId = doc.id;
  const { profile } = useUserProfile();
  const { success: notifySuccess, error: notifyError } = useNotification();
  const [reportModalOpen, setReportModalOpen] = useState(false);
  const [reportingFile, setReportingFile] = useState<DocumentFileDto | null>(null);
  const [draggedIndex, setDraggedIndex] = useState<number | null>(null);
  const [dragOverIndex, setDragOverIndex] = useState<number | null>(null);

  // Check if current user is owner of the document
  const isOwner = profile?.id === doc.createdById;
  const canReorder = isOwner && !!onReorder;

  const handleReport = (fileId: string) => {
    const file = files.find((f) => f.id === fileId);
    if (file) {
      setReportingFile(file);
      setReportModalOpen(true);
    }
  };

  const handleResubmit = async (fileId: string) => {
    if (!documentId) return;

    try {
      await postApiDocumentByDocumentIdFilesByFileIdResubmit({
        path: { documentId, fileId },
      });
      notifySuccess("Đã gửi yêu cầu duyệt lại. Admin sẽ xem xét file của bạn.");
      onRefresh?.();
    } catch (err: any) {
      notifyError(err?.message || "Không thể gửi yêu cầu duyệt lại");
    }
  };

  const handleDragStart = useCallback((e: React.DragEvent, index: number) => {
    e.dataTransfer.effectAllowed = "move";
    e.dataTransfer.setData("text/plain", index.toString());
    setDraggedIndex(index);
  }, []);

  const handleDragOver = useCallback((e: React.DragEvent, index: number) => {
    e.preventDefault();
    e.dataTransfer.dropEffect = "move";
    if (draggedIndex !== null && draggedIndex !== index) {
      setDragOverIndex(index);
    }
  }, [draggedIndex]);

  const handleDragEnd = useCallback(() => {
    setDraggedIndex(null);
    setDragOverIndex(null);
  }, []);

  const handleDrop = useCallback((e: React.DragEvent, targetIndex: number) => {
    e.preventDefault();
    if (draggedIndex === null || draggedIndex === targetIndex) {
      handleDragEnd();
      return;
    }

    // Reorder files
    const newFiles = [...files];
    const [draggedItem] = newFiles.splice(draggedIndex, 1);
    newFiles.splice(targetIndex, 0, draggedItem);

    // Update order values
    const reorderedFiles = newFiles.map((f, idx) => ({
      ...f,
      order: idx,
    }));

    onReorder?.(reorderedFiles);
    handleDragEnd();
  }, [draggedIndex, files, onReorder, handleDragEnd]);

  return (
    <>
      <div className="flex flex-col gap-2">
        {files.map((file, index) => {
          const detailHref =
            documentId && file.id
              ? `/documents/${documentId}/files/${file.id}`
              : undefined;

          const isDragging = draggedIndex === index;
          const isDragOver = dragOverIndex === index;

          // File item content
          const fileContent = (
            <DocumentFileItem
              file={file}
              index={index}
              documentName={doc.documentName}
              isOwner={isOwner}
              onEdit={onEdit}
              onDelete={onDelete}
              onReport={handleReport}
              onResubmit={handleResubmit}
            />
          );

          // Wrapped with link if detail href exists
          const linkedContent = detailHref ? (
            <Link href={detailHref} className="block flex-1">
              {fileContent}
            </Link>
          ) : (
            <div className="flex-1">{fileContent}</div>
          );

          return (
            <div
              key={file.id}
              className={`flex items-center gap-2 ${isDragging ? "opacity-50" : ""} ${isDragOver ? "border-t-2 border-primary" : ""}`}
              onDragOver={(e) => canReorder && handleDragOver(e, index)}
              onDrop={(e) => canReorder && handleDrop(e, index)}
            >
              {/* Drag handle - only show for owner */}
              {canReorder && (
                <div
                  draggable
                  onDragStart={(e) => handleDragStart(e, index)}
                  onDragEnd={handleDragEnd}
                  className="cursor-grab active:cursor-grabbing p-1 text-muted-foreground hover:text-foreground flex-shrink-0"
                >
                  <GripVertical className="h-5 w-5" />
                </div>
              )}

              {linkedContent}
            </div>
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
