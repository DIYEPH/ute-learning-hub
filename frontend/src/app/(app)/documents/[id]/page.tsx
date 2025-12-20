"use client";

import { useEffect, useState } from "react";
import { useParams, useRouter } from "next/navigation";
import Link from "next/link";
import { ArrowLeft, Loader2, FileText, Edit } from "lucide-react";

import { getApiDocumentById, deleteApiDocumentByDocumentIdFilesByFileId, putApiDocumentByDocumentIdFilesByFileId } from "@/src/api/database/sdk.gen";
import type { DocumentDetailDto, DocumentFileDto } from "@/src/api/database/types.gen";
import { Button } from "@/src/components/ui/button";
import { Badge } from "@/src/components/ui/badge";
import { EditDocumentModal } from "@/src/components/documents/edit-document-modal";
import { EditDocumentFileModal } from "@/src/components/documents/edit-document-file-modal";
import { DocumentFileUpload } from "@/src/components/documents/document-file-upload";
import { DocumentFileList } from "@/src/components/documents/document-file-list";
import { useUserProfile } from "@/src/hooks/use-user-profile";
import { useNotification } from "@/src/components/providers/notification-provider";

export default function DocumentDetailPage() {
  const params = useParams<{ id: string }>();
  const router = useRouter();
  const documentId = params?.id;
  const { profile } = useUserProfile();

  const [data, setData] = useState<DocumentDetailDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [showEditModal, setShowEditModal] = useState(false);
  const [showEditFileModal, setShowEditFileModal] = useState(false);
  const [editingFile, setEditingFile] = useState<DocumentFileDto | null>(null);
  const [showFullDescription, setShowFullDescription] = useState(false);
  const { success: notifySuccess, error: notifyError } = useNotification();

  // Check if current user is the owner
  const isOwner = profile?.id && data?.createdById && profile.id === data.createdById;

  useEffect(() => {
    if (!documentId) return;
    let cancelled = false;

    const fetchDetail = async () => {
      setLoading(true);
      setError(null);
      try {
        const response = await getApiDocumentById({
          path: { id: documentId },
        });

        const payload = (response.data ??
          response) as DocumentDetailDto | undefined;
        // Check if payload is a valid document (has id) and not an error object
        if (!cancelled && payload && typeof payload === 'object' && 'id' in payload) {
          setData(payload);
        }
      } catch (err: any) {
        if (cancelled) return;
        const message =
          err?.response?.data?.message ||
          err?.message ||
          "Không thể tải chi tiết tài liệu";
        setError(message);
      } finally {
        if (!cancelled) {
          setLoading(false);
        }
      }
    };

    void fetchDetail();

    return () => {
      cancelled = true;
    };
  }, [documentId]);



  const refreshData = async () => {
    if (!documentId) return;
    try {
      const response = await getApiDocumentById({
        path: { id: documentId },
      });
      const payload = (response.data ?? response) as DocumentDetailDto | undefined;
      // Check if payload is a valid document (has id) and not an error object
      if (payload && typeof payload === 'object' && 'id' in payload) {
        setData(payload);
      }
    } catch (err) {
      console.error("Error refreshing data:", err);
    }
  };

  const handleEditFile = (file: DocumentFileDto) => {
    setEditingFile(file);
    setShowEditFileModal(true);
  };

  const handleDeleteFile = async (fileId: string) => {
    if (!documentId || !confirm("Bạn có chắc muốn xóa chương/file này?")) return;
    try {
      await deleteApiDocumentByDocumentIdFilesByFileId({
        path: { documentId, fileId },
      });
      notifySuccess("Đã xóa file thành công");
      await refreshData();
    } catch (err: any) {
      notifyError(err?.message || "Không thể xóa file");
    }
  };

  const handleReorder = async (reorderedFiles: DocumentFileDto[]) => {
    if (!documentId) return;

    // Optimistic update
    setData(prev => prev ? { ...prev, files: reorderedFiles } : prev);

    // Update each file's order in the backend
    try {
      await Promise.all(
        reorderedFiles.map((file, index) =>
          file.id
            ? putApiDocumentByDocumentIdFilesByFileId({
              path: { documentId, fileId: file.id },
              body: {
                documentId,
                documentFileId: file.id,
                order: index,
              },
            })
            : Promise.resolve()
        )
      );
      notifySuccess("Đã cập nhật thứ tự các chương");
    } catch (err: any) {
      notifyError(err?.message || "Không thể cập nhật thứ tự");
      await refreshData(); // Revert on error
    }
  };

  if (!documentId) {
    return (
      <div className="space-y-4">
        <p className="text-sm text-red-600 dark:text-red-400">
          Không tìm thấy mã tài liệu hợp lệ.
        </p>
      </div>
    );
  }

  if (loading && !data) {
    return (
      <div className="flex items-center justify-center py-12">
        <Loader2 className="h-6 w-6 animate-spin text-sky-500" />
      </div>
    );
  }

  if (error && !data) {
    return (
      <div className="space-y-4">
        <Button
          variant="ghost"
          size="sm"
          onClick={() => router.back()}
          className="inline-flex items-center gap-2"
        >
          <ArrowLeft className="h-4 w-4" />
          Quay lại
        </Button>
        <div className=" border border-red-200 bg-red-50 p-4 text-sm text-red-600 dark:border-red-900 dark:bg-red-950 dark:text-red-300">
          {error}
        </div>
      </div>
    );
  }

  // Guard against null data
  if (!data) {
    return (
      <div className="flex items-center justify-center py-12">
        <Loader2 className="h-6 w-6 animate-spin text-sky-500" />
      </div>
    );
  }

  const doc = data;
  const files = doc.files ?? [];
  const tags = doc.tags ?? [];
  const authors = doc.authors ?? [];

  return (
    <div className="flex flex-col gap-4 lg:gap-6">
      <div className="flex items-center justify-between gap-3">
        <Button
          variant="ghost"
          size="sm"
          onClick={() => router.back()}
          className="inline-flex items-center gap-2"
        >
          <ArrowLeft className="h-4 w-4" />
          Quay lại
        </Button>
        {isOwner && (
          <Button
            variant="outline"
            size="sm"
            onClick={() => setShowEditModal(true)}
            className="inline-flex items-center gap-1"
          >
            <Edit className="h-4 w-4" />
            Sửa thông tin
          </Button>
        )}
      </div>

      {/* Thông tin document + upload + danh sách file */}
      <div className="flex flex-col gap-4">
        {/* Thông tin document - 2 column layout */}
        <div className=" border border-slate-200 bg-white p-4 shadow-sm dark:border-slate-700 dark:bg-slate-900">
          <div className="grid gap-4 md:grid-cols-2">
            {/* Left: Title, Author, Type badges */}
            <div className="space-y-2">
              <h1 className="text-xl font-semibold text-foreground">
                {doc.documentName ?? "Tài liệu"}
              </h1>
              {authors.length > 0 && (
                <p className="text-xs text-muted-foreground">
                  Tác giả:{" "}
                  <span className="font-medium">
                    {authors.map((a) => a.fullName).join(", ")}
                  </span>
                </p>
              )}
              {/* Người đăng */}
              {doc.createdById && (
                <Link href={`/profile/${doc.createdById}`} className="inline-flex items-center gap-2 text-xs text-muted-foreground hover:text-foreground">
                  <span>Người đăng:</span>
                  {doc.createdByAvatarUrl && (
                    <img
                      src={doc.createdByAvatarUrl}
                      alt={doc.createdByName || ""}
                      className="h-5 w-5 rounded-full object-cover"
                    />
                  )}
                  <span className="font-medium hover:underline">{doc.createdByName || "Người dùng"}</span>
                </Link>
              )}
              <div className="flex flex-wrap gap-1.5">
                {doc.type?.typeName && (
                  <Badge variant="outline" className="border-sky-200 text-sky-700">
                    {doc.type.typeName}
                  </Badge>
                )}
                {doc.visibility === 0 && (
                  <Badge variant="outline" className="border-blue-200 text-blue-700">
                    Công khai
                  </Badge>
                )}
                {doc.visibility === 1 && (
                  <Badge variant="outline" className="border-slate-200 text-slate-700">
                    Riêng tư
                  </Badge>
                )}
                {doc.visibility === 2 && (
                  <Badge variant="outline" className="border-amber-200 text-amber-700">
                    Nội bộ
                  </Badge>
                )}
              </div>
            </div>

            {/* Right: Tags, Description */}
            <div className="space-y-2">
              {tags.length > 0 && (
                <div className="flex flex-wrap gap-1.5">
                  {tags.map((tag) => (
                    <Badge
                      key={tag.id}
                      variant="secondary"
                      className="bg-slate-100 text-slate-700 dark:bg-slate-800 dark:text-slate-200 text-[11px]"
                    >
                      #{tag.tagName}
                    </Badge>
                  ))}
                </div>
              )}

              {doc.description && (
                <div>
                  <h2 className="text-xs font-semibold text-foreground mb-1">Mô tả</h2>
                  <p
                    className={`text-sm text-muted-foreground whitespace-pre-wrap ${!showFullDescription ? "line-clamp-2" : ""
                      }`}
                  >
                    {doc.description}
                  </p>
                  {doc.description.length > 100 && (
                    <button
                      onClick={() => setShowFullDescription(!showFullDescription)}
                      className="text-xs text-sky-600 hover:text-sky-700 mt-1"
                    >
                      {showFullDescription ? "Thu gọn" : "Xem thêm"}
                    </button>
                  )}
                </div>
              )}
            </div>
          </div>

          {/* Form upload - full width below */}
          {isOwner && (
            <div className="mt-4 pt-4 border-t border-slate-200 dark:border-slate-700">
              <DocumentFileUpload
                documentId={documentId}
                onUploadSuccess={refreshData}
              />
            </div>
          )}
        </div>

        {/* Danh sách chương/file */}
        <div className=" border border-slate-200 bg-white shadow-sm dark:border-slate-700 dark:bg-slate-900">
          <div className="border-b border-slate-200 px-4 py-3 dark:border-slate-700 flex items-center justify-between">
            <div className="flex items-center gap-2 text-sm font-semibold text-foreground">
              <FileText className="h-4 w-4" />
              <span>Chương / file ({files.length})</span>
            </div>
          </div>
          <div className="p-4">
            <DocumentFileList
              files={files}
              document={doc}
              onEdit={handleEditFile}
              onDelete={handleDeleteFile}
              onReorder={handleReorder}
              onRefresh={refreshData}
            />
          </div>
        </div>
      </div>

      {data && (
        <EditDocumentModal
          open={showEditModal}
          onOpenChange={setShowEditModal}
          document={data}
          onSuccess={refreshData}
        />
      )}

      {/* Modal chỉnh sửa file */}
      {editingFile && documentId && (
        <EditDocumentFileModal
          open={showEditFileModal}
          onOpenChange={(open: boolean) => {
            setShowEditFileModal(open);
            if (!open) setEditingFile(null);
          }}
          documentId={documentId}
          file={editingFile}
          onSuccess={refreshData}
        />
      )}
    </div>
  );
}

