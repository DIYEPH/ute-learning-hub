"use client";

import { useEffect, useState } from "react";
import { useParams, useRouter } from "next/navigation";
import { Loader2, ArrowLeft } from "lucide-react";
import { getApiAccountProfileByUserId, getApiDocument } from "@/src/api/database/sdk.gen";
import type { ProfileDto, DocumentDto } from "@/src/api/database/types.gen";
import { Avatar, AvatarFallback, AvatarImage } from "@/src/components/ui/avatar";
import { Badge } from "@/src/components/ui/badge";
import { Button } from "@/src/components/ui/button";
import { DocumentCard } from "@/src/components/documents/document-card";

const PAGE_SIZE = 12;

export default function UserProfilePage() {
    const params = useParams();
    const router = useRouter();
    const userId = params.id as string;

    const [profile, setProfile] = useState<ProfileDto | null>(null);
    const [documents, setDocuments] = useState<DocumentDto[]>([]);
    const [loading, setLoading] = useState(true);
    const [loadingMore, setLoadingMore] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [page, setPage] = useState(1);
    const [hasMore, setHasMore] = useState(false);
    const [totalDocs, setTotalDocs] = useState(0);

    useEffect(() => {
        if (!userId) return;
        void loadData();
    }, [userId]);

    const loadData = async () => {
        try {
            setLoading(true);
            const [profileRes, docsRes] = await Promise.all([
                getApiAccountProfileByUserId({ path: { userId } }),
                getApiDocument({
                    query: {
                        CreatedById: userId,
                        Page: 1,
                        PageSize: PAGE_SIZE,
                        SortDescending: true,
                    },
                }),
            ]);

            const profileData = (profileRes as any)?.data || profileRes;
            setProfile(profileData as ProfileDto);

            const docsData = (docsRes as any)?.data || docsRes;
            setDocuments(docsData.items || []);
            setTotalDocs(docsData.totalCount || 0);
            setHasMore(!!docsData.hasNextPage);
            setPage(1);
        } catch (err: any) {
            setError(err?.message || "Không thể tải thông tin người dùng");
        } finally {
            setLoading(false);
        }
    };

    const loadMore = async () => {
        if (loadingMore || !hasMore) return;
        try {
            setLoadingMore(true);
            const nextPage = page + 1;
            const res = await getApiDocument({
                query: {
                    CreatedById: userId,
                    Page: nextPage,
                    PageSize: PAGE_SIZE,
                    SortDescending: true,
                },
            });
            const data = (res as any)?.data || res;
            setDocuments((prev) => [...prev, ...(data.items || [])]);
            setHasMore(!!data.hasNextPage);
            setPage(nextPage);
        } catch (err) {
            console.error("Error loading more documents:", err);
        } finally {
            setLoadingMore(false);
        }
    };

    if (loading) {
        return (
            <div className="flex h-[50vh] items-center justify-center">
                <Loader2 className="h-6 w-6 animate-spin text-sky-500" />
            </div>
        );
    }

    if (error || !profile) {
        return (
            <div className="space-y-4">
                <Button variant="ghost" size="sm" onClick={() => router.back()}>
                    <ArrowLeft className="h-4 w-4 mr-2" />
                    Quay lại
                </Button>
                <div className="border border-red-200 bg-red-50 p-4 text-sm text-red-600 dark:border-red-900 dark:bg-red-950 dark:text-red-300">
                    {error || "Không tìm thấy người dùng"}
                </div>
            </div>
        );
    }

    const initials = (profile.fullName || profile.username || "?")
        .split(" ")
        .map((p) => p[0])
        .join("")
        .slice(0, 2)
        .toUpperCase();

    return (
        <div className="space-y-6">
            <Button variant="ghost" size="sm" onClick={() => router.back()}>
                <ArrowLeft className="h-4 w-4 mr-2" />
                Quay lại
            </Button>

            {/* Profile Header */}
            <div className="border border-slate-200 bg-white p-6 dark:border-slate-700 dark:bg-slate-900">
                <div className="flex items-start gap-4">
                    <Avatar className="h-20 w-20">
                        <AvatarImage src={profile.avatarUrl || undefined} alt={profile.fullName || ""} />
                        <AvatarFallback className="text-xl">{initials}</AvatarFallback>
                    </Avatar>

                    <div className="flex-1 min-w-0">
                        <h1 className="text-2xl font-semibold text-foreground truncate">
                            {profile.fullName || profile.username || "Người dùng"}
                        </h1>

                        {profile.username && (
                            <p className="text-sm text-slate-500 dark:text-slate-400">
                                @{profile.username}
                            </p>
                        )}

                        <div className="flex flex-wrap gap-2 mt-3">
                            {profile.trustLevel && (
                                <Badge variant="outline" className="border-amber-200 text-amber-700">
                                    {profile.trustLevel}
                                </Badge>
                            )}
                            {profile.major && (
                                <Badge variant="secondary">
                                    {(profile.major as any)?.majorName || "Ngành học"}
                                </Badge>
                            )}
                        </div>
                    </div>
                </div>

                {profile.introduction && (
                    <div className="mt-4 pt-4 border-t border-slate-200 dark:border-slate-700">
                        <p className="text-sm text-slate-600 dark:text-slate-300 whitespace-pre-wrap">
                            {profile.introduction}
                        </p>
                    </div>
                )}
            </div>

            {/* Documents Section */}
            <div className="space-y-4">
                <h2 className="text-lg font-semibold text-foreground">
                    Tài liệu đã đăng ({totalDocs})
                </h2>

                {documents.length === 0 ? (
                    <div className="text-center py-8 text-slate-500 dark:text-slate-400 border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-900">
                        Người dùng chưa đăng tài liệu nào
                    </div>
                ) : (
                    <div className="grid gap-3 grid-cols-2 sm:grid-cols-3 lg:grid-cols-4 xl:grid-cols-5">
                        {documents.map((doc) => (
                            <DocumentCard
                                key={doc.id}
                                id={doc.id}
                                title={doc.documentName || ""}
                                subjectName={doc.subject?.subjectName || undefined}
                                thumbnailFileId={doc.thumbnailFileId}
                                tags={doc.tags?.map((t) => t.tagName || "").filter(Boolean)}
                                fileCount={doc.fileCount}
                                commentCount={doc.commentCount}
                                usefulCount={doc.usefulCount}
                                notUsefulCount={doc.notUsefulCount}
                            />
                        ))}
                    </div>
                )}

                {hasMore && (
                    <div className="flex justify-center pt-2">
                        <Button variant="outline" onClick={loadMore} disabled={loadingMore}>
                            {loadingMore && <Loader2 className="h-4 w-4 mr-2 animate-spin" />}
                            Tải thêm
                        </Button>
                    </div>
                )}
            </div>
        </div>
    );
}
