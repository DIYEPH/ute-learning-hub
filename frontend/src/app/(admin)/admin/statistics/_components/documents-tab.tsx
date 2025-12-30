"use client";

import { useEffect, useState } from "react";
import { Library, BarChart3, CircleCheck, CircleDashed, Sparkles, Ban, Loader2 } from "lucide-react";
import { StatCard, StatsBarChart, StatsPieChart, StatsLineChart, StatsTable } from "@/src/components/statistics";
import { getApiStatisticsDocuments } from "@/src/api/database";
import type { DocumentStatsDto, TopDocumentDto } from "@/src/api/database/types.gen";
import { Avatar, AvatarFallback, AvatarImage } from "@/src/components/ui/avatar";

interface DocumentsTabProps {
    days: number;
}

export function DocumentsTab({ days }: DocumentsTabProps) {
    const [data, setData] = useState<DocumentStatsDto | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        async function fetchData() {
            setLoading(true);
            setError(null);
            try {
                const response = await getApiStatisticsDocuments({
                    query: { days },
                });
                if (response.data) {
                    setData(response.data as DocumentStatsDto);
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

    const subjectChartData = data.documentsBySubject?.map((item) => ({
        label: item.label ?? "",
        value: item.value ?? 0,
        color: item.color ?? undefined,
    })) ?? [];

    const typeChartData = (data.documentsByType as Array<{ label?: string; value?: number; color?: string | null }>)?.map((item) => ({
        label: item.label ?? "",
        value: item.value ?? 0,
        color: item.color ?? undefined,
    })) ?? [];

    const viewsChartData = data.viewsOverTime?.map((point) => ({
        date: point.date ?? "",
        value: point.value ?? 0,
    })) ?? [];

    const topDocsColumns = [
        {
            key: "name" as const,
            header: "Tài liệu",
            render: (item: TopDocumentDto) => (
                <div className="flex items-center gap-3">
                    <Avatar className="h-10 w-10 rounded">
                        <AvatarImage src={item.coverUrl ?? undefined} alt={item.name} />
                        <AvatarFallback className="rounded">{item.name?.charAt(0)}</AvatarFallback>
                    </Avatar>
                    <span className="font-medium line-clamp-1">{item.name}</span>
                </div>
            ),
        },
        {
            key: "viewCount" as const,
            header: "Lượt xem",
            align: "right" as const,
            render: (item: TopDocumentDto) => item.viewCount?.toLocaleString() ?? 0,
        },
    ];

    return (
        <div className="space-y-6">
            {/* Stat Cards */}
            <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
                <StatCard
                    title="Tổng tài liệu"
                    value={data.totalDocuments?.toLocaleString() ?? 0}
                    icon={Library}
                    color="primary"
                />
                <StatCard
                    title="Đã duyệt"
                    value={data.approvedDocuments?.toLocaleString() ?? 0}
                    icon={CircleCheck}
                    color="success"
                />
                <StatCard
                    title="Chờ duyệt"
                    value={data.pendingDocuments?.toLocaleString() ?? 0}
                    icon={CircleDashed}
                    color="warning"
                />
                <StatCard
                    title="Tổng lượt xem"
                    value={data.totalViews?.toLocaleString() ?? 0}
                    description={`Trung bình ${data.avgViewsPerDocument?.toFixed(1) ?? 0}/tài liệu`}
                    icon={BarChart3}
                    color="default"
                />
            </div>

            {/* Reviews stats */}
            <div className="grid gap-4 md:grid-cols-2">
                <StatCard
                    title="Đánh giá hữu ích"
                    value={data.totalUsefulReviews?.toLocaleString() ?? 0}
                    icon={Sparkles}
                    color="success"
                />
                <StatCard
                    title="Đánh giá không hữu ích"
                    value={data.totalNotUsefulReviews?.toLocaleString() ?? 0}
                    icon={Ban}
                    color="danger"
                />
            </div>

            {/* Charts */}
            <div className="grid gap-6 lg:grid-cols-2">
                <StatsBarChart
                    title="Tài liệu theo môn học"
                    data={subjectChartData}
                    horizontal
                    height={300}
                />
                <StatsPieChart
                    title="Tài liệu theo loại"
                    data={typeChartData}
                    showLegend
                />
            </div>

            {/* Views over time */}
            <StatsLineChart
                title="Lượt xem theo thời gian"
                data={viewsChartData}
                color="#3b82f6"
            />

            {/* Top Documents Table */}
            <StatsTable
                title="Top tài liệu được xem nhiều nhất"
                data={data.topDocumentsByViews ?? []}
                columns={topDocsColumns}
            />
        </div>
    );
}
