"use client";

import { useEffect, useState } from "react";
import { useParams, useRouter } from "next/navigation";
import Link from "next/link";
import { ArrowLeft, Loader2, FileText, Edit, Eye } from "lucide-react";

import {
  getApiDocumentById,
  deleteApiDocumentByDocumentIdFilesByFileId,
  putApiDocumentByDocumentIdFilesByFileId,
} from "@/src/api/database/sdk.gen";
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
  const { id: documentId } = useParams<{ id: string }>();
  const router = useRouter();
  const { profile } = useUserProfile();
  const { success, error } = useNotification();

  const [data, setData] = useState<DocumentDetailDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [showEditModal, setShowEditModal] = useState(false);
  const [showEditFileModal, setShowEditFileModal] = useState(false);
  const [editingFile, setEditingFile] = useState<DocumentFileDto | null>(null);
  const [showFullDescription, setShowFullDescription] = useState(false);

  const isOwner = profile?.id === data?.createdById;

  /* ======================= DATA ======================= */

  const fetchDetail = async () => {
    if (!documentId) return;
    setLoading(true);
    try {
      const res = await getApiDocumentById({ path: { id: documentId } });
      const payload = (res.data ?? res) as DocumentDetailDto;
      if (payload?.id) setData(payload);
    } catch (e: any) {
      error(e?.message || "Không thể tải tài liệu");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { void fetchDetail(); }, [documentId]);

  /* ======================= ACTIONS ======================= */

  const handleDeleteFile = async (fileId: string) => {
    if (!documentId || !confirm("Xóa file này?")) return;
    try {
      await deleteApiDocumentByDocumentIdFilesByFileId({ path: { documentId, fileId } });
      success("Đã xóa file");
      await fetchDetail();
    } catch (e: any) {
      error(e?.message || "Không thể xóa file");
    }
  };

  const handleReorder = async (files: DocumentFileDto[]) => {
    if (!documentId) return;
    setData(prev => prev ? { ...prev, files } : prev);
    try {
      await Promise.all(
        files.map((f, i) =>
          f.id
            ? putApiDocumentByDocumentIdFilesByFileId({
              path: { documentId, fileId: f.id },
              body: { order: i },
            })
            : Promise.resolve()
        )
      );
      success("Đã cập nhật thứ tự");
    } catch (e: any) {
      error(e?.message || "Cập nhật thất bại");
      await fetchDetail();
    }
  };

  /* ======================= GUARDS ======================= */

  if (!documentId)
    return <p className="text-sm text-red-600">Không có mã tài liệu</p>;

  if (loading && !data)
    return <div className="flex justify-center py-12"><Loader2 className="animate-spin" /></div>;

  if (!data)
    return <div className="flex justify-center py-12"><Loader2 className="animate-spin" /></div>;

  const { files = [], tags = [], authors = [] } = data;

  /* ======================= UI ======================= */

  return (
    <div className="flex flex-col gap-4 lg:gap-6">

      {/* Header */}
      <div className="flex justify-between">
        <Button variant="ghost" size="sm" onClick={() => router.back()}>
          <ArrowLeft className="h-4 w-4 mr-1" /> Quay lại
        </Button>
        {isOwner && (
          <Button variant="outline" size="sm" onClick={() => setShowEditModal(true)}>
            <Edit className="h-4 w-4 mr-1" /> Sửa
          </Button>
        )}
      </div>

      {/* Info */}
      <div className="border p-4 bg-card">
        <div className="grid md:grid-cols-3 gap-4">

          {/* Left: Main info - spans 2 columns */}
          <div className="md:col-span-2 grid md:grid-cols-2 gap-4">
            <div>
              <h1 className="text-xl font-semibold">{data.documentName}</h1>
              {authors.length > 0 && <p className="text-xs">Tác giả: {authors.map(a => a.fullName).join(", ")}</p>}
              {data.createdById && (
                <Link href={`/profile/${data.createdById}`} className="text-xs flex gap-2 mt-1">
                  {data.createdByAvatarUrl && <img src={data.createdByAvatarUrl} className="h-5 w-5 rounded-full" />}
                  <span>{data.createdByName}</span>
                </Link>
              )}
              <div className="flex gap-1 mt-2">
                {data.type?.typeName && <Badge variant="outline">{data.type.typeName}</Badge>}
              </div>
            </div>

            <div>
              <div className="flex flex-wrap gap-1">
                {tags.map(t => <Badge key={t.id} variant="secondary">#{t.tagName}</Badge>)}
              </div>
              {data.description && (
                <>
                  <p className={`text-sm mt-2 ${!showFullDescription && "line-clamp-2"}`}>
                    {data.description}
                  </p>
                  {data.description.length > 100 && (
                    <button className="text-xs text-primary" onClick={() => setShowFullDescription(v => !v)}>
                      {showFullDescription ? "Thu gọn" : "Xem thêm"}
                    </button>
                  )}
                </>
              )}
            </div>
          </div>

          {/* Right: Stats box */}
          <div className="flex flex-col items-center justify-center border rounded-lg p-4 bg-muted">
            <span className="text-xs text-muted-foreground uppercase tracking-wide mb-1">
              Tổng số lượt xem
            </span>
            <div className="flex items-center gap-2">
              <span className="text-3xl font-bold text-foreground">
                {data.totalViewCount?.toLocaleString() ?? 0}
              </span>
            </div>
          </div>

        </div>

        {isOwner && (
          <div className="mt-4 pt-4 border-t">
            <DocumentFileUpload documentId={documentId} onUploadSuccess={fetchDetail} />
          </div>
        )}
      </div>

      {/* Files */}
      <div className="border bg-card">
        <div className="border-b px-4 py-3 flex items-center gap-2">
          <FileText className="h-4 w-4" /> Chương / file ({files.length})
        </div>
        <div className="p-4">
          <DocumentFileList
            files={files}
            document={data}
            onEdit={f => { setEditingFile(f); setShowEditFileModal(true); }}
            onDelete={handleDeleteFile}
            onReorder={handleReorder}
            onRefresh={fetchDetail}
          />
        </div>
      </div>

      {/* Modals */}
      <EditDocumentModal open={showEditModal} onOpenChange={setShowEditModal} document={data} onSuccess={fetchDetail} />

      {editingFile && (
        <EditDocumentFileModal
          open={showEditFileModal}
          onOpenChange={o => { setShowEditFileModal(o); if (!o) setEditingFile(null); }}
          documentId={documentId}
          file={editingFile}
          onSuccess={fetchDetail}
        />
      )}

    </div>
  );
}
