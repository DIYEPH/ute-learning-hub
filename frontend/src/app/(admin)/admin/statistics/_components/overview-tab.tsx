"use client";

import { useEffect, useState } from "react";
import { UsersRound, BookOpen, TrendingUp, Flag, MessageSquareMore, Loader2, FileStack } from "lucide-react";
import { StatCard } from "@/src/components/statistics";
import { StatsLineChart } from "@/src/components/statistics";
import {
    getApiStatisticsOverview,
} from "@/src/api/database";
import type { OverviewStatsDto } from "@/src/api/database/types.gen";

interface OverviewTabProps {
    days: number;
}

export function OverviewTab({ days }: OverviewTabProps) {
    const [data, setData] = useState<OverviewStatsDto | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        async function fetchData() {
            setLoading(true);
            setError(null);
            try {
                const response = await getApiStatisticsOverview({
                    query: { days },
                });
                if (response.data) {
                    setData(response.data as OverviewStatsDto);
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

    // Transform time series data for charts
    const usersChartData = data.usersOverTime?.map((point) => ({
        date: point.date ?? "",
        value: point.value ?? 0,
    })) ?? [];

    const documentsChartData = (data.documentsOverTime as Array<{ date?: string; value?: number }>)?.map((point) => ({
        date: point.date ?? "",
        value: point.value ?? 0,
    })) ?? [];

    const viewsChartData = (data.viewsOverTime as Array<{ date?: string; value?: number }>)?.map((point) => ({
        date: point.date ?? "",
        value: point.value ?? 0,
    })) ?? [];

    return (
        <div className="space-y-6">
            {/* Stat Cards Grid */}
            <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
                <StatCard
                    title="Tổng người dùng"
                    value={data.totalUsers?.toLocaleString() ?? 0}
                    description={`+${data.newUsersLast7Days ?? 0} trong 7 ngày qua`}
                    icon={UsersRound}
                    color="primary"
                />
                <StatCard
                    title="Tổng tài liệu"
                    value={data.totalDocuments?.toLocaleString() ?? 0}
                    description={`+${data.newDocumentsLast7Days ?? 0} trong 7 ngày qua`}
                    icon={BookOpen}
                    color="success"
                />
                <StatCard
                    title="Tổng lượt xem"
                    value={data.totalViews?.toLocaleString() ?? 0}
                    icon={TrendingUp}
                    color="warning"
                />
                <StatCard
                    title="Tổng hội thoại"
                    value={data.totalConversations?.toLocaleString() ?? 0}
                    icon={MessageSquareMore}
                    color="default"
                />
            </div>

            {/* Pending Items */}
            <div className="grid gap-4 md:grid-cols-2">
                <StatCard
                    title="Báo cáo chờ xử lý"
                    value={data.pendingReports ?? 0}
                    icon={Flag}
                    color="danger"
                />
                <StatCard
                    title="Tài liệu chờ duyệt"
                    value={data.pendingDocumentFiles ?? 0}
                    icon={FileStack}
                    color="warning"
                />
            </div>

            {/* Charts */}
            <div className="grid gap-6 lg:grid-cols-2">
                <StatsLineChart
                    title="Người dùng mới theo thời gian"
                    data={usersChartData}
                    color="#3b82f6"
                />
                <StatsLineChart
                    title="Tài liệu mới theo thời gian"
                    data={documentsChartData}
                    color="#10b981"
                />
            </div>

            <StatsLineChart
                title="Lượt xem theo thời gian"
                data={viewsChartData}
                color="#f59e0b"
            />
        </div>
    );
}
