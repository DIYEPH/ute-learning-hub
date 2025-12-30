"use client";

import { useEffect, useState } from "react";
import { useParams, useRouter } from "next/navigation";
import Link from "next/link";
import { ArrowLeft, Loader2, FileText, Edit, Eye, ThumbsUp, BookOpen, Plus, ChevronUp } from "lucide-react";
import { getFileUrlById } from "@/src/lib/file-url";

import {
  getApiDocumentById,
  deleteApiDocumentByDocumentIdFilesByFileId,
  putApiDocumentByDocumentIdFilesByFileId,
} from "@/src/api";
import type { DocumentDetailDto, DocumentFileDto } from "@/src/api/database/types.gen";

import { Button } from "@/src/components/ui/button";
import { Badge } from "@/src/components/ui/badge";
import { Avatar, AvatarFallback, AvatarImage } from "@/src/components/ui/avatar";
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
  const [showUploadForm, setShowUploadForm] = useState(false);

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

  const handleUploadSuccess = () => {
    setShowUploadForm(false);
    fetchDetail();
  };

  /* ======================= GUARDS ======================= */

  if (!documentId)
    return <p className="text-sm text-red-600">Không có mã tài liệu</p>;

  if (loading && !data)
    return <div className="flex justify-center py-12"><Loader2 className="animate-spin text-primary" /></div>;

  if (!data)
    return <div className="flex justify-center py-12"><Loader2 className="animate-spin text-primary" /></div>;

  const { files = [], tags = [], authors = [] } = data;

  // Calculate rating
  const totalReviews = (data.usefulCount || 0) + (data.notUsefulCount || 0);
  const ratingPercent = totalReviews > 0 ? Math.round((data.usefulCount || 0) / totalReviews * 100) : null;

  const initials = (data.createdByName || "?").split(" ").map(s => s[0]).join("").slice(0, 2).toUpperCase();

  /* ======================= UI ======================= */

  return (
    <div className="flex flex-col gap-4">

      {/* Back button */}
      <Button variant="ghost" size="sm" className="w-fit" onClick={() => router.back()}>
        <ArrowLeft className="h-4 w-4 mr-1" /> Quay lại
      </Button>

      {/* Hero Header - 2 Column Layout */}
      <div className="relative rounded-xl bg-linear-to-br from-amber-50 to-orange-100 dark:from-amber-950/30 dark:to-orange-900/20 p-4 md:p-5">
        {/* Edit button - top right corner */}
        {isOwner && (
          <Button variant="outline" size="sm" onClick={() => setShowEditModal(true)} className="absolute top-3 right-3">
            <Edit className="h-4 w-4 mr-1" /> Sửa
          </Button>
        )}
        <div className="flex flex-col md:flex-row gap-4 md:gap-6">
          {/* Left: Thumbnail + Info */}
          <div className="flex items-stretch gap-3 md:gap-4 flex-1">
            {/* Thumbnail */}
            <div className="shrink-0 w-16 md:w-20 overflow-hidden flex items-center justify-center bg-muted">
              {data.coverFileId ? (
                // eslint-disable-next-line @next/next/no-img-element
                <img
                  src={getFileUrlById(data.coverFileId)}
                  alt={data.documentName || ""}
                  className="h-full w-full object-cover"
                />
              ) : (
                <BookOpen className="h-6 w-6 md:h-7 md:w-7 text-white" />
              )}
            </div>

            {/* Info */}
            <div className="flex-1 min-w-0">
              <div className="flex items-start justify-between gap-2">
                <h1 className="text-lg md:text-xl font-bold text-foreground leading-tight">
                  {data.documentName}
                </h1>
              </div>

              {/* Author & metadata + Stats on same line */}
              <div className="flex flex-wrap items-center gap-2 mt-2 text-sm text-muted-foreground">
                {data.createdById && (
                  <Link href={`/profile/${data.createdById}`} className="flex items-center gap-1.5 hover:text-foreground transition-colors">
                    <span>by</span>
                    <Avatar className="h-5 w-5">
                      <AvatarImage src={data.createdByAvatarUrl || undefined} />
                      <AvatarFallback className="text-[10px]">{initials}</AvatarFallback>
                    </Avatar>
                    <span className="font-medium text-foreground">{data.createdByName}</span>
                  </Link>
                )}
                <span>•</span>
                <span className="flex items-center gap-1">
                  <FileText className="h-3.5 w-3.5" />
                  {files.length} chương
                </span>
                {data.subject?.subjectName && (
                  <>
                    <span>•</span>
                    <span>{data.subject.subjectName}</span>
                  </>
                )}
                {ratingPercent !== null && (
                  <>
                    <span>•</span>
                    <span className="flex items-center gap-1 text-emerald-600 dark:text-emerald-400 font-medium">
                      <ThumbsUp className="h-3.5 w-3.5" />
                      {ratingPercent}% ({totalReviews})
                    </span>
                  </>
                )}
                <span>•</span>
                <span className="flex items-center gap-1">
                  <Eye className="h-3.5 w-3.5" />
                  {(data.totalViewCount || 0).toLocaleString()}
                </span>
              </div>

              {/* Tags */}
              {tags.length > 0 && (
                <div className="flex flex-wrap gap-1.5 mt-2">
                  {tags.map(t => (
                    <Badge key={t.id} variant="secondary" className="text-xs">
                      {t.tagName}
                    </Badge>
                  ))}
                </div>
              )}
            </div>
          </div>

          {/* Right: Description + Edit button */}
          <div className="shrink-0 md:w-72 md:border-l md:pl-5 md:border-border/50 flex flex-col gap-3">
            {data.description && (
              <div>
                <p className="text-xs font-medium text-muted-foreground mb-1">Mô tả</p>
                <p className="text-sm text-muted-foreground leading-relaxed line-clamp-4">
                  {data.description}
                </p>
              </div>
            )}
          </div>
        </div>
      </div>

      {/* Files Section */}
      <div className="border rounded-xl bg-card overflow-hidden">
        <div className="border-b px-4 py-3 flex items-center justify-between bg-muted/30">
          <div className="flex items-center gap-2 font-semibold">
            <FileText className="h-4 w-4" />
            Danh sách chương ({files.length})
          </div>
          {isOwner && (
            <Button
              variant={showUploadForm ? "secondary" : "outline"}
              size="sm"
              onClick={() => setShowUploadForm(v => !v)}
            >
              {showUploadForm ? (
                <><ChevronUp className="h-4 w-4 mr-1" /> Ẩn</>
              ) : (
                <><Plus className="h-4 w-4 mr-1" /> Thêm chương</>
              )}
            </Button>
          )}
        </div>

        {/* Collapsible Upload Form */}
        {isOwner && showUploadForm && (
          <div className="px-4 py-4 border-b bg-muted/50">
            <DocumentFileUpload documentId={documentId} onUploadSuccess={handleUploadSuccess} />
          </div>
        )}

        {/* File List */}
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
