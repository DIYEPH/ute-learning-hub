"use client";

import { useEffect, useState } from "react";
import { ShieldAlert, Hourglass, CheckCircle2, MessageSquareX, VolumeX, FileQuestion, Loader2 } from "lucide-react";
import { StatCard, StatsPieChart, StatsLineChart, StatsTable, UserCell } from "@/src/components/statistics";
import { getApiStatisticsModeration } from "@/src/api/database";
import type { ModerationStatsDto, TopReportedUserDto } from "@/src/api/database/types.gen";

interface ModerationTabProps {
    days: number;
}

export function ModerationTab({ days }: ModerationTabProps) {
    const [data, setData] = useState<ModerationStatsDto | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        async function fetchData() {
            setLoading(true);
            setError(null);
            try {
                const response = await getApiStatisticsModeration({
                    query: { days },
                });
                if (response.data) {
                    setData(response.data as ModerationStatsDto);
                }
            } catch (err) {
                setError("Không thể tải dữ liệu thống kê");
                console.error(err);
            } finally {
                setLoading(false);
            }
        }
        fetchData();
    }, [days]);

    if (loading) {
        return (
            <div className="flex h-64 items-center justify-center">
                <Loader2 className="h-8 w-8 animate-spin text-muted-foreground" />
            </div>
        );
    }

    if (error || !data) {
        return (
            <div className="flex h-64 items-center justify-center text-muted-foreground">
                {error || "Không có dữ liệu"}
            </div>
        );
    }

    const reasonChartData = data.reportsByReason?.map((item) => ({
        label: item.label ?? "",
        value: item.value ?? 0,
        color: item.color ?? undefined,
    })) ?? [];

    const reportsChartData = data.reportsOverTime?.map((point) => ({
        date: point.date ?? "",
        value: point.value ?? 0,
    })) ?? [];

    const topReportedColumns = [
        {
            key: "fullName" as const,
            header: "Người dùng",
            render: (item: TopReportedUserDto) => (
                <UserCell
                    name={item.fullName ?? ""}
                    avatarUrl={item.avatarUrl}
                />
            ),
        },
        {
            key: "reportCount" as const,
            header: "Số báo cáo",
            align: "right" as const,
            render: (item: TopReportedUserDto) => (
                <span className="font-medium text-destructive">{item.reportCount ?? 0}</span>
            ),
        },
    ];

    return (
        <div className="space-y-6">
            {/* Stat Cards */}
            <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
                <StatCard
                    title="Tổng báo cáo"
                    value={data.totalReports?.toLocaleString() ?? 0}
                    icon={ShieldAlert}
                    color="default"
                />
                <StatCard
                    title="Chờ xử lý"
                    value={data.pendingReports?.toLocaleString() ?? 0}
                    icon={Hourglass}
                    color="warning"
                />
                <StatCard
                    title="Đã xử lý"
                    value={data.approvedReports?.toLocaleString() ?? 0}
                    icon={CheckCircle2}
                    color="success"
                />
            </div>

            {/* Content moderation stats */}
            <div className="grid gap-4 md:grid-cols-3">
                <StatCard
                    title="Bình luận chờ duyệt"
                    value={data.pendingComments?.toLocaleString() ?? 0}
                    icon={MessageSquareX}
                    color="warning"
                />
                <StatCard
                    title="Bình luận bị ẩn"
                    value={data.hiddenComments?.toLocaleString() ?? 0}
                    icon={VolumeX}
                    color="danger"
                />
                <StatCard
                    title="Tài liệu chờ duyệt"
                    value={data.pendingDocumentFiles?.toLocaleString() ?? 0}
                    icon={FileQuestion}
                    color="warning"
                />
            </div>

            {/* Charts */}
            <div className="grid gap-6 lg:grid-cols-2">
                <StatsPieChart
                    title="Báo cáo theo lý do"
                    data={reasonChartData}
                    showLegend
                />
                <StatsLineChart
                    title="Báo cáo theo thời gian"
                    data={reportsChartData}
                    color="#ef4444"
                />
            </div>

            {/* Top Reported Users Table */}
            <StatsTable
                title="Người dùng bị báo cáo nhiều nhất"
                data={data.topReportedUsers ?? []}
                columns={topReportedColumns}
            />
        </div>
    );
}
