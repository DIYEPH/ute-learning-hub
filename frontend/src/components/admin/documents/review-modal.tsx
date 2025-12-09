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
import type { ReviewDocumentCommand } from "@/src/api/database/types.gen";

interface ReviewModalProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    documentId: string | null;
    documentName: string;
    onSubmit: (command: ReviewDocumentCommand) => void | Promise<void>;
    loading?: boolean;
}

export function ReviewModal({
    open,
    onOpenChange,
    documentId,
    documentName,
    onSubmit,
    loading,
}: ReviewModalProps) {
    const t = useTranslations("admin.documents");
    const [reviewNote, setReviewNote] = useState("");
    const [selectedStatus, setSelectedStatus] = useState<number | null>(null);

    const handleSubmit = async (status: number) => {
        if (!documentId) return;
        setSelectedStatus(status);
        await onSubmit({
            documentId,
            reviewStatus: status,
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
                    <div className="p-3 bg-slate-50 dark:bg-slate-800 rounded-lg">
                        <span className="font-medium">{documentName}</span>
                    </div>

                    <div>
                        <Label htmlFor="reviewNote">{t("reviewModal.note")}</Label>
                        <textarea
                            id="reviewNote"
                            value={reviewNote}
                            onChange={(e) => setReviewNote(e.target.value)}
                            disabled={loading}
                            rows={3}
                            className="mt-1 flex w-full rounded-md border border-input bg-background text-foreground px-3 py-2 text-sm shadow-sm transition-colors placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-50 resize-none"
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
                        onClick={() => handleSubmit(3)} // Rejected = 3
                        disabled={loading}
                    >
                        {loading && selectedStatus === 3 ? (
                            <Loader2 size={16} className="mr-1 animate-spin" />
                        ) : (
                            <XCircle size={16} className="mr-1" />
                        )}
                        {t("reviewModal.reject")}
                    </Button>
                    <Button
                        onClick={() => handleSubmit(2)} // Approved = 2
                        disabled={loading}
                        className="bg-green-600 hover:bg-green-700"
                    >
                        {loading && selectedStatus === 2 ? (
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
