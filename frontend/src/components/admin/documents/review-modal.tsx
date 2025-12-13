"use client";

import { useState } from "react";
import {
    Dialog,
    DialogContent,
    DialogHeader,
    DialogTitle,
    DialogFooter,
} from "@/src/components/ui/dialog";
import { Button } from "@/src/components/ui/button";
import { Label } from "@/src/components/ui/label";
import { CheckCircle, XCircle, Loader2 } from "lucide-react";
import { useTranslations } from "next-intl";

// Local type matching backend ReviewDocumentFileRequest
export interface ReviewDocumentFileCommand {
    documentFileId?: string;
    status: number; // ContentStatus enum: 0=PendingReview, 1=Approved, 2=Hidden
    reviewNote?: string | null;
}

interface ReviewModalProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    documentFileId: string | null;
    documentFileName: string;
    onSubmit: (command: ReviewDocumentFileCommand) => void | Promise<void>;
    loading?: boolean;
}

export function ReviewModal({
    open,
    onOpenChange,
    documentFileId,
    documentFileName,
    onSubmit,
    loading,
}: ReviewModalProps) {
    const t = useTranslations("admin.documents");
    const [reviewNote, setReviewNote] = useState("");
    const [selectedStatus, setSelectedStatus] = useState<number | null>(null);

    const handleSubmit = async (status: number) => {
        if (!documentFileId) return;
        setSelectedStatus(status);
        await onSubmit({
            documentFileId,
            status: status,
            reviewNote: reviewNote || null,
        });
        setReviewNote("");
        setSelectedStatus(null);
    };

    const handleClose = () => {
        setReviewNote("");
        setSelectedStatus(null);
        onOpenChange(false);
    };

    return (
        <Dialog open={open} onOpenChange={handleClose}>
            <DialogContent className="max-w-md">
                <DialogHeader>
                    <DialogTitle>{t("reviewModal.title")}</DialogTitle>
                </DialogHeader>

                <div className="space-y-4 py-4">
                    <div className="text-sm text-slate-600 dark:text-slate-400">
                        {t("reviewModal.description")}
                    </div>
                    <div className="p-3 bg-slate-50 dark:bg-slate-800 ">
                        <span className="font-medium">{documentFileName}</span>
                    </div>

                    <div>
                        <Label htmlFor="reviewNote">{t("reviewModal.note")}</Label>
                        <textarea
                            id="reviewNote"
                            value={reviewNote}
                            onChange={(e) => setReviewNote(e.target.value)}
                            disabled={loading}
                            rows={3}
                            className="mt-1 flex w-full  border border-input bg-background text-foreground px-3 py-2 text-sm shadow-sm transition-colors placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-50 resize-none"
                            placeholder={t("reviewModal.notePlaceholder")}
                        />
                    </div>
                </div>

                <DialogFooter className="flex gap-2 sm:gap-2">
                    <Button
                        variant="outline"
                        onClick={handleClose}
                        disabled={loading}
                    >
                        {t("reviewModal.cancel")}
                    </Button>
                    <Button
                        variant="destructive"
                        onClick={() => handleSubmit(2)} // Hidden = 2
                        disabled={loading}
                    >
                        {loading && selectedStatus === 2 ? (
                            <Loader2 size={16} className="mr-1 animate-spin" />
                        ) : (
                            <XCircle size={16} className="mr-1" />
                        )}
                        {t("reviewModal.reject")}
                    </Button>
                    <Button
                        onClick={() => handleSubmit(1)} // Approved = 1
                        disabled={loading}
                        className="bg-green-600 hover:bg-green-700"
                    >
                        {loading && selectedStatus === 1 ? (
                            <Loader2 size={16} className="mr-1 animate-spin" />
                        ) : (
                            <CheckCircle size={16} className="mr-1" />
                        )}
                        {t("reviewModal.approve")}
                    </Button>
                </DialogFooter>
            </DialogContent>
        </Dialog>
    );
}

