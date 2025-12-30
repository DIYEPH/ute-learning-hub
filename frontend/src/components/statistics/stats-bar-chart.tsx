"use client";

import { Card, CardContent, CardHeader, CardTitle } from "@/src/components/ui/card";
import {
    BarChart,
    Bar,
    XAxis,
    YAxis,
    CartesianGrid,
    Tooltip,
    ResponsiveContainer,
    Cell,
} from "recharts";

interface DataItem {
    label: string;
    value: number;
    color?: string;
}

interface StatsBarChartProps {
    title: string;
    data: DataItem[];
    labelKey?: string;
    valueKey?: string;
    color?: string;
    showGrid?: boolean;
    height?: number;
    horizontal?: boolean;
    colors?: string[];
}

const DEFAULT_COLORS = [
    "#3b82f6", // blue
    "#10b981", // green
    "#f59e0b", // yellow
    "#ef4444", // red
    "#8b5cf6", // purple
    "#ec4899", // pink
    "#06b6d4", // cyan
    "#f97316", // orange
];

export function StatsBarChart({
    title,
    data,
    labelKey = "label",
    valueKey = "value",
    color,
    showGrid = true,
    height = 300,
    horizontal = false,
    colors = DEFAULT_COLORS,
}: StatsBarChartProps) {
    return (
        <Card>
            <CardHeader className="pb-2">
                <CardTitle className="text-base font-medium">{title}</CardTitle>
            </CardHeader>
            <CardContent>
                <ResponsiveContainer width="100%" height={height}>
                    <BarChart
                        data={data}
                        layout={horizontal ? "vertical" : "horizontal"}
                    >
                        {showGrid && (
                            <CartesianGrid
                                strokeDasharray="3 3"
                                className="stroke-muted"
                            />
                        )}
                        {horizontal ? (
                            <>
                                <XAxis type="number" fontSize={12} tickLine={false} />
                                <YAxis
                                    type="category"
                                    dataKey={labelKey}
                                    fontSize={12}
                                    tickLine={false}
                                    width={120}
                                />
                            </>
                        ) : (
                            <>
                                <XAxis
                                    dataKey={labelKey}
                                    fontSize={12}
                                    tickLine={false}
                                    axisLine={false}
                                />
                                <YAxis fontSize={12} tickLine={false} axisLine={false} />
                            </>
                        )}
                        <Tooltip
                            contentStyle={{
                                backgroundColor: "hsl(var(--card))",
                                border: "1px solid hsl(var(--border))",
                                borderRadius: "8px",
                            }}
                        />
                        <Bar dataKey={valueKey} radius={[4, 4, 0, 0]}>
                            {data.map((entry, index) => (
                                <Cell
                                    key={`cell-${index}`}
                                    fill={
                                        entry.color ||
                                        color ||
                                        colors[index % colors.length]
                                    }
                                />
                            ))}
                        </Bar>
                    </BarChart>
                </ResponsiveContainer>
            </CardContent>
        </Card>
    );
}
