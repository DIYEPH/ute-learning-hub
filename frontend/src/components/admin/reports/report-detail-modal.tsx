"use client";

import { Avatar, AvatarFallback, AvatarImage } from "@/src/components/ui/avatar";
import { Badge } from "@/src/components/ui/badge";
import { Button } from "@/src/components/ui/button";
import {
    Dialog,
    DialogContent,
    DialogHeader,
    DialogTitle,
    DialogFooter,
} from "@/src/components/ui/dialog";
import { CheckCircle, EyeOff, FileText, MessageCircle, Loader2 } from "lucide-react";
import { useTranslations } from "next-intl";
import type { GroupedReport } from "@/src/hooks/use-reports";
import { REPORT_REASON_LABELS } from "@/src/hooks/use-reports";

interface ReportDetailModalProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    grouped: GroupedReport | null;
    onApprove: (reportIds: string[]) => void;
    onHide: (reportIds: string[]) => void;
    loading?: boolean;
}

const statusLabels: Record<number, { label: string; variant: "default" | "outline" | "secondary" | "destructive" }> = {
    0: { label: "Chờ xử lý", variant: "destructive" },
    1: { label: "Đã duyệt", variant: "default" },
    2: { label: "Từ chối", variant: "secondary" },
};

export function ReportDetailModal({
    open,
    onOpenChange,
    grouped,
    onApprove,
    onHide,
    loading,
}: ReportDetailModalProps) {
    const t = useTranslations("admin.reports");

    const formatDate = (dateStr?: string) => {
        if (!dateStr) return "-";
        return new Date(dateStr).toLocaleString("vi-VN", {
            day: "2-digit",
            month: "2-digit",
            year: "numeric",
            hour: "2-digit",
            minute: "2-digit",
        });
    };

    const getInitials = (name?: string) => {
        if (!name) return "?";
        return name.split(" ").map(n => n[0]).join("").toUpperCase().slice(0, 2);
    };

    return (
        <Dialog open={open} onOpenChange={onOpenChange}>
            <DialogContent className="max-w-2xl max-h-[80vh] overflow-y-auto">
                <DialogHeader>
                    <DialogTitle className="flex items-center gap-2">
                        {grouped?.type === "documentFile" ? (
                            <FileText size={20} className="text-primary" />
                        ) : (
                            <MessageCircle size={20} className="text-green-500" />
                        )}
                        {t("detailModal.title")} ({grouped?.reportCount || 0})
                    </DialogTitle>
                </DialogHeader>

                <div className="space-y-4 py-4">
                    {/* Target info */}
                    <div className="p-3 bg-muted">
                        <div className="text-sm text-muted-foreground mb-1">
                            {grouped?.type === "documentFile" ? t("type.documentFile") : t("type.comment")}
                        </div>
                        {grouped?.targetUrl ? (
                            <a
                                href={grouped.targetUrl}
                                target="_blank"
                                rel="noopener noreferrer"
                                className="text-sm text-primary hover:text-primary/80 hover:underline"
                            >
                                {grouped.targetUrl}
                            </a>
                        ) : (
                            <div className="font-mono text-xs text-muted-foreground">
                                ID: {grouped?.targetId}
                            </div>
                        )}
                    </div>

                    {/* Reports list */}
                    <div className="space-y-3">
                        <div className="text-sm font-medium">{t("detailModal.reports")}</div>
                        {grouped?.reports.map((report) => {
                            const statusInfo = statusLabels[report.status ?? 0] || statusLabels[0];
                            return (
                                <div
                                    key={report.id}
                                    className="p-3 border  space-y-2"
                                >
                                    <div className="flex items-start justify-between">
                                        <div className="flex items-center gap-2">
                                            <Avatar className="h-8 w-8">
                                                {report.reporterAvatarUrl && (
                                                    <AvatarImage src={report.reporterAvatarUrl} />
                                                )}
                                                <AvatarFallback className="text-xs">
                                                    {getInitials(report.reporterName)}
                                                </AvatarFallback>
                                            </Avatar>
                                            <div>
                                                <div className="text-sm font-medium">
                                                    {report.reporterName}
                                                </div>
                                                <div className="text-xs text-muted-foreground">
                                                    {formatDate(report.createdAt)}
                                                </div>
                                            </div>
                                        </div>
                                        <div className="flex items-center gap-2">
                                            <Badge variant="secondary" className="text-xs">
                                                {REPORT_REASON_LABELS[report.reason ?? 0] || "Khác"}
                                            </Badge>
                                            <Badge variant={statusInfo.variant} className="text-xs">
                                                {statusInfo.label}
                                            </Badge>
                                        </div>
                                    </div>
                                    <div className="text-sm text-muted-foreground pl-10">
                                        {report.content || t("detailModal.noContent")}
                                    </div>
                                </div>
                            );
                        })}
                    </div>
                </div>

                <DialogFooter className="flex gap-2">
                    <Button
                        variant="outline"
                        onClick={() => onOpenChange(false)}
                        disabled={loading}
                    >
                        {t("detailModal.close")}
                    </Button>
                    <Button
                        variant="secondary"
                        onClick={() => {
                            const ids = grouped?.reports.map(r => r.id).filter((id): id is string => !!id) || [];
                            onHide(ids);
                        }}
                        disabled={loading || grouped?.status !== 0}
                    >
                        {loading ? (
                            <Loader2 size={16} className="mr-1 animate-spin" />
                        ) : (
                            <EyeOff size={16} className="mr-1" />
                        )}
                        {t("detailModal.rejectReport")}
                    </Button>
                    <Button
                        onClick={() => {
                            const ids = grouped?.reports.map(r => r.id).filter((id): id is string => !!id) || [];
                            onApprove(ids);
                        }}
                        disabled={loading || grouped?.status !== 0}
                        className="bg-green-600 hover:bg-green-700"
                    >
                        {loading ? (
                            <Loader2 size={16} className="mr-1 animate-spin" />
                        ) : (
                            <CheckCircle size={16} className="mr-1" />
                        )}
                        {t("detailModal.approveReport")}
                    </Button>
                </DialogFooter>
            </DialogContent>
        </Dialog>
    );
}

