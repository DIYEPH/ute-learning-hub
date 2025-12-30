"use client";

import { Card, CardContent, CardHeader, CardTitle } from "@/src/components/ui/card";
import {
    LineChart,
    Line,
    XAxis,
    YAxis,
    CartesianGrid,
    Tooltip,
    ResponsiveContainer,
    Legend,
} from "recharts";

interface DataPoint {
    date: string;
    value: number;
    [key: string]: string | number;
}

interface StatsLineChartProps {
    title: string;
    data: DataPoint[];
    dataKey?: string;
    xAxisKey?: string;
    color?: string;
    showGrid?: boolean;
    height?: number;
    multiLine?: {
        key: string;
        color: string;
        name: string;
    }[];
}

export function StatsLineChart({
    title,
    data,
    dataKey = "value",
    xAxisKey = "date",
    color = "var(--primary)",
    showGrid = true,
    height = 300,
    multiLine,
}: StatsLineChartProps) {
    return (
        <Card>
            <CardHeader className="pb-2">
                <CardTitle className="text-base font-medium">{title}</CardTitle>
            </CardHeader>
            <CardContent>
                <ResponsiveContainer width="100%" height={height}>
                    <LineChart data={data}>
                        {showGrid && (
                            <CartesianGrid
                                strokeDasharray="3 3"
                                className="stroke-muted"
                            />
                        )}
                        <XAxis
                            dataKey={xAxisKey}
                            fontSize={12}
                            tickLine={false}
                            axisLine={false}
                            className="fill-muted-foreground"
                        />
                        <YAxis
                            fontSize={12}
                            tickLine={false}
                            axisLine={false}
                            className="fill-muted-foreground"
                        />
                        <Tooltip
                            contentStyle={{
                                backgroundColor: "hsl(var(--card))",
                                border: "1px solid hsl(var(--border))",
                                borderRadius: "8px",
                            }}
                        />
                        {multiLine ? (
                            <>
                                <Legend />
                                {multiLine.map((line) => (
                                    <Line
                                        key={line.key}
                                        type="monotone"
                                        dataKey={line.key}
                                        stroke={line.color}
                                        strokeWidth={2}
                                        dot={false}
                                        name={line.name}
                                    />
                                ))}
                            </>
                        ) : (
                            <Line
                                type="monotone"
                                dataKey={dataKey}
                                stroke={color}
                                strokeWidth={2}
                                dot={false}
                            />
                        )}
                    </LineChart>
                </ResponsiveContainer>
            </CardContent>
        </Card>
    );
}
