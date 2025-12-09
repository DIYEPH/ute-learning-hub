"use client";

import { useState, useEffect } from "react";
import {
    Dialog,
    DialogContent,
    DialogHeader,
    DialogTitle,
} from "@/src/components/ui/dialog";
import { Badge } from "@/src/components/ui/badge";
import { FileText, Download, Calendar } from "lucide-react";
import { useTranslations } from "next-intl";
import { useDocuments } from "@/src/hooks/use-documents";
import type { DocumentDetailDto, DocumentFileDto } from "@/src/api/database/types.gen";

interface DocumentDetailModalProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    documentId: string | null;
}

export function DocumentDetailModal({
    open,
    onOpenChange,
    documentId,
}: DocumentDetailModalProps) {
    const t = useTranslations("admin.documents");
    const { fetchDocumentById } = useDocuments();
    const [document, setDocument] = useState<DocumentDetailDto | null>(null);
    const [loading, setLoading] = useState(false);

    useEffect(() => {
        if (open && documentId) {
            setLoading(true);
            fetchDocumentById(documentId)
                .then((doc) => setDocument(doc))
                .finally(() => setLoading(false));
        } else {
            setDocument(null);
        }
    }, [open, documentId, fetchDocumentById]);

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
                            <h3 className="font-semibold text-lg">{document.documentName}</h3>
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
                                {document.isDownload && (
                                    <span className="flex items-center gap-1">
                                        <Download size={14} />
                                        {t("detailModal.downloadable")}
                                    </span>
                                )}
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
                                {document.files?.map((file: DocumentFileDto) => (
                                    <div
                                        key={file.id}
                                        className="flex items-center gap-3 p-3 bg-slate-50 dark:bg-slate-800 rounded-lg"
                                    >
                                        <FileText size={20} className="text-slate-400" />
                                        <div className="flex-1 min-w-0">
                                            <div className="font-medium text-sm truncate">
                                                {file.title || `File ${file.order || 1}`}
                                            </div>
                                            <div className="text-xs text-slate-500">
                                                {formatFileSize(file.fileSize)} • {file.mimeType || "-"}
                                                {file.totalPages && ` • ${file.totalPages} ${t("detailModal.pages")}`}
                                            </div>
                                        </div>
                                        {file.isPrimary && (
                                            <Badge variant="default" className="text-xs">
                                                {t("detailModal.primary")}
                                            </Badge>
                                        )}
                                    </div>
                                ))}
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
