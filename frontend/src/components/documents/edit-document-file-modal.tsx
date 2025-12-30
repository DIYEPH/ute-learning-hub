"use client";

import { useState, useEffect } from "react";
import { Loader2 } from "lucide-react";
import { putApiDocumentByDocumentIdFilesByFileId } from "@/src/api";
import type { DocumentFileDto } from "@/src/api/database/types.gen";
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
    const [saving, setSaving] = useState(false);
    const { success: notifySuccess, error: notifyError } = useNotification();

    // Reset form when file changes
    useEffect(() => {
        setTitle(file.title || "");
    }, [file]);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        if (!file.id) return;

        setSaving(true);
        try {
            await putApiDocumentByDocumentIdFilesByFileId({
                path: { documentId, fileId: file.id },
                body: {
                    title: title.trim() || undefined,
                    order: file.order ?? undefined,
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

