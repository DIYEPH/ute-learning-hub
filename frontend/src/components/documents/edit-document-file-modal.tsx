"use client";

import { useState, useEffect, useRef } from "react";
import { Loader2, Upload, X, Image as ImageIcon } from "lucide-react";
import { putApiDocumentByDocumentIdFilesByFileId, postApiFile } from "@/src/api/database/sdk.gen";
import type { DocumentFileDto, FileDto } from "@/src/api/database/types.gen";
import {
    Dialog,
    DialogContent,
    DialogHeader,
    DialogTitle,
} from "@/src/components/ui/dialog";
import { Button } from "@/src/components/ui/button";
import { Input } from "@/src/components/ui/input";
import { Label } from "@/src/components/ui/label";
import { useNotification } from "@/src/components/providers/notification-provider";
import { getFileUrlById } from "@/src/lib/file-url";

interface EditDocumentFileModalProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    documentId: string;
    file: DocumentFileDto;
    onSuccess?: () => void;
}

export function EditDocumentFileModal({
    open,
    onOpenChange,
    documentId,
    file,
    onSuccess,
}: EditDocumentFileModalProps) {
    const [title, setTitle] = useState(file.title || "");
    const [coverFileId, setCoverFileId] = useState<string | null>(file.coverFileId ?? null);
    const [pendingCoverFile, setPendingCoverFile] = useState<File | null>(null);
    const [coverPreview, setCoverPreview] = useState<string | null>(null);
    const [saving, setSaving] = useState(false);
    const fileInputRef = useRef<HTMLInputElement>(null);
    const { success: notifySuccess, error: notifyError } = useNotification();

    // Reset form when file changes
    useEffect(() => {
        setTitle(file.title || "");
        setCoverFileId(file.coverFileId ?? null);
        setPendingCoverFile(null);
        setCoverPreview(null);
    }, [file]);

    const handleCoverSelect = (e: React.ChangeEvent<HTMLInputElement>) => {
        const selectedFile = e.target.files?.[0];
        if (!selectedFile) return;

        // Store file for upload on save
        setPendingCoverFile(selectedFile);

        // Show preview immediately
        const previewUrl = URL.createObjectURL(selectedFile);
        setCoverPreview(previewUrl);

        // Clear the existing coverFileId since we're replacing it
        setCoverFileId(null);

        if (fileInputRef.current) {
            fileInputRef.current.value = "";
        }
    };

    const handleRemoveCover = () => {
        setCoverFileId(null);
        setPendingCoverFile(null);
        setCoverPreview(null);
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        if (!file.id) return;

        setSaving(true);
        try {
            let finalCoverFileId = coverFileId;

            // Upload pending cover file if exists
            if (pendingCoverFile) {
                const response = await postApiFile({
                    body: { file: pendingCoverFile },
                    query: { category: "covers" },
                });
                const uploaded = (response.data ?? response) as FileDto | undefined;
                if (uploaded?.id) {
                    finalCoverFileId = uploaded.id;
                }
            }

            await putApiDocumentByDocumentIdFilesByFileId({
                path: { documentId, fileId: file.id },
                body: {
                    documentId,
                    documentFileId: file.id,
                    title: title.trim() || undefined,
                    order: file.order ?? undefined,
                    coverFileId: finalCoverFileId ?? undefined,
                },
            });
            notifySuccess("Đã cập nhật thông tin file");
            onSuccess?.();
            onOpenChange(false);
        } catch (err: any) {
            notifyError(err?.message || "Không thể cập nhật file");
        } finally {
            setSaving(false);
        }
    };

    // Get current cover URL
    const currentCoverUrl = coverPreview || (coverFileId ? getFileUrlById(coverFileId) : null);

    return (
        <Dialog open={open} onOpenChange={onOpenChange}>
            <DialogContent className="max-w-md">
                <DialogHeader>
                    <DialogTitle>Chỉnh sửa chương / file</DialogTitle>
                </DialogHeader>

                <form onSubmit={handleSubmit} className="space-y-4">
                    {/* Title */}
                    <div className="space-y-2">
                        <Label htmlFor="file-title">Tiêu đề</Label>
                        <Input
                            id="file-title"
                            value={title}
                            onChange={(e) => setTitle(e.target.value)}
                            placeholder="Nhập tiêu đề"
                        />
                    </div>

                    {/* Cover Image */}
                    <div className="space-y-2">
                        <Label>Ảnh bìa</Label>
                        <div className="flex gap-3">
                            {/* Preview */}
                            <div className="relative w-24 h-16 bg-slate-100 dark:bg-slate-800 rounded overflow-hidden flex-shrink-0">
                                {currentCoverUrl ? (
                                    <>
                                        <img
                                            src={currentCoverUrl}
                                            alt="Cover"
                                            className="w-full h-full object-contain"
                                        />
                                        <button
                                            type="button"
                                            onClick={handleRemoveCover}
                                            className="absolute top-1 right-1 bg-red-500 text-white rounded-full p-0.5 hover:bg-red-600 transition-colors"
                                        >
                                            <X className="h-3 w-3" />
                                        </button>
                                    </>
                                ) : (
                                    <div className="w-full h-full flex items-center justify-center">
                                        <ImageIcon className="h-6 w-6 text-slate-400" />
                                    </div>
                                )}
                            </div>

                            {/* Upload button */}
                            <div className="flex-1">
                                <input
                                    ref={fileInputRef}
                                    type="file"
                                    accept="image/*"
                                    onChange={handleCoverSelect}
                                    className="hidden"
                                    id="cover-upload"
                                />
                                <label
                                    htmlFor="cover-upload"
                                    className="flex items-center justify-center gap-2 px-3 py-2 border border-dashed border-slate-300 dark:border-slate-600 rounded cursor-pointer hover:bg-slate-50 dark:hover:bg-slate-800 transition-colors text-sm text-slate-600 dark:text-slate-400"
                                >
                                    <Upload className="h-4 w-4" />
                                    Chọn ảnh bìa
                                </label>
                                <p className="text-xs text-slate-500 mt-1">PNG, JPG tối đa 5MB</p>
                            </div>
                        </div>
                    </div>

                    <div className="flex justify-end gap-2 pt-2">
                        <Button
                            type="button"
                            variant="outline"
                            onClick={() => onOpenChange(false)}
                            disabled={saving}
                        >
                            Hủy
                        </Button>
                        <Button type="submit" disabled={saving}>
                            {saving ? (
                                <>
                                    <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                                    Đang lưu...
                                </>
                            ) : (
                                "Lưu thay đổi"
                            )}
                        </Button>
                    </div>
                </form>
            </DialogContent>
        </Dialog>
    );
}
