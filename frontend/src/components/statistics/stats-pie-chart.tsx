"use client";

import { Card, CardContent, CardHeader, CardTitle } from "@/src/components/ui/card";
import {
    PieChart,
    Pie,
    Cell,
    ResponsiveContainer,
    Tooltip,
    Legend,
} from "recharts";

interface DataItem {
    label: string;
    value: number;
    color?: string;
    [key: string]: unknown;
}

interface StatsPieChartProps {
    title: string;
    data: DataItem[];
    labelKey?: string;
    valueKey?: string;
    height?: number;
    innerRadius?: number;
    outerRadius?: number;
    showLegend?: boolean;
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
    "#6366f1", // indigo
    "#14b8a6", // teal
];

export function StatsPieChart({
    title,
    data,
    labelKey = "label",
    valueKey = "value",
    height = 300,
    innerRadius = 0,
    outerRadius = 80,
    showLegend = true,
    colors = DEFAULT_COLORS,
}: StatsPieChartProps) {
    return (
        <Card>
            <CardHeader className="pb-2">
                <CardTitle className="text-base font-medium">{title}</CardTitle>
            </CardHeader>
            <CardContent>
                <ResponsiveContainer width="100%" height={height}>
                    <PieChart>
                        <Pie
                            data={data}
                            cx="50%"
                            cy="50%"
                            innerRadius={innerRadius}
                            outerRadius={outerRadius}
                            paddingAngle={2}
                            dataKey={valueKey}
                            nameKey={labelKey}
                            label={({ name, percent }) =>
                                `${name} ${((percent ?? 0) * 100).toFixed(0)}%`
                            }
                            labelLine={false}
                        >
                            {data.map((entry, index) => (
                                <Cell
                                    key={`cell-${index}`}
                                    fill={entry.color || colors[index % colors.length]}
                                />
                            ))}
                        </Pie>
                        <Tooltip
                            contentStyle={{
                                backgroundColor: "hsl(var(--card))",
                                border: "1px solid hsl(var(--border))",
                                borderRadius: "8px",
                            }}
                        />
                        {showLegend && (
                            <Legend
                                layout="vertical"
                                align="right"
                                verticalAlign="middle"
                            />
                        )}
                    </PieChart>
                </ResponsiveContainer>
            </CardContent>
        </Card>
    );
}

// Donut chart variant
export function StatsDoughnutChart(props: StatsPieChartProps) {
    return (
        <StatsPieChart
            {...props}
            innerRadius={props.innerRadius ?? 50}
            outerRadius={props.outerRadius ?? 80}
        />
    );
}
