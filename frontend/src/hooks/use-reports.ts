"use client";

import { useState, useCallback } from "react";
import {
    getApiReport,
    postApiReportByIdReview,
} from "@/src/api/database/sdk.gen";
import type {
    ReportDto,
    GetApiReportData,
    ReviewReportCommand,
} from "@/src/api/database/types.gen";
import { useCrud, type PagedResponse } from "./use-crud";

// Group reports by documentFileId or commentId
export interface GroupedReport {
    key: string; // documentFileId or commentId
    type: "documentFile" | "comment";
    targetId: string;
    reports: ReportDto[];
    reportCount: number;
    latestContent: string;
    latestReporterName: string;
    latestCreatedAt: string;
    status: number; // status of the first report
}

export function useReports() {
    const [pendingCount, setPendingCount] = useState(0);

    const crud = useCrud<ReportDto, never, ReviewReportCommand, GetApiReportData["query"]>({
        fetchAll: async (params) => {
            const response = await getApiReport({ query: params });
            return (response as unknown as { data: PagedResponse<ReportDto> })?.data || response as PagedResponse<ReportDto>;
        },
        create: async () => {
            // Reports are created by users, not admin
            throw new Error("Not supported");
        },
        update: async (id, command) => {
            // Review report instead of update
            await postApiReportByIdReview({
                path: { id },
                body: command,
            });
            return null;
        },
        delete: async () => {
            // Reports cannot be deleted directly
            throw new Error("Not supported");
        },
        errorMessages: {
            fetch: "Không thể tải danh sách báo cáo",
            update: "Không thể xử lý báo cáo",
        },
    });

    const fetchPendingCount = useCallback(async () => {
        try {
            const response = await getApiReport({ query: { Status: 0 as any, PageSize: 1 } });
            const data = (response as unknown as { data: PagedResponse<ReportDto> })?.data || response as PagedResponse<ReportDto>;
            setPendingCount(data.totalCount || 0);
            return data.totalCount || 0;
        } catch {
            return 0;
        }
    }, []);

    const reviewReport = useCallback(
        async (reportId: string, command: ReviewReportCommand): Promise<boolean> => {
            try {
                await postApiReportByIdReview({
                    path: { id: reportId },
                    body: command,
                });
                return true;
            } catch {
                return false;
            }
        },
        []
    );

    // Group reports by targetId (documentFileId or commentId)
    const groupReports = useCallback((reports: ReportDto[]): GroupedReport[] => {
        const grouped = new Map<string, GroupedReport>();

        for (const report of reports) {
            let key: string;
            let type: "documentFile" | "comment";
            let targetId: string;

            if (report.documentFileId) {
                key = `file-${report.documentFileId}`;
                type = "documentFile";
                targetId = report.documentFileId;
            } else if (report.commentId) {
                key = `comment-${report.commentId}`;
                type = "comment";
                targetId = report.commentId;
            } else {
                continue;
            }

            if (grouped.has(key)) {
                const existing = grouped.get(key)!;
                existing.reports.push(report);
                existing.reportCount++;
                // Update latest if newer
                if (report.createdAt && existing.latestCreatedAt && report.createdAt > existing.latestCreatedAt) {
                    existing.latestContent = report.content || "";
                    existing.latestReporterName = report.reporterName || "";
                    existing.latestCreatedAt = report.createdAt;
                }
            } else {
                grouped.set(key, {
                    key,
                    type,
                    targetId,
                    reports: [report],
                    reportCount: 1,
                    latestContent: report.content || "",
                    latestReporterName: report.reporterName || "",
                    latestCreatedAt: report.createdAt || "",
                    status: report.status ?? 0,
                });
            }
        }

        return Array.from(grouped.values()).sort((a, b) =>
            b.latestCreatedAt.localeCompare(a.latestCreatedAt)
        );
    }, []);

    return {
        // CRUD operations
        fetchReports: crud.fetchItems,
        reviewReport,

        // Extra functions
        fetchPendingCount,
        groupReports,

        // State
        items: crud.items,
        totalCount: crud.totalCount,
        pendingCount,
        loading: crud.loading,
        error: crud.error,
    };
}
