"use client";

import { useState, useEffect } from "react";
import { Loader2, Check, X, Clock } from "lucide-react";
import { Button } from "@/src/components/ui/button";
import { Avatar, AvatarFallback, AvatarImage } from "@/src/components/ui/avatar";
import {
    getApiConversationJoinRequest,
    postApiConversationJoinRequestByIdReview,
} from "@/src/api";
import type { ConversationJoinRequestDto } from "@/src/api/database/types.gen";
import { useNotification } from "@/src/components/providers/notification-provider";

interface JoinRequestListProps {
    conversationId: string;
    onSuccess?: () => void;
}

export function JoinRequestList({ conversationId, onSuccess }: JoinRequestListProps) {
    const [requests, setRequests] = useState<ConversationJoinRequestDto[]>([]);
    const [loading, setLoading] = useState(true);
    const [reviewingId, setReviewingId] = useState<string | null>(null);
    const { success: notifySuccess, error: notifyError } = useNotification();

    const fetchRequests = async () => {
        setLoading(true);
        try {
            const response = await getApiConversationJoinRequest({
                query: {
                    ConversationId: conversationId,
                    Status: '0', // PendingReview
                    Page: 1,
                    PageSize: 50,
                },
            });
            const payload = (response.data ?? response) as any;
            setRequests(payload?.items ?? []);
        } catch (err) {
            console.error("Error fetching join requests:", err);
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        if (conversationId) {
            fetchRequests();
        }
    }, [conversationId]);

    const handleReview = async (requestId: string, approve: boolean) => {
        setReviewingId(requestId);
        try {
            await postApiConversationJoinRequestByIdReview<true>({
                path: { id: requestId },
                body: {
                    status: approve ? 1 : 2, // 1 = Approved, 2 = Rejected
                    reviewNote: undefined,
                },
                throwOnError: true,
            });
            notifySuccess(approve ? "Đã duyệt yêu cầu" : "Đã từ chối yêu cầu");
            fetchRequests();
            onSuccess?.();
        } catch (err: any) {
            notifyError(err?.message || "Không thể xử lý yêu cầu");
        } finally {
            setReviewingId(null);
        }
    };

    if (loading) {
        return (
            <div className="flex items-center justify-center py-4">
                <Loader2 className="h-5 w-5 animate-spin text-muted-foreground" />
            </div>
        );
    }

    if (requests.length === 0) {
        return (
            <p className="text-sm text-muted-foreground py-2">
                Không có yêu cầu nào đang chờ duyệt
            </p>
        );
    }

    return (
        <div className="space-y-3">
            {requests.map((req) => (
                <div
                    key={req.id}
                    className="flex items-start gap-3 p-3 bg-muted rounded-lg"
                >
                    <Avatar className="h-9 w-9 flex-shrink-0">
                        <AvatarImage src={req.requesterAvatarUrl || undefined} />
                        <AvatarFallback>{req.requesterName?.[0] || "?"}</AvatarFallback>
                    </Avatar>

                    <div className="flex-1 min-w-0">
                        <p className="text-sm font-medium text-foreground truncate">
                            {req.requesterName || "Người dùng"}
                        </p>
                        {req.content && (
                            <p className="text-xs text-muted-foreground mt-0.5 line-clamp-2">
                                {req.content}
                            </p>
                        )}
                        <div className="flex items-center gap-1 mt-1 text-[10px] text-muted-foreground">
                            <Clock className="h-3 w-3" />
                            {req.createdAt ? new Date(req.createdAt).toLocaleDateString("vi-VN") : ""}
                        </div>
                    </div>

                    <div className="flex gap-1 flex-shrink-0">
                        <Button
                            size="sm"
                            variant="ghost"
                            className="h-8 w-8 p-0 text-green-600 hover:text-green-700 hover:bg-green-50 dark:hover:bg-green-900/30"
                            onClick={() => req.id && handleReview(req.id, true)}
                            disabled={reviewingId === req.id}
                        >
                            {reviewingId === req.id ? (
                                <Loader2 className="h-4 w-4 animate-spin" />
                            ) : (
                                <Check className="h-4 w-4" />
                            )}
                        </Button>
                        <Button
                            size="sm"
                            variant="ghost"
                            className="h-8 w-8 p-0 text-red-600 hover:text-red-700 hover:bg-red-50 dark:hover:bg-red-900/30"
                            onClick={() => req.id && handleReview(req.id, false)}
                            disabled={reviewingId === req.id}
                        >
                            <X className="h-4 w-4" />
                        </Button>
                    </div>
                </div>
            ))}
        </div>
    );
}
