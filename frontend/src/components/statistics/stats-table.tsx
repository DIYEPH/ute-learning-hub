"use client";

import { Card, CardContent, CardHeader, CardTitle } from "@/src/components/ui/card";
import {
    Table,
    TableBody,
    TableCell,
    TableHead,
    TableHeader,
    TableRow,
} from "@/src/components/ui/table";
import { Avatar, AvatarFallback, AvatarImage } from "@/src/components/ui/avatar";

interface StatsTableColumn<T> {
    key: keyof T | string;
    header: string;
    render?: (item: T, index: number) => React.ReactNode;
    align?: "left" | "center" | "right";
}

interface StatsTableProps<T> {
    title: string;
    data: T[];
    columns: StatsTableColumn<T>[];
    emptyMessage?: string;
}

export function StatsTable<T extends Record<string, unknown>>({
    title,
    data,
    columns,
    emptyMessage = "Không có dữ liệu",
}: StatsTableProps<T>) {
    return (
        <Card>
            <CardHeader className="pb-2">
                <CardTitle className="text-base font-medium">{title}</CardTitle>
            </CardHeader>
            <CardContent>
                <Table>
                    <TableHeader>
                        <TableRow>
                            {columns.map((col) => (
                                <TableHead
                                    key={String(col.key)}
                                    className={
                                        col.align === "center"
                                            ? "text-center"
                                            : col.align === "right"
                                                ? "text-right"
                                                : ""
                                    }
                                >
                                    {col.header}
                                </TableHead>
                            ))}
                        </TableRow>
                    </TableHeader>
                    <TableBody>
                        {data.length === 0 ? (
                            <TableRow>
                                <TableCell
                                    colSpan={columns.length}
                                    className="text-center text-muted-foreground"
                                >
                                    {emptyMessage}
                                </TableCell>
                            </TableRow>
                        ) : (
                            data.map((item, index) => (
                                <TableRow key={index}>
                                    {columns.map((col) => (
                                        <TableCell
                                            key={String(col.key)}
                                            className={
                                                col.align === "center"
                                                    ? "text-center"
                                                    : col.align === "right"
                                                        ? "text-right"
                                                        : ""
                                            }
                                        >
                                            {col.render
                                                ? col.render(item, index)
                                                : (item[col.key as keyof T] as React.ReactNode)}
                                        </TableCell>
                                    ))}
                                </TableRow>
                            ))
                        )}
                    </TableBody>
                </Table>
            </CardContent>
        </Card>
    );
}

// Helper component for user avatar in tables
interface UserCellProps {
    name: string;
    avatarUrl?: string | null;
    subtitle?: string;
}

export function UserCell({ name, avatarUrl, subtitle }: UserCellProps) {
    return (
        <div className="flex items-center gap-3">
            <Avatar className="h-8 w-8">
                <AvatarImage src={avatarUrl || undefined} alt={name} />
                <AvatarFallback>{name.charAt(0).toUpperCase()}</AvatarFallback>
            </Avatar>
            <div>
                <p className="font-medium">{name}</p>
                {subtitle && (
                    <p className="text-xs text-muted-foreground">{subtitle}</p>
                )}
            </div>
        </div>
    );
}
