"use client";

import { Card, CardContent } from "@/src/components/ui/card";
import { cn } from "@/lib/utils";
import { LucideIcon } from "lucide-react";

interface StatCardProps {
    title: string;
    value: string | number;
    description?: string;
    icon?: LucideIcon;
    trend?: {
        value: number;
        isPositive: boolean;
    };
    className?: string;
    color?: "default" | "primary" | "success" | "warning" | "danger";
}

const colorVariants = {
    default: "bg-muted text-muted-foreground",
    primary: "bg-primary/10 text-primary",
    success: "bg-green-500/10 text-green-600",
    warning: "bg-yellow-500/10 text-yellow-600",
    danger: "bg-red-500/10 text-red-600",
};

export function StatCard({
    title,
    value,
    description,
    icon: Icon,
    trend,
    className,
    color = "primary",
}: StatCardProps) {
    return (
        <Card className={cn("relative overflow-hidden", className)}>
            <CardContent className="p-6">
                <div className="flex items-center justify-between">
                    <div className="space-y-1">
                        <p className="text-sm font-medium text-muted-foreground">
                            {title}
                        </p>
                        <p className="text-2xl font-bold">{value}</p>
                        {description && (
                            <p className="text-xs text-muted-foreground">
                                {description}
                            </p>
                        )}
                        {trend && (
                            <p
                                className={cn(
                                    "text-xs font-medium",
                                    trend.isPositive
                                        ? "text-green-600"
                                        : "text-red-600"
                                )}
                            >
                                {trend.isPositive ? "+" : ""}
                                {trend.value}% so với tuần trước
                            </p>
                        )}
                    </div>
                    {Icon && (
                        <div
                            className={cn(
                                "flex h-12 w-12 items-center justify-center rounded-full",
                                colorVariants[color]
                            )}
                        >
                            <Icon className="h-6 w-6" />
                        </div>
                    )}
                </div>
            </CardContent>
        </Card>
    );
}
