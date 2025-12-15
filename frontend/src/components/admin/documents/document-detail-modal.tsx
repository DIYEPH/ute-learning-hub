"use client";

import { useState, useEffect } from "react";
import {
    Dialog,
    DialogContent,
    DialogHeader,
    DialogTitle,
} from "@/src/components/ui/dialog";
import { Badge } from "@/src/components/ui/badge";
import { Button } from "@/src/components/ui/button";
import { FileText, Calendar, Check, X, ExternalLink, Loader2 } from "lucide-react";
import { useTranslations } from "next-intl";
import { useDocuments } from "@/src/hooks/use-documents";
import { useNotification } from "@/src/components/providers/notification-provider";
import type { DocumentDetailDto, DocumentFileDto } from "@/src/api/database/types.gen";

interface DocumentDetailModalProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    documentId: string | null;
    onReviewSuccess?: () => void;
}

const statusLabels: Record<number, { label: string; variant: "default" | "destructive" | "secondary" | "outline" }> = {
    0: { label: "Chờ duyệt", variant: "destructive" },
    1: { label: "Đã duyệt", variant: "default" },
    2: { label: "Đã ẩn", variant: "secondary" },
};

export function DocumentDetailModal({
    open,
    onOpenChange,
    documentId,
    onReviewSuccess,
}: DocumentDetailModalProps) {
    const t = useTranslations("admin.documents");
    const { fetchDocumentById, reviewDocumentFile } = useDocuments();
    const notification = useNotification();
    const [document, setDocument] = useState<DocumentDetailDto | null>(null);
    const [loading, setLoading] = useState(false);
    const [reviewingFileId, setReviewingFileId] = useState<string | null>(null);
    const [rejectingFileId, setRejectingFileId] = useState<string | null>(null);
    const [rejectReason, setRejectReason] = useState("");

    const loadDocument = async () => {
        if (!documentId) return;
        setLoading(true);
        const doc = await fetchDocumentById(documentId);
        setDocument(doc);
        setLoading(false);
    };

    useEffect(() => {
        if (open && documentId) {
            loadDocument();
        } else {
            setDocument(null);
        }
    }, [open, documentId]);

    const handleReview = async (fileId: string, status: number, note?: string) => {
        setReviewingFileId(fileId);
        try {
            const success = await reviewDocumentFile({
                fileId,
                documentFileId: fileId,
                status,
                reviewNote: note || (status === 1 ? "Đã duyệt" : "Nội dung không phù hợp"),
            });
            if (success) {
                notification.success(status === 1 ? "Đã duyệt file thành công!" : "Đã từ chối file!");
                setRejectingFileId(null);
                setRejectReason("");
                await loadDocument(); // Refresh
                onReviewSuccess?.();
            } else {
                notification.error("Không thể xử lý file");
            }
        } catch {
            notification.error("Đã xảy ra lỗi");
        } finally {
            setReviewingFileId(null);
        }
    };

    const handleStartReject = (fileId: string) => {
        setRejectingFileId(fileId);
        setRejectReason("");
    };

    const handleCancelReject = () => {
        setRejectingFileId(null);
        setRejectReason("");
    };

    const formatDate = (date?: string | null) => {
        if (!date) return "-";
        return new Date(date).toLocaleString("vi-VN", {
            day: "2-digit",
            month: "2-digit",
            year: "numeric",
            hour: "2-digit",
            minute: "2-digit",
        });
    };

    const formatFileSize = (bytes?: number) => {
        if (!bytes) return "-";
        if (bytes < 1024) return `${bytes} B`;
        if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
        return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
    };

    return (
        <Dialog open={open} onOpenChange={onOpenChange}>
            <DialogContent className="max-w-2xl max-h-[80vh] overflow-y-auto">
                <DialogHeader>
                    <DialogTitle>{t("detailModal.title")}</DialogTitle>
                </DialogHeader>

                {loading ? (
                    <div className="flex items-center justify-center py-8">
                        <span className="text-sm text-slate-500">{t("table.loading")}</span>
                    </div>
                ) : document ? (
                    <div className="space-y-6">
                        {/* Document Info */}
                        <div className="space-y-3">
                            <div className="flex items-start justify-between">
                                <h3 className="font-semibold text-lg">{document.documentName}</h3>
                                <a
                                    href={`/documents/${document.id}`}
                                    target="_blank"
                                    rel="noopener noreferrer"
                                    className="text-sm text-blue-600 hover:text-blue-800 hover:underline flex items-center gap-1"
                                >
                                    <ExternalLink size={14} />
                                    Xem tài liệu
                                </a>
                            </div>
                            {document.description && (
                                <p className="text-sm text-slate-600 dark:text-slate-400">
                                    {document.description}
                                </p>
                            )}
                            <div className="flex flex-wrap gap-2">
                                {document.subject && (
                                    <Badge variant="secondary">
                                        {document.subject.subjectName}
                                    </Badge>
                                )}
                                {document.type && (
                                    <Badge variant="outline">{document.type.typeName}</Badge>
                                )}
                                {document.tags?.map((tag) => (
                                    <Badge key={tag.id} variant="outline" className="text-xs">
                                        {tag.tagName}
                                    </Badge>
                                ))}
                            </div>
                            <div className="flex items-center gap-4 text-sm text-slate-500">
                                <span className="flex items-center gap-1">
                                    <Calendar size={14} />
                                    {formatDate(document.createdAt)}
                                </span>
                            </div>
                        </div>

                        {/* Authors */}
                        {document.authors && document.authors.length > 0 && (
                            <div>
                                <h4 className="font-medium mb-2">{t("detailModal.authors")}</h4>
                                <div className="flex flex-wrap gap-2">
                                    {document.authors.map((author) => (
                                        <Badge key={author.id} variant="secondary">
                                            {author.fullName}
                                        </Badge>
                                    ))}
                                </div>
                            </div>
                        )}

                        {/* Files */}
                        <div>
                            <h4 className="font-medium mb-2">
                                {t("detailModal.files")} ({document.files?.length || 0})
                            </h4>
                            <div className="space-y-2">
                                {document.files?.map((file: DocumentFileDto) => {
                                    const status = file.status ?? 0;
                                    const statusInfo = statusLabels[status] || statusLabels[0];
                                    const isPending = status === 0;
                                    const isReviewing = reviewingFileId === file.id;

                                    return (
                                        <div
                                            key={file.id}
                                            className="flex items-center gap-3 p-3 bg-slate-50 dark:bg-slate-800 rounded"
                                        >
                                            <FileText size={20} className="text-slate-400 flex-shrink-0" />
                                            <div className="flex-1 min-w-0">
                                                <div className="flex items-center gap-2">
                                                    <span className="font-medium text-sm truncate">
                                                        {file.title || `File ${file.order || 1}`}
                                                    </span>
                                                    <Badge variant={statusInfo.variant} className="text-xs">
                                                        {statusInfo.label}
                                                    </Badge>
                                                </div>
                                                <div className="text-xs text-slate-500">
                                                    {formatFileSize(file.fileSize)} • {file.mimeType || "-"}
                                                    {file.totalPages && ` • ${file.totalPages} ${t("detailModal.pages")}`}
                                                </div>
                                            </div>
                                            <div className="flex items-center gap-1 flex-shrink-0">
                                                {/* Link to view file */}
                                                <Button
                                                    variant="ghost"
                                                    size="sm"
                                                    className="h-7 w-7 p-0"
                                                    asChild
                                                >
                                                    <a
                                                        href={`/documents/${document.id}/files/${file.id}`}
                                                        target="_blank"
                                                        rel="noopener noreferrer"
                                                    >
                                                        <ExternalLink size={14} />
                                                    </a>
                                                </Button>
                                                {/* Review buttons for pending files */}
                                                {isPending && (
                                                    rejectingFileId === file.id ? (
                                                        <div className="flex items-center gap-1">
                                                            <input
                                                                type="text"
                                                                value={rejectReason}
                                                                onChange={(e) => setRejectReason(e.target.value)}
                                                                placeholder="Lý do từ chối..."
                                                                className="h-7 px-2 text-xs border rounded w-32"
                                                                autoFocus
                                                            />
                                                            <Button
                                                                variant="ghost"
                                                                size="sm"
                                                                className="h-7 w-7 p-0 text-red-600"
                                                                onClick={() => handleReview(file.id!, 2, rejectReason)}
                                                                disabled={isReviewing || !rejectReason.trim()}
                                                            >
                                                                {isReviewing ? <Loader2 size={14} className="animate-spin" /> : <Check size={14} />}
                                                            </Button>
                                                            <Button
                                                                variant="ghost"
                                                                size="sm"
                                                                className="h-7 w-7 p-0"
                                                                onClick={handleCancelReject}
                                                            >
                                                                <X size={14} />
                                                            </Button>
                                                        </div>
                                                    ) : (
                                                        <>
                                                            <Button
                                                                variant="ghost"
                                                                size="sm"
                                                                className="h-7 w-7 p-0 text-green-600 hover:text-green-700 hover:bg-green-50"
                                                                onClick={() => handleReview(file.id!, 1)}
                                                                disabled={isReviewing}
                                                                title="Duyệt"
                                                            >
                                                                {isReviewing ? <Loader2 size={14} className="animate-spin" /> : <Check size={14} />}
                                                            </Button>
                                                            <Button
                                                                variant="ghost"
                                                                size="sm"
                                                                className="h-7 w-7 p-0 text-red-600 hover:text-red-700 hover:bg-red-50"
                                                                onClick={() => handleStartReject(file.id!)}
                                                                disabled={isReviewing}
                                                                title="Từ chối"
                                                            >
                                                                <X size={14} />
                                                            </Button>
                                                        </>
                                                    )
                                                )}
                                            </div>
                                        </div>
                                    );
                                })}
                                {(!document.files || document.files.length === 0) && (
                                    <p className="text-sm text-slate-500 py-4 text-center">
                                        {t("detailModal.noFiles")}
                                    </p>
                                )}
                            </div>
                        </div>
                    </div>
                ) : (
                    <div className="flex items-center justify-center py-8">
                        <span className="text-sm text-slate-500">{t("table.noData")}</span>
                    </div>
                )}
            </DialogContent>
        </Dialog>
    );
}
