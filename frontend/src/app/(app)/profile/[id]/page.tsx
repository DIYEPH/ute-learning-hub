"use client";

import { useEffect, useState } from "react";
import { useParams, useRouter } from "next/navigation";
import { ArrowLeft, Loader2, MessageCircle } from "lucide-react";

import {
    getApiAccountProfileByUserId,
    getApiDocument,
    postApiConversationDmByUserId,
} from "@/src/api/database/sdk.gen";

import type { ProfileDto, DocumentDto, GetOrCreateDmResponse } from "@/src/api/database/types.gen";

import { Avatar, AvatarFallback, AvatarImage } from "@/src/components/ui/avatar";
import { Badge } from "@/src/components/ui/badge";
import { Button } from "@/src/components/ui/button";
import { DocumentCard } from "@/src/components/documents/document-card";
import { useUserProfile } from "@/src/hooks/use-user-profile";
import { useNotification } from "@/src/components/providers/notification-provider";

const PAGE_SIZE = 12;

export default function UserProfilePage() {
    const { id: userId } = useParams<{ id: string }>();
    const router = useRouter();
    const { profile: me } = useUserProfile();
    const { success, error } = useNotification();

    const [profile, setProfile] = useState<ProfileDto | null>(null);
    const [docs, setDocs] = useState<DocumentDto[]>([]);
    const [page, setPage] = useState(1);
    const [hasMore, setHasMore] = useState(false);
    const [total, setTotal] = useState(0);
    const [loading, setLoading] = useState(true);
    const [loadingMore, setLoadingMore] = useState(false);
    const [startingDm, setStartingDm] = useState(false);
    const isOwn = me?.id === userId;

    useEffect(() => {
        if (!userId) return;
        void load(1, false);
    }, [userId]);

    const load = async (p: number, append: boolean) => {
        append ? setLoadingMore(true) : setLoading(true);
        try {
            const [pr, dr] = await Promise.all([
                getApiAccountProfileByUserId({ path: { userId } }),
                getApiDocument({
                    query: { CreatedById: userId, Page: p, PageSize: PAGE_SIZE, SortDescending: true },
                }),
            ]);

            const profileData = (pr as any)?.data ?? pr;
            const docsData = (dr as any)?.data ?? dr;

            setProfile(profileData);
            setDocs(prev => append ? [...prev, ...(docsData.items ?? [])] : (docsData.items ?? []));
            setHasMore(Boolean(docsData.hasNextPage));
            setTotal(docsData.totalCount ?? 0);
            setPage(p);
        } catch {
            error("Không thể tải thông tin người dùng");
        } finally {
            append ? setLoadingMore(false) : setLoading(false);
        }
    };

    const startDm = async () => {
        if (startingDm || isOwn) return;
        setStartingDm(true);
        try {
            const res = await postApiConversationDmByUserId({
                path: { userId },
                body: { message: "Xin chào! 👋" },
            });
            const data = (res.data ?? res) as GetOrCreateDmResponse;
            if (data.conversation?.id) {
                success("Đã bắt đầu trò chuyện");
                router.push(`/chat?conversationId=${data.conversation.id}`);
            }
        } catch (e: any) {
            error(e?.response?.data?.message || "Không thể nhắn tin");
        } finally {
            setStartingDm(false);
        }
    };

    if (loading) {
        return (
            <div className="flex h-[50vh] items-center justify-center">
                <Loader2 className="animate-spin text-sky-500" />
            </div>
        );
    }

    if (!profile) {
        return (
            <div className="space-y-4">
                <Button variant="ghost" size="sm" onClick={() => router.back()}>
                    <ArrowLeft className="h-4 w-4 mr-2" /> Quay lại
                </Button>
                <div className="p-4 text-sm text-red-600 bg-red-50 rounded">
                    Không tìm thấy người dùng
                </div>
            </div>
        );
    }

    const initials =
        (profile.fullName || profile.username || "?")
            .split(" ")
            .map(s => s[0])
            .join("")
            .slice(0, 2)
            .toUpperCase();

    return (
        <div className="space-y-6">

            <Button variant="ghost" size="sm" onClick={() => router.back()}>
                <ArrowLeft className="h-4 w-4 mr-2" /> Quay lại
            </Button>

            <div className="border bg-white p-6 dark:bg-slate-900">
                <div className="flex gap-4">
                    <Avatar className="h-20 w-20">
                        <AvatarImage src={profile.avatarUrl || undefined} />
                        <AvatarFallback className="text-xl">{initials}</AvatarFallback>
                    </Avatar>

                    <div className="flex-1 min-w-0">
                        <h1 className="text-2xl font-semibold truncate">
                            {profile.fullName || profile.username}
                        </h1>
                        {profile.username && (
                            <p className="text-sm text-slate-500">@{profile.username}</p>
                        )}

                        <div className="flex flex-wrap items-center gap-2 mt-3">
                            {profile.trustLevel && (
                                <Badge variant="outline">{profile.trustLevel}</Badge>
                            )}
                            {profile.major && (
                                <Badge variant="secondary">
                                    {(profile.major as any)?.majorName}
                                </Badge>
                            )}

                            {!isOwn && (
                                <Button
                                    size="sm"
                                    variant="outline"
                                    onClick={startDm}
                                    disabled={startingDm}
                                    className="ml-auto"
                                >
                                    {startingDm
                                        ? <Loader2 className="h-4 w-4 animate-spin mr-1" />
                                        : <MessageCircle className="h-4 w-4 mr-1" />
                                    }
                                    Nhắn tin
                                </Button>
                            )}
                        </div>
                    </div>
                </div>

                {profile.introduction && (
                    <p className="mt-4 pt-4 border-t text-sm whitespace-pre-wrap">
                        {profile.introduction}
                    </p>
                )}
            </div>

            <h2 className="text-lg font-semibold">
                Tài liệu đã đăng ({total})
            </h2>

            {docs.length === 0 ? (
                <div className="p-8 text-center text-slate-500 border bg-white">
                    Chưa có tài liệu
                </div>
            ) : (
                <div className="grid gap-3 grid-cols-2 sm:grid-cols-3 lg:grid-cols-4 xl:grid-cols-5">
                    {docs.map(d => (
                        <DocumentCard
                            key={d.id}
                            id={d.id}
                            title={d.documentName || ""}
                            subjectName={d.subject?.subjectName}
                            thumbnailFileId={d.thumbnailFileId}
                            tags={d.tags?.map(t => t.tagName || "").filter(Boolean)}
                            fileCount={d.fileCount}
                            commentCount={d.commentCount}
                            usefulCount={d.usefulCount}
                            notUsefulCount={d.notUsefulCount}
                            totalViewCount={d.totalViewCount}
                        />
                    ))}
                </div>
            )}

            {hasMore && (
                <div className="flex justify-center">
                    <Button
                        variant="outline"
                        onClick={() => load(page + 1, true)}
                        disabled={loadingMore}
                    >
                        {loadingMore && <Loader2 className="h-4 w-4 animate-spin mr-2" />}
                        Tải thêm
                    </Button>
                </div>
            )}

        </div>
    );
}
