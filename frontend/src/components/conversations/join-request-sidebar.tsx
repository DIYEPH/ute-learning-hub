"use client";

import { useState, useEffect } from "react";
import { X, Loader2, Check, UserPlus, Clock } from "lucide-react";
import { cn } from "@/lib/utils";
import { Button } from "@/src/components/ui/button";
import { ScrollArea } from "@/src/components/ui/scroll-area";
import { Avatar, AvatarFallback, AvatarImage } from "@/src/components/ui/avatar";
import {
    getApiConversationJoinRequest,
    postApiConversationJoinRequestByIdReview,
} from "@/src/api/database/sdk.gen";
import type { ConversationJoinRequestDto } from "@/src/api/database/types.gen";
import { useNotification } from "@/src/components/providers/notification-provider";

interface JoinRequestSidebarProps {
    open: boolean;
    onClose: () => void;
    conversationId: string;
    conversationName: string;
    onSuccess?: () => void;
}

export function JoinRequestSidebar({
    open,
    onClose,
    conversationId,
    conversationName,
    onSuccess,
}: JoinRequestSidebarProps) {
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
        if (open && conversationId) {
            fetchRequests();
        }
    }, [open, conversationId]);

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

    return (
        <>
            {/* Overlay cho mobile */}
            {open && (
                <div
                    className="fixed inset-0 bg-black/50 z-40 md:hidden"
                    onClick={onClose}
                />
            )}

            {/* Sidebar */}
            <div
                className={cn(
                    "fixed top-0 right-0 h-full bg-white dark:bg-slate-900 border-l border-slate-200 dark:border-slate-700 z-50 transition-transform duration-300 ease-in-out",
                    "w-full md:w-80",
                    open ? "translate-x-0" : "translate-x-full"
                )}
            >
                <div className="flex flex-col h-full">
                    {/* Header */}
                    <div className="flex items-center justify-between p-4 border-b border-slate-200 dark:border-slate-700">
                        <div className="flex items-center gap-2">
                            <UserPlus className="h-5 w-5 text-sky-500" />
                            <h3 className="text-lg font-semibold text-foreground">
                                Yêu cầu tham gia
                            </h3>
                        </div>
                        <Button
                            variant="ghost"
                            size="sm"
                            onClick={onClose}
                            className="h-8 w-8 p-0"
                        >
                            <X className="h-4 w-4" />
                        </Button>
                    </div>

                    {/* Content */}
                    <ScrollArea className="flex-1">
                        <div className="p-4">
                            {loading ? (
                                <div className="flex items-center justify-center py-12">
                                    <Loader2 className="h-6 w-6 animate-spin text-slate-400" />
                                </div>
                            ) : requests.length === 0 ? (
                                <div className="text-center py-12">
                                    <UserPlus className="h-12 w-12 mx-auto mb-3 text-slate-300 dark:text-slate-600" />
                                    <p className="text-sm text-slate-500 dark:text-slate-400">
                                        Không có yêu cầu nào đang chờ duyệt
                                    </p>
                                </div>
                            ) : (
                                <div className="space-y-3">
                                    {requests.map((req) => (
                                        <div
                                            key={req.id}
                                            className="p-3 bg-slate-50 dark:bg-slate-800/50 rounded-lg"
                                        >
                                            <div className="flex items-start gap-3">
                                                <Avatar className="h-10 w-10 flex-shrink-0">
                                                    <AvatarImage src={req.requesterAvatarUrl || undefined} />
                                                    <AvatarFallback>{req.requesterName?.[0] || "?"}</AvatarFallback>
                                                </Avatar>

                                                <div className="flex-1 min-w-0">
                                                    <p className="text-sm font-medium text-foreground">
                                                        {req.requesterName || "Người dùng"}
                                                    </p>
                                                    {req.content && (
                                                        <p className="text-xs text-slate-500 dark:text-slate-400 mt-1">
                                                            "{req.content}"
                                                        </p>
                                                    )}
                                                    <div className="flex items-center gap-1 mt-2 text-[10px] text-slate-400">
                                                        <Clock className="h-3 w-3" />
                                                        {req.createdAt ? new Date(req.createdAt).toLocaleDateString("vi-VN") : ""}
                                                    </div>
                                                </div>
                                            </div>

                                            <div className="flex gap-2 mt-3">
                                                <Button
                                                    size="sm"
                                                    variant="default"
                                                    className="flex-1"
                                                    onClick={() => req.id && handleReview(req.id, true)}
                                                    disabled={reviewingId === req.id}
                                                >
                                                    {reviewingId === req.id ? (
                                                        <Loader2 className="h-4 w-4 animate-spin" />
                                                    ) : (
                                                        <>
                                                            <Check className="h-4 w-4 mr-1" />
                                                            Duyệt
                                                        </>
                                                    )}
                                                </Button>
                                                <Button
                                                    size="sm"
                                                    variant="outline"
                                                    className="flex-1 text-red-600 border-red-200 hover:bg-red-50 dark:border-red-800 dark:hover:bg-red-900/30"
                                                    onClick={() => req.id && handleReview(req.id, false)}
                                                    disabled={reviewingId === req.id}
                                                >
                                                    <X className="h-4 w-4 mr-1" />
                                                    Từ chối
                                                </Button>
                                            </div>
                                        </div>
                                    ))}
                                </div>
                            )}
                        </div>
                    </ScrollArea>
                </div>
            </div>
        </>
    );
}
