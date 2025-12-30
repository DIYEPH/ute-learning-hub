"use client";

import { useEffect, useState } from "react";
import { MessagesSquare, Radio, Send, UserRound, Loader2 } from "lucide-react";
import { StatCard, StatsBarChart, StatsLineChart } from "@/src/components/statistics";
import { getApiStatisticsConversations } from "@/src/api/database";
import type { ConversationStatsDto } from "@/src/api/database/types.gen";

interface ConversationsTabProps {
    days: number;
}

export function ConversationsTab({ days }: ConversationsTabProps) {
    const [data, setData] = useState<ConversationStatsDto | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        async function fetchData() {
            setLoading(true);
            setError(null);
            try {
                const response = await getApiStatisticsConversations({
                    query: { days },
                });
                if (response.data) {
                    setData(response.data as ConversationStatsDto);
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

    const subjectChartData = data.conversationsBySubject?.map((item) => ({
        label: item.label ?? "",
        value: item.value ?? 0,
        color: item.color ?? undefined,
    })) ?? [];

    const messagesChartData = data.messagesOverTime?.map((point) => ({
        date: point.date ?? "",
        value: point.value ?? 0,
    })) ?? [];

    return (
        <div className="space-y-6">
            {/* Stat Cards */}
            <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
                <StatCard
                    title="Tổng hội thoại"
                    value={data.totalConversations?.toLocaleString() ?? 0}
                    icon={MessagesSquare}
                    color="primary"
                />
                <StatCard
                    title="Đang hoạt động"
                    value={data.activeConversations?.toLocaleString() ?? 0}
                    icon={Radio}
                    color="success"
                />
                <StatCard
                    title="Tin nhắn (7 ngày)"
                    value={data.totalMessagesLast7Days?.toLocaleString() ?? 0}
                    icon={Send}
                    color="warning"
                />
                <StatCard
                    title="Thành viên TB"
                    value={data.avgMembersPerConversation?.toFixed(1) ?? 0}
                    icon={UserRound}
                    color="default"
                />
            </div>

            {/* Charts */}
            <div className="grid gap-6 lg:grid-cols-2">
                <StatsBarChart
                    title="Hội thoại theo môn học"
                    data={subjectChartData}
                    horizontal
                    height={300}
                />
                <StatsLineChart
                    title="Tin nhắn theo thời gian"
                    data={messagesChartData}
                    color="#8b5cf6"
                />
            </div>
        </div>
    );
}
