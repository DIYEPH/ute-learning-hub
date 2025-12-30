"use client";

import { useState, useEffect, useCallback } from "react";
import { getApiReport, getApiDocumentPendingFilesCount } from "@/src/api";
import type { PagedResponse } from "./use-crud";
import type { ReportDto } from "@/src/api/database/types.gen";

export function useAdminBadges() {
    const [pendingReports, setPendingReports] = useState(0);
    const [pendingDocumentFiles, setPendingDocumentFiles] = useState(0);
    const [loading, setLoading] = useState(true);

    const fetchBadges = useCallback(async () => {
        setLoading(true);
        try {
            // Fetch pending reports count (Status = 0 is PendingReview)
            const reportRes = await getApiReport({ query: { Status: "0", PageSize: 1 } });
            const reportData = (reportRes as unknown as { data: PagedResponse<ReportDto> })?.data || reportRes as PagedResponse<ReportDto>;
            setPendingReports(reportData.totalCount || 0);

            // Fetch pending document files count from dedicated API
            const pendingFilesRes = await getApiDocumentPendingFilesCount();
            const pendingFilesData = (pendingFilesRes as any)?.data ?? pendingFilesRes ?? 0;
            setPendingDocumentFiles(pendingFilesData);
        } catch (err) {
            console.error("Error fetching admin badges:", err);
        } finally {
            setLoading(false);
        }
    }, []);

    useEffect(() => {
        fetchBadges();
    }, [fetchBadges]);

    return {
        pendingReports,
        pendingDocumentFiles,
        loading,
        refresh: fetchBadges,
    };
}
