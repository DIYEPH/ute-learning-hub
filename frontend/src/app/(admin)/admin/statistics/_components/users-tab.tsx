"use client";

import { useEffect, useState } from "react";
import { Contact, Zap, ShieldOff, Award, Loader2 } from "lucide-react";
import { StatCard, StatsBarChart, StatsPieChart, StatsLineChart, StatsTable, UserCell } from "@/src/components/statistics";
import { getApiStatisticsUsers } from "@/src/api/database";
import type { UserStatsDto2, TopUserDto } from "@/src/api/database/types.gen";

interface UsersTabProps {
    days: number;
}

export function UsersTab({ days }: UsersTabProps) {
    const [data, setData] = useState<UserStatsDto2 | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        async function fetchData() {
            setLoading(true);
            setError(null);
            try {
                const response = await getApiStatisticsUsers({
                    query: { days },
                });
                if (response.data) {
                    setData(response.data as UserStatsDto2);
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

    const majorChartData = data.usersByMajor?.map((item) => ({
        label: item.label ?? "",
        value: item.value ?? 0,
        color: item.color ?? undefined,
    })) ?? [];

    const trustLevelChartData = (data.usersByTrustLevel as Array<{ label?: string; value?: number; color?: string | null }>)?.map((item) => ({
        label: item.label ?? "",
        value: item.value ?? 0,
        color: item.color ?? undefined,
    })) ?? [];

    const registrationsChartData = data.registrationsOverTime?.map((point) => ({
        date: point.date ?? "",
        value: point.value ?? 0,
    })) ?? [];

    const topContributorsColumns = [
        {
            key: "fullName" as const,
            header: "Người dùng",
            render: (item: TopUserDto) => (
                <UserCell
                    name={item.fullName ?? ""}
                    avatarUrl={item.avatarUrl}
                    subtitle={`${item.documentCount ?? 0} tài liệu`}
                />
            ),
        },
        {
            key: "trustScore" as const,
            header: "Điểm tin cậy",
            align: "right" as const,
            render: (item: TopUserDto) => (
                <span className="font-medium text-primary">{item.trustScore ?? 0}</span>
            ),
        },
    ];

    return (
        <div className="space-y-6">
            {/* Stat Cards */}
            <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
                <StatCard
                    title="Tổng người dùng"
                    value={data.totalUsers?.toLocaleString() ?? 0}
                    icon={Contact}
                    color="primary"
                />
                <StatCard
                    title="Hoạt động (7 ngày)"
                    value={data.activeUsersLast7Days?.toLocaleString() ?? 0}
                    icon={Zap}
                    color="success"
                />
                <StatCard
                    title="Bị cấm"
                    value={data.bannedUsers?.toLocaleString() ?? 0}
                    icon={ShieldOff}
                    color="danger"
                />
                <StatCard
                    title="Điểm tin cậy TB"
                    value={data.avgTrustScore?.toFixed(1) ?? 0}
                    icon={Award}
                    color="warning"
                />
            </div>

            {/* Charts */}
            <div className="grid gap-6 lg:grid-cols-2">
                <StatsBarChart
                    title="Người dùng theo ngành"
                    data={majorChartData}
                    horizontal
                    height={300}
                />
                <StatsPieChart
                    title="Người dùng theo mức tin cậy"
                    data={trustLevelChartData}
                    showLegend
                />
            </div>

            {/* Registrations over time */}
            <StatsLineChart
                title="Đăng ký mới theo thời gian"
                data={registrationsChartData}
                color="#10b981"
            />

            {/* Top Contributors Table */}
            <StatsTable
                title="Top người đóng góp"
                data={data.topContributors ?? []}
                columns={topContributorsColumns}
            />
        </div>
    );
}
