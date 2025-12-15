"use client";

import { useState, useEffect, useCallback } from "react";
import { Pagination } from "@/src/components/ui/pagination";
import { useTranslations } from "next-intl";
import { useReports, type GroupedReport } from "@/src/hooks/use-reports";
import { useNotification } from "@/src/components/providers/notification-provider";
import { ReportTable } from "@/src/components/admin/reports/report-table";
import { ReportDetailModal } from "@/src/components/admin/reports/report-detail-modal";
import { AdvancedSearchFilter } from "@/src/components/admin/advanced-search-filter";
import type { ReportDto } from "@/src/api/database/types.gen";

export default function ReportsManagementPage() {
    const t = useTranslations("admin.reports");
    const notification = useNotification();
    const { fetchReports, reviewReport, groupReports, loading, error } = useReports();

    const [reports, setReports] = useState<ReportDto[]>([]);
    const [groupedReports, setGroupedReports] = useState<GroupedReport[]>([]);
    const [totalCount, setTotalCount] = useState(0);
    const [page, setPage] = useState(1);
    const [pageSize] = useState(50); // Fetch more to group properly
    const [searchTerm, setSearchTerm] = useState("");
    const [statusFilter, setStatusFilter] = useState<string | null>(null);
    const [typeFilter, setTypeFilter] = useState<string | null>(null);

    const [detailModalOpen, setDetailModalOpen] = useState(false);
    const [selectedGrouped, setSelectedGrouped] = useState<GroupedReport | null>(null);
    const [formLoading, setFormLoading] = useState(false);

    const loadReports = useCallback(async () => {
        try {
            const response = await fetchReports({
                SearchTerm: searchTerm || undefined,
                Status: statusFilter ? parseInt(statusFilter) as any : undefined,
                Page: page,
                PageSize: pageSize,
            });

            if (response) {
                let items = response.items || [];

                // Client-side filter by type if needed
                if (typeFilter === "documentFile") {
                    items = items.filter(r => r.documentFileId);
                } else if (typeFilter === "comment") {
                    items = items.filter(r => r.commentId);
                }

                setReports(items);
                setTotalCount(response.totalCount || 0);
                setGroupedReports(groupReports(items));
            }
        } catch (err) {
            console.error("Error loading reports:", err);
        }
    }, [fetchReports, groupReports, searchTerm, statusFilter, typeFilter, page, pageSize]);

    useEffect(() => {
        loadReports();
    }, [loadReports]);

    const handleApprove = async (reportIds: string[]) => {
        setFormLoading(true);
        try {
            await Promise.all(
                reportIds.map(id => reviewReport(id, { reportId: id, status: 1 }))
            );
            await loadReports();
            setDetailModalOpen(false);
            setSelectedGrouped(null);
            notification.success(t("notifications.approveSuccess"));
        } catch (err) {
            console.error("Error approving reports:", err);
            notification.error(t("notifications.approveError"));
        } finally {
            setFormLoading(false);
        }
    };

    const handleHide = async (reportIds: string[]) => {
        setFormLoading(true);
        try {
            await Promise.all(
                reportIds.map(id => reviewReport(id, { reportId: id, status: 2 }))
            );
            await loadReports();
            setDetailModalOpen(false);
            setSelectedGrouped(null);
            notification.success(t("notifications.hideSuccess"));
        } catch (err) {
            console.error("Error hiding content:", err);
            notification.error(t("notifications.hideError"));
        } finally {
            setFormLoading(false);
        }
    };

    const handleReset = () => {
        setSearchTerm("");
        setStatusFilter(null);
        setTypeFilter(null);
        setPage(1);
    };

    const handleFilterChange = (key: string, value: string | null) => {
        if (key === "status") {
            setStatusFilter(value);
        } else if (key === "type") {
            setTypeFilter(value);
        }
        setPage(1);
    };

    const totalPages = Math.ceil(totalCount / pageSize);

    return (
        <div>
            <div className="mb-6">
                <h1 className="text-xl md:text-2xl font-semibold text-foreground">{t("title")}</h1>
            </div>

            <div className="mb-4">
                <AdvancedSearchFilter
                    searchTerm={searchTerm}
                    onSearchChange={setSearchTerm}
                    placeholder={t("searchPlaceholder")}
                    filters={[
                        {
                            key: "status",
                            label: t("filter.status"),
                            type: "select",
                            value: statusFilter,
                            options: [
                                { value: "0", label: t("filter.pending") },
                                { value: "1", label: t("filter.approved") },
                                { value: "2", label: t("filter.rejected") },
                            ],
                        },
                        {
                            key: "type",
                            label: t("filter.type"),
                            type: "select",
                            value: typeFilter,
                            options: [
                                { value: "documentFile", label: t("type.documentFile") },
                                { value: "comment", label: t("type.comment") },
                            ],
                        },
                    ]}
                    onFilterChange={handleFilterChange}
                    onReset={handleReset}
                />
            </div>

            {error && (
                <div className="mb-4 p-4 bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 ">
                    <p className="text-sm text-red-600 dark:text-red-400">{error}</p>
                </div>
            )}

            {groupedReports.length > 0 && (
                <div className="mb-2 text-sm text-slate-600 dark:text-slate-400">
                    {t("foundCount", { count: groupedReports.length })}
                </div>
            )}

            <div className="mt-4">
                <ReportTable
                    groupedReports={groupedReports}
                    loading={loading}
                    onViewDetail={(grouped) => {
                        setSelectedGrouped(grouped);
                        setDetailModalOpen(true);
                    }}
                />
            </div>

            <Pagination
                currentPage={page}
                totalPages={totalPages}
                totalItems={totalCount}
                pageSize={pageSize}
                onPageChange={setPage}
                loading={loading}
                className="mt-4"
            />

            {/* Detail Modal */}
            <ReportDetailModal
                open={detailModalOpen}
                onOpenChange={(open) => {
                    setDetailModalOpen(open);
                    if (!open) setSelectedGrouped(null);
                }}
                grouped={selectedGrouped}
                onApprove={handleApprove}
                onHide={handleHide}
                loading={formLoading}
            />
        </div>
    );
}

