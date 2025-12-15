"use client";

import { useState, useEffect, useCallback } from "react";
import { Star } from "lucide-react";
import { Button } from "@/src/components/ui/button";
import { DropdownMenuWrapper } from "@/src/components/ui/dropdown-menu-wrapper";
import {
    DropdownMenuLabel,
    DropdownMenuSeparator,
} from "@/src/components/ui/dropdown-menu";
import { useUserProfile } from "@/src/hooks/use-user-profile";
import { getApiUserByIdTrustHistory } from "@/src/api/database/sdk.gen";
import type { UserTrustHistoryDto } from "@/src/api/database/types.gen";

export function TrustPointsBadge() {
    const { profile, loading: profileLoading } = useUserProfile();
    const [history, setHistory] = useState<UserTrustHistoryDto[]>([]);
    const [loadingHistory, setLoadingHistory] = useState(false);

    const trustPoints = (profile as any)?.trustScore ?? 0;

    const fetchHistory = useCallback(async () => {
        if (!profile?.id) return;
        setLoadingHistory(true);
        try {
            const res = await getApiUserByIdTrustHistory({
                path: { id: profile.id! },
            });
            setHistory(res.data ?? []);
        } catch {
            setHistory([]);
        } finally {
            setLoadingHistory(false);
        }
    }, [profile?.id]);

    const handleOpenChange = useCallback((open: boolean) => {
        if (open) {
            fetchHistory();
        }
    }, [fetchHistory]);

    if (profileLoading || !profile) return null;

    return (
        <DropdownMenuWrapper
            onOpenChange={handleOpenChange}
            trigger={
                <Button
                    variant="ghost"
                    size="sm"
                    className="gap-1 text-amber-600 dark:text-amber-400 hover:text-amber-700 dark:hover:text-amber-300"
                >
                    <Star className="h-4 w-4 fill-current" />
                    <span className="font-medium">{trustPoints}</span>
                </Button>
            }
            align="end"
            contentClassName="w-80"
        >
            <DropdownMenuLabel>Lịch sử điểm tin cậy</DropdownMenuLabel>
            <DropdownMenuSeparator />
            <div className="max-h-64 overflow-y-auto">
                {loadingHistory ? (
                    <p className="p-3 text-sm text-muted-foreground">Đang tải...</p>
                ) : history.length === 0 ? (
                    <p className="p-3 text-sm text-muted-foreground">Chưa có lịch sử</p>
                ) : (
                    <div className="divide-y">
                        {history.slice(0, 10).map((item) => (
                            <div key={item.id} className="p-3 flex justify-between items-start text-sm">
                                <div className="flex-1 min-w-0">
                                    <div className="flex items-center gap-2">
                                        <span className={item.score && item.score > 0 ? "text-green-600 font-medium" : "text-red-600 font-medium"}>
                                            {item.score && item.score > 0 ? `+${item.score}` : item.score}
                                        </span>
                                        <span className="text-xs text-muted-foreground">
                                            ({item.oldScore} → {item.newScore})
                                        </span>
                                    </div>
                                    <p className="text-muted-foreground text-xs mt-0.5 truncate">{item.reason}</p>
                                </div>
                                <span className="text-xs text-muted-foreground whitespace-nowrap ml-2">
                                    {item.createdAt && new Date(item.createdAt).toLocaleDateString("vi-VN")}
                                </span>
                            </div>
                        ))}
                    </div>
                )}
            </div>
        </DropdownMenuWrapper>
    );
}
