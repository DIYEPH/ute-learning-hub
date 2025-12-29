"use client";

import { useState, useEffect } from "react";
import { Loader2, UserPlus, Check, X, Search, Sparkles } from "lucide-react";
import {
    getApiConversationByIdSuggestedUsers,
    postApiConversationByIdInvitations,
} from "@/src/api/database/sdk.gen";
import type { SuggestedUserDto, GetSuggestedUsersResponse } from "@/src/api/database/types.gen";
import { Button } from "@/src/components/ui/button";
import { Input } from "@/src/components/ui/input";
import { Avatar, AvatarFallback, AvatarImage } from "@/src/components/ui/avatar";
import { Sheet, SheetContent, SheetHeader, SheetTitle } from "@/src/components/ui/sheet";
import { useNotification } from "@/src/components/providers/notification-provider";

interface SuggestedUsersSidebarProps {
    open: boolean;
    onClose: () => void;
    conversationId: string;
    conversationName: string;
    onInviteSent?: () => void;
}

export function SuggestedUsersSidebar({
    open,
    onClose,
    conversationId,
    conversationName,
    onInviteSent,
}: SuggestedUsersSidebarProps) {
    const { success: notifySuccess, error: notifyError } = useNotification();
    const [users, setUsers] = useState<SuggestedUserDto[]>([]);
    const [loading, setLoading] = useState(false);
    const [invitingUserId, setInvitingUserId] = useState<string | null>(null);
    const [searchTerm, setSearchTerm] = useState("");
    const [invitedUserIds, setInvitedUserIds] = useState<Set<string>>(new Set());

    useEffect(() => {
        if (open) {
            void loadSuggestedUsers();
        }
    }, [open, conversationId]);

    const loadSuggestedUsers = async () => {
        setLoading(true);
        try {
            const res = await getApiConversationByIdSuggestedUsers({
                path: { id: conversationId },
                query: { topK: 20, minScore: 0.1 },
            });
            const data = (res.data ?? res) as GetSuggestedUsersResponse;
            setUsers(data.users || []);
        } catch (err: any) {
            notifyError(err?.message || "Không thể tải danh sách gợi ý");
        } finally {
            setLoading(false);
        }
    };

    const handleInvite = async (user: SuggestedUserDto) => {
        if (!user.userId) return;
        setInvitingUserId(user.userId);
        try {
            await postApiConversationByIdInvitations({
                path: { id: conversationId },
                body: {
                    userId: user.userId,
                    message: `Bạn được mời tham gia nhóm "${conversationName}"`,
                },
                throwOnError: true,
            });
            setInvitedUserIds((prev) => new Set([...prev, user.userId!]));
            notifySuccess(`Đã gửi lời mời đến ${user.fullName || "người dùng"}`);
            onInviteSent?.();
        } catch (err: any) {
            notifyError(err?.response?.data?.message || err?.body?.message || err?.message || "Không thể gửi lời mời");
        } finally {
            setInvitingUserId(null);
        }
    };

    const filteredUsers = searchTerm
        ? users.filter((u) =>
            (u.fullName || "").toLowerCase().includes(searchTerm.toLowerCase())
        )
        : users;

    const getInitials = (name?: string) => {
        if (!name) return "?";
        return name
            .split(" ")
            .map((p) => p[0])
            .join("")
            .slice(0, 2)
            .toUpperCase();
    };

    return (
        <Sheet open={open} onOpenChange={(o) => !o && onClose()}>
            <SheetContent side="right" className="w-[350px] sm:w-[400px] p-0">
                <SheetHeader className="px-4 py-3 border-b border-border">
                    <SheetTitle className="flex items-center gap-2 text-base">
                        <Sparkles className="h-4 w-4 text-amber-500" />
                        Gợi ý thành viên (AI)
                    </SheetTitle>
                </SheetHeader>

                <div className="p-4 space-y-4">
                    {/* Search */}
                    <div className="relative">
                        <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
                        <Input
                            placeholder="Tìm kiếm người dùng..."
                            value={searchTerm}
                            onChange={(e) => setSearchTerm(e.target.value)}
                            className="pl-9"
                        />
                    </div>

                    {/* Users List */}
                    <div className="space-y-2 max-h-[60vh] overflow-y-auto">
                        {loading ? (
                            <div className="flex items-center justify-center py-8">
                                <Loader2 className="h-6 w-6 animate-spin text-primary" />
                            </div>
                        ) : filteredUsers.length === 0 ? (
                            <div className="text-center py-8 text-muted-foreground">
                                <p className="text-sm">
                                    {searchTerm ? "Không tìm thấy người dùng" : "Không có gợi ý nào"}
                                </p>
                                <p className="text-xs mt-1">
                                    AI sẽ gợi ý những người có sở thích tương đồng với nhóm
                                </p>
                            </div>
                        ) : (
                            filteredUsers.map((user) => {
                                const isInvited = user.userId ? invitedUserIds.has(user.userId) : false;
                                const isInviting = invitingUserId === user.userId;
                                const matchPercent = user.similarity ? Math.round(user.similarity * 100) : 0;

                                return (
                                    <div
                                        key={user.userId}
                                        className="flex items-center gap-3 p-3 rounded-lg border border-border bg-card hover:shadow-sm transition-shadow"
                                    >
                                        <Avatar className="h-10 w-10">
                                            <AvatarImage src={user.avatarUrl || undefined} />
                                            <AvatarFallback>{getInitials(user.fullName)}</AvatarFallback>
                                        </Avatar>

                                        <div className="flex-1 min-w-0">
                                            <p className="text-sm font-medium text-foreground truncate">
                                                {user.fullName || "Người dùng"}
                                            </p>
                                            <div className="flex items-center gap-2 text-xs text-muted-foreground">
                                                {matchPercent > 0 && (
                                                    <span className="inline-flex items-center px-1.5 py-0.5 rounded bg-amber-100 text-amber-700 dark:bg-amber-900 dark:text-amber-300 font-medium">
                                                        {matchPercent}% phù hợp
                                                    </span>
                                                )}
                                                {user.rank && (
                                                    <span className="truncate">Hạng #{user.rank}</span>
                                                )}
                                            </div>
                                        </div>

                                        <Button
                                            size="sm"
                                            variant={isInvited ? "ghost" : "outline"}
                                            disabled={isInvited || isInviting}
                                            onClick={() => handleInvite(user)}
                                            className="shrink-0"
                                        >
                                            {isInviting ? (
                                                <Loader2 className="h-4 w-4 animate-spin" />
                                            ) : isInvited ? (
                                                <>
                                                    <Check className="h-4 w-4 mr-1 text-green-500" />
                                                    Đã mời
                                                </>
                                            ) : (
                                                <>
                                                    <UserPlus className="h-4 w-4 mr-1" />
                                                    Mời
                                                </>
                                            )}
                                        </Button>
                                    </div>
                                );
                            })
                        )}
                    </div>

                    {/* Refresh button */}
                    {!loading && users.length > 0 && (
                        <div className="pt-2 border-t border-border">
                            <Button
                                variant="outline"
                                size="sm"
                                onClick={loadSuggestedUsers}
                                className="w-full"
                            >
                                <Sparkles className="h-4 w-4 mr-2" />
                                Tải lại gợi ý
                            </Button>
                        </div>
                    )}
                </div>
            </SheetContent>
        </Sheet>
    );
}
