"use client";

import { useState, Fragment } from "react";
import { Badge } from "@/src/components/ui/badge";
import { Button } from "@/src/components/ui/button";
import {
    Table,
    TableBody,
    TableCell,
    TableHead,
    TableHeader,
    TableRow,
} from "@/src/components/ui/table";
import { Eye, FileText, MessageCircle, ChevronDown, ChevronRight } from "lucide-react";
import { useTranslations } from "next-intl";
import type { GroupedReport } from "@/src/hooks/use-reports";

interface ReportTableProps {
    groupedReports: GroupedReport[];
    loading?: boolean;
    onViewDetail: (grouped: GroupedReport) => void;
    onSort?: (key: string, direction: "asc" | "desc" | null) => void;
    sortKey?: string | null;
    sortDirection?: "asc" | "desc" | null;
}

const statusLabels: Record<number, { label: string; variant: "default" | "outline" | "secondary" | "destructive" }> = {
    0: { label: "Chờ xử lý", variant: "destructive" },
    1: { label: "Đã duyệt", variant: "default" },
    2: { label: "Từ chối", variant: "secondary" },
};

export function ReportTable({
    groupedReports,
    loading,
    onViewDetail,
}: ReportTableProps) {
    const t = useTranslations("admin.reports");
    const [expandedRows, setExpandedRows] = useState<Set<string>>(new Set());

    const toggleExpand = (key: string) => {
        const newExpanded = new Set(expandedRows);
        if (newExpanded.has(key)) {
            newExpanded.delete(key);
        } else {
            newExpanded.add(key);
        }
        setExpandedRows(newExpanded);
    };

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

    if (loading) {
        return (
            <div className="flex items-center justify-center py-12">
                <span className="text-sm text-slate-500">{t("table.loading")}</span>
            </div>
        );
    }

    if (groupedReports.length === 0) {
        return (
            <div className="flex items-center justify-center py-12">
                <span className="text-sm text-slate-500">{t("table.noData")}</span>
            </div>
        );
    }

    return (
        <div className="border rounded overflow-x-auto">
            <Table>
                <TableHeader>
                    <TableRow className="bg-slate-50 dark:bg-slate-800">
                        <TableHead className="w-10"></TableHead>
                        <TableHead className="min-w-[120px]">{t("table.target")}</TableHead>
                        <TableHead className="min-w-[80px]">{t("table.reportCount")}</TableHead>
                        <TableHead className="min-w-[200px]">{t("table.latestContent")}</TableHead>
                        <TableHead className="min-w-[120px]">{t("table.reporter")}</TableHead>
                        <TableHead className="min-w-[100px]">{t("table.status")}</TableHead>
                        <TableHead className="min-w-[140px]">{t("table.createdAt")}</TableHead>
                        <TableHead className="text-right min-w-[100px]">{t("table.actions")}</TableHead>
                    </TableRow>
                </TableHeader>
                <TableBody>
                    {groupedReports.map((grouped) => {
                        const isExpanded = expandedRows.has(grouped.key);
                        const statusInfo = statusLabels[grouped.status] || statusLabels[0];

                        return (
                            <Fragment key={grouped.key}>
                                <TableRow key={grouped.key} className="hover:bg-slate-50 dark:hover:bg-slate-800/50">
                                    <TableCell>
                                        {grouped.reportCount > 1 && (
                                            <Button
                                                variant="ghost"
                                                size="sm"
                                                onClick={() => toggleExpand(grouped.key)}
                                                className="h-6 w-6 p-0"
                                            >
                                                {isExpanded ? <ChevronDown size={14} /> : <ChevronRight size={14} />}
                                            </Button>
                                        )}
                                    </TableCell>
                                    <TableCell>
                                        <div className="flex items-center gap-2">
                                            {grouped.type === "documentFile" ? (
                                                <FileText size={16} className="text-blue-500" />
                                            ) : (
                                                <MessageCircle size={16} className="text-green-500" />
                                            )}
                                            <span className="text-sm">
                                                {grouped.type === "documentFile" ? t("type.documentFile") : t("type.comment")}
                                            </span>
                                        </div>
                                    </TableCell>
                                    <TableCell>
                                        <Badge variant={grouped.reportCount > 2 ? "destructive" : "outline"}>
                                            {grouped.reportCount}
                                        </Badge>
                                    </TableCell>
                                    <TableCell>
                                        <span className="text-sm line-clamp-2 max-w-xs">
                                            {grouped.latestContent || "-"}
                                        </span>
                                    </TableCell>
                                    <TableCell>
                                        <span className="text-sm">
                                            {grouped.latestReporterName}
                                        </span>
                                    </TableCell>
                                    <TableCell>
                                        <Badge variant={statusInfo.variant}>
                                            {statusInfo.label}
                                        </Badge>
                                    </TableCell>
                                    <TableCell>
                                        <span className="text-sm text-slate-500">
                                            {formatDate(grouped.latestCreatedAt)}
                                        </span>
                                    </TableCell>
                                    <TableCell className="text-right">
                                        <Button
                                            variant="ghost"
                                            size="sm"
                                            onClick={() => onViewDetail(grouped)}
                                        >
                                            <Eye size={16} className="mr-1" />
                                            {t("table.view")}
                                        </Button>
                                    </TableCell>
                                </TableRow>
                                {/* Expanded rows showing all reports */}
                                {isExpanded && grouped.reports.slice(1).map((report) => (
                                    <TableRow key={report.id} className="bg-slate-50/50 dark:bg-slate-800/30">
                                        <TableCell></TableCell>
                                        <TableCell></TableCell>
                                        <TableCell></TableCell>
                                        <TableCell>
                                            <span className="text-sm line-clamp-2 max-w-xs text-slate-600 dark:text-slate-400">
                                                {report.content || "-"}
                                            </span>
                                        </TableCell>
                                        <TableCell>
                                            <span className="text-sm text-slate-600 dark:text-slate-400">
                                                {report.reporterName}
                                            </span>
                                        </TableCell>
                                        <TableCell></TableCell>
                                        <TableCell>
                                            <span className="text-sm text-slate-400">
                                                {formatDate(report.createdAt)}
                                            </span>
                                        </TableCell>
                                        <TableCell></TableCell>
                                    </TableRow>
                                ))}
                            </Fragment>
                        );
                    })}
                </TableBody>
            </Table>
        </div>
    );
}

