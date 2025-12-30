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
import {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
} from "@/src/components/ui/select";
import { postApiReport } from "@/src/api";
import { useNotification } from "@/src/components/providers/notification-provider";

export type ReportTargetType = "documentFile" | "comment" | "document" | "user" | "conversation";

// Map ReportReason enum values to Vietnamese labels
const REPORT_REASONS = [
    { value: 0, label: "Khác" },
    { value: 1, label: "Vi phạm bản quyền" },
    { value: 2, label: "Nội dung xúc phạm, lăng mạ" },
    { value: 3, label: "Spam, quảng cáo" },
    { value: 4, label: "Thông tin sai lệch" },
    { value: 5, label: "Nội dung bạo lực" },
    { value: 6, label: "Nội dung không phù hợp" },
    { value: 7, label: "Quấy rối, bắt nạt" },
    { value: 8, label: "Đạo văn, sao chép" },
] as const;

interface ReportModalProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    targetType: ReportTargetType;
    targetId: string;
    targetTitle?: string; 
}

export function ReportModal({
    open,
    onOpenChange,
    targetType,
    targetId,
    targetTitle,
}: ReportModalProps) {
    const [reason, setReason] = useState<number | null>(null);
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
        if (reason === null) {
            error("Vui lòng chọn lý do báo cáo");
            return;
        }

        setSubmitting(true);
        try {
            // Map targetType to API body fields
            const body: Record<string, unknown> = {
                reason: reason,
                content: content.trim() || REPORT_REASONS.find(r => r.value === reason)?.label || "",
            };

            if (targetType === "documentFile") {
                body.documentFileId = targetId;
            } else if (targetType === "comment") {
                body.commentId = targetId;
            }
            // Add more target types as needed when API supports them

            await postApiReport({
                body: body as { documentFileId?: string | null; commentId?: string | null; reason?: number; content?: string },
                throwOnError: true,
            });

            success("Đã gửi báo cáo thành công. Chúng tôi sẽ xem xét trong thời gian sớm nhất.");
            setReason(null);
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
            setReason(null);
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

                <div className="space-y-4">
                    {targetTitle && (
                        <p className="text-sm text-muted-foreground">
                            Báo cáo {getTargetLabel()}:{" "}
                            <span className="font-medium text-foreground">{targetTitle}</span>
                        </p>
                    )}

                    <div>
                        <Label htmlFor="report-reason">Lý do báo cáo <span className="text-red-500">*</span></Label>
                        <Select
                            value={reason !== null ? String(reason) : undefined}
                            onValueChange={(value: string) => setReason(Number(value))}
                            disabled={submitting}
                        >
                            <SelectTrigger id="report-reason" className="mt-1.5">
                                <SelectValue placeholder="Chọn lý do báo cáo..." />
                            </SelectTrigger>
                            <SelectContent>
                                {REPORT_REASONS.map((item) => (
                                    <SelectItem key={item.value} value={String(item.value)}>
                                        {item.label}
                                    </SelectItem>
                                ))}
                            </SelectContent>
                        </Select>
                    </div>

                    <div>
                        <Label htmlFor="report-content">Mô tả chi tiết (không bắt buộc)</Label>
                        <Textarea
                            id="report-content"
                            value={content}
                            onChange={(e) => setContent(e.target.value)}
                            placeholder={`Mô tả thêm chi tiết về vi phạm...`}
                            rows={3}
                            className="mt-1.5"
                            disabled={submitting}
                        />
                        <p className="text-xs text-muted-foreground mt-1">
                            Cung cấp thêm thông tin để chúng tôi xử lý nhanh hơn.
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
                        disabled={reason === null || submitting}
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

