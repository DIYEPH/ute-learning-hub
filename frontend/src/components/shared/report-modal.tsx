"use client";

import { useState } from "react";
import { Flag, Loader2 } from "lucide-react";
import {
    Dialog,
    DialogContent,
    DialogHeader,
    DialogTitle,
    DialogFooter,
} from "@/src/components/ui/dialog";
import { Button } from "@/src/components/ui/button";
import { Textarea } from "@/src/components/ui/textarea";
import { Label } from "@/src/components/ui/label";
import { postApiReport } from "@/src/api/database/sdk.gen";
import { useNotification } from "@/src/components/providers/notification-provider";

export type ReportTargetType = "documentFile" | "comment" | "document" | "user" | "conversation";

interface ReportModalProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    targetType: ReportTargetType;
    targetId: string;
    targetTitle?: string; // Display name of what's being reported
}

export function ReportModal({
    open,
    onOpenChange,
    targetType,
    targetId,
    targetTitle,
}: ReportModalProps) {
    const [content, setContent] = useState("");
    const [submitting, setSubmitting] = useState(false);
    const { success, error } = useNotification();

    const getTargetLabel = () => {
        switch (targetType) {
            case "documentFile":
                return "tài liệu";
            case "comment":
                return "bình luận";
            case "document":
                return "tài liệu";
            case "user":
                return "người dùng";
            case "conversation":
                return "cuộc trò chuyện";
            default:
                return "nội dung";
        }
    };

    const handleSubmit = async () => {
        if (!content.trim()) {
            error("Vui lòng nhập nội dung báo cáo");
            return;
        }

        setSubmitting(true);
        try {
            // Map targetType to API body fields
            const body: Record<string, unknown> = {
                content: content.trim(),
            };

            if (targetType === "documentFile") {
                body.documentFileId = targetId;
            } else if (targetType === "comment") {
                body.commentId = targetId;
            }
            // Add more target types as needed when API supports them

            await postApiReport({
                body: body as { documentFileId?: string | null; commentId?: string | null; content?: string },
                throwOnError: true,
            });

            success("Đã gửi báo cáo thành công. Chúng tôi sẽ xem xét trong thời gian sớm nhất.");
            setContent("");
            onOpenChange(false);
        } catch (err: any) {
            const errorMessage =
                err?.response?.data?.message ||
                err?.message ||
                "Không thể gửi báo cáo";
            error(errorMessage);
        } finally {
            setSubmitting(false);
        }
    };

    const handleClose = () => {
        if (!submitting) {
            setContent("");
            onOpenChange(false);
        }
    };

    return (
        <Dialog open={open} onOpenChange={handleClose}>
            <DialogContent className="sm:max-w-md">
                <DialogHeader>
                    <DialogTitle className="flex items-center gap-2">
                        <Flag className="h-5 w-5 text-red-500" />
                        Báo cáo vi phạm
                    </DialogTitle>
                </DialogHeader>

                <div className="space-y-3">
                    {targetTitle && (
                        <p className="text-sm text-muted-foreground">
                            Báo cáo {getTargetLabel()}:{" "}
                            <span className="font-medium text-foreground">{targetTitle}</span>
                        </p>
                    )}

                    <div>
                        <Label htmlFor="report-content">Lý do báo cáo</Label>
                        <Textarea
                            id="report-content"
                            value={content}
                            onChange={(e) => setContent(e.target.value)}
                            placeholder={`Mô tả lý do bạn muốn báo cáo ${getTargetLabel()} này...`}
                            rows={4}
                            className="mt-1.5"
                            disabled={submitting}
                        />
                        <p className="text-xs text-muted-foreground mt-1">
                            Vui lòng cung cấp thông tin chi tiết để chúng tôi có thể xử lý báo cáo nhanh chóng.
                        </p>
                    </div>
                </div>

                <DialogFooter className="gap-2 sm:gap-0">
                    <Button
                        variant="outline"
                        onClick={handleClose}
                        disabled={submitting}
                    >
                        Hủy
                    </Button>
                    <Button
                        onClick={handleSubmit}
                        disabled={!content.trim() || submitting}
                        variant="destructive"
                    >
                        {submitting ? (
                            <>
                                <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                                Đang gửi...
                            </>
                        ) : (
                            "Gửi báo cáo"
                        )}
                    </Button>
                </DialogFooter>
            </DialogContent>
        </Dialog>
    );
}

