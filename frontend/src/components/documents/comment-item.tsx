"use client";

import { useState } from "react";
import Link from "next/link";
import { MoreVertical, Flag, ChevronDown, ChevronUp, CornerDownRight } from "lucide-react";
import { useNotification } from "@/src/components/providers/notification-provider";
import { DropdownMenu, DropdownMenuContent, DropdownMenuItem, DropdownMenuTrigger } from "@/src/components/ui/dropdown-menu";
import { Avatar, AvatarImage, AvatarFallback } from "@/src/components/ui/avatar";
import { getProfileLink } from "@/src/lib/profile-utils";
import { getRelativeTime } from "@/src/lib/date-utils";
import { getApiComment } from "@/src/api";
import type { CommentDetailDto, PagedResponseOfCommentDetailDto } from "@/src/api/database/types.gen";

interface CommentItemProps {
    comment: CommentDetailDto;
    isOwner: boolean;
    authenticated: boolean;
    onReport: (c: CommentDetailDto) => void;
    onReply: (parentId: string) => void;
    documentFileId: string;
    profileId?: string;
}

export function CommentItem({ comment, isOwner, authenticated, onReport, onReply, documentFileId, profileId }: CommentItemProps) {
    const [showReplies, setShowReplies] = useState(false);
    const [replies, setReplies] = useState<CommentDetailDto[]>([]);
    const [loadingReplies, setLoadingReplies] = useState(false);
    const { error: notifyError } = useNotification();

    // Load replies
    const loadReplies = async () => {
        if (!comment.id || loadingReplies) return;
        setLoadingReplies(true);
        try {
            const res = await getApiComment<true>({
                query: { DocumentFileId: documentFileId, ParentId: comment.id, Page: 1, PageSize: 50 },
                throwOnError: true
            });
            const data = (res.data ?? res) as PagedResponseOfCommentDetailDto;
            setReplies(data.items ?? []);
        } catch { notifyError("Không thể tải phản hồi"); }
        finally { setLoadingReplies(false); }
    };

    const toggleReplies = async () => {
        if (!showReplies && replies.length === 0) await loadReplies();
        setShowReplies(!showReplies);
    };

    const replyCount = comment.replyCount ?? 0;
    const createdAt = comment.createdAt ? new Date(comment.createdAt) : new Date();

    return (
        <div className="group">
            <div className="flex gap-2">
                <Link href={getProfileLink(comment.createdById, profileId)} className="shrink-0">
                    <Avatar className="h-8 w-8">
                        <AvatarImage src={comment.authorAvatarUrl || undefined} alt={comment.authorName} />
                        <AvatarFallback className="bg-primary/10 text-primary text-xs">
                            {comment.authorName?.charAt(0)?.toUpperCase() || "?"}
                        </AvatarFallback>
                    </Avatar>
                </Link>
                <div className="flex-1 min-w-0">
                    <div className="inline-block bg-muted rounded-2xl px-3 py-2 max-w-full">
                        <Link
                            href={getProfileLink(comment.createdById, profileId)}
                            className="text-[13px] font-semibold text-foreground hover:underline"
                        >
                            {comment.authorName}
                        </Link>
                        <p className="text-[13px] text-foreground whitespace-pre-wrap wrap-break-word mt-0.5">
                            {comment.content}
                        </p>
                    </div>
                    <div className="flex items-center gap-4 mt-1 px-2">
                        <span className="text-[11px] text-muted-foreground">{getRelativeTime(createdAt)}</span>
                        {authenticated && (
                            <button
                                type="button"
                                onClick={() => comment.id && onReply(comment.id)}
                                className="text-[11px] font-semibold text-muted-foreground hover:text-foreground hover:underline"
                            >
                                Trả lời
                            </button>
                        )}
                        {authenticated && !isOwner && (
                            <DropdownMenu>
                                <DropdownMenuTrigger asChild>
                                    <button className="text-muted-foreground hover:text-foreground opacity-0 group-hover:opacity-100 transition-opacity">
                                        <MoreVertical className="h-3.5 w-3.5" />
                                    </button>
                                </DropdownMenuTrigger>
                                <DropdownMenuContent align="start" className="min-w-[120px]">
                                    <DropdownMenuItem onClick={() => onReport(comment)} className="text-red-600 focus:text-red-600">
                                        <Flag className="h-3.5 w-3.5 mr-2" />Báo cáo
                                    </DropdownMenuItem>
                                </DropdownMenuContent>
                            </DropdownMenu>
                        )}
                    </div>
                    {replyCount > 0 && (
                        <button
                            type="button"
                            onClick={toggleReplies}
                            className="flex items-center gap-1.5 mt-1.5 px-2 text-[12px] font-semibold text-primary hover:underline"
                        >
                            <CornerDownRight className="h-3 w-3" />
                            {showReplies ? (
                                <><ChevronUp className="h-3 w-3" />Ẩn phản hồi</>
                            ) : (
                                <>Xem {replyCount} phản hồi<ChevronDown className="h-3 w-3" /></>
                            )}
                        </button>
                    )}
                    {showReplies && (
                        <div className="mt-2 ml-2 pl-3 border-l-2 border-border/50 space-y-2">
                            {loadingReplies ? (
                                <p className="text-xs text-muted-foreground">Đang tải...</p>
                            ) : replies.map(reply => (
                                <div key={reply.id} className="flex gap-2 group">
                                    <Link href={getProfileLink(reply.createdById, profileId)} className="shrink-0">
                                        <Avatar className="h-6 w-6">
                                            <AvatarImage src={reply.authorAvatarUrl || undefined} alt={reply.authorName} />
                                            <AvatarFallback className="bg-primary/10 text-primary text-[10px]">
                                                {reply.authorName?.charAt(0)?.toUpperCase() || "?"}
                                            </AvatarFallback>
                                        </Avatar>
                                    </Link>
                                    <div className="flex-1 min-w-0">
                                        <div className="inline-block bg-muted rounded-2xl px-3 py-1.5 max-w-full">
                                            <Link
                                                href={getProfileLink(reply.createdById, profileId)}
                                                className="text-[12px] font-semibold text-foreground hover:underline"
                                            >
                                                {reply.authorName}
                                            </Link>
                                            <p className="text-[12px] text-foreground whitespace-pre-wrap wrap-break-word">
                                                {reply.content}
                                            </p>
                                        </div>
                                        <div className="flex items-center gap-3 mt-0.5 px-2">
                                            <span className="text-[10px] text-muted-foreground">
                                                {getRelativeTime(new Date(reply.createdAt as string))}
                                            </span>
                                        </div>
                                    </div>
                                </div>
                            ))}
                        </div>
                    )}
                </div>
            </div>
        </div>
    );
}