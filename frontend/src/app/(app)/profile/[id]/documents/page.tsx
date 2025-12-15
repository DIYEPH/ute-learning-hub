"use client";

import { useEffect, useState } from "react";
import { useParams, useRouter } from "next/navigation";
import { Loader2, ArrowLeft, FileText, User } from "lucide-react";
import Link from "next/link";
import { getApiDocument, getApiAccountProfileByUserId } from "@/src/api/database/sdk.gen";
import type { DocumentDto, ProfileDto } from "@/src/api/database/types.gen";
import { DocumentCard } from "@/src/components/documents/document-card";
import { Button } from "@/src/components/ui/button";
import { Avatar, AvatarFallback, AvatarImage } from "@/src/components/ui/avatar";

const PAGE_SIZE = 24;

export default function UserDocumentsPage() {
    const params = useParams();
    const router = useRouter();
    const userId = params.id as string;

    const [profile, setProfile] = useState<ProfileDto | null>(null);
    const [documents, setDocuments] = useState<DocumentDto[]>([]);
    const [loading, setLoading] = useState(true);
    const [loadingMore, setLoadingMore] = useState(false);
    const [page, setPage] = useState(1);
    const [hasMore, setHasMore] = useState(false);
    const [totalCount, setTotalCount] = useState(0);

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
                        AuthorId: userId,
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
            setTotalCount(docsData.totalCount || 0);
            setHasMore(!!docsData.hasNextPage);
            setPage(1);
        } catch (err) {
            console.error("Error loading user documents:", err);
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
                    AuthorId: userId,
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

    const initials = (profile?.fullName || profile?.username || "?")
        .split(" ")
        .map((p) => p[0])
        .join("")
        .slice(0, 2)
        .toUpperCase();

    return (
        <div className="space-y-6">
            {/* Header */}
            <div className="flex items-center gap-4">
                <Button variant="ghost" size="sm" onClick={() => router.back()}>
                    <ArrowLeft className="h-4 w-4 mr-2" />
                    Quay lại
                </Button>
            </div>

            {/* User Info */}
            <div className="flex items-center gap-4 border border-slate-200 bg-white p-4 dark:border-slate-700 dark:bg-slate-900">
                <Link href={`/profile/${userId}`}>
                    <Avatar className="h-12 w-12">
                        <AvatarImage src={profile?.avatarUrl || undefined} />
                        <AvatarFallback>{initials}</AvatarFallback>
                    </Avatar>
                </Link>
                <div className="flex-1 min-w-0">
                    <Link href={`/profile/${userId}`} className="hover:underline">
                        <h1 className="text-lg font-semibold text-foreground truncate">
                            {profile?.fullName || profile?.username || "Người dùng"}
                        </h1>
                    </Link>
                    <p className="text-sm text-slate-500">
                        <FileText className="h-4 w-4 inline mr-1" />
                        {totalCount} tài liệu
                    </p>
                </div>
                <Link href={`/profile/${userId}`}>
                    <Button variant="outline" size="sm">
                        <User className="h-4 w-4 mr-2" />
                        Xem hồ sơ
                    </Button>
                </Link>
            </div>

            {/* Documents Grid */}
            {documents.length === 0 ? (
                <div className="text-center py-12 text-slate-500 dark:text-slate-400">
                    Người dùng chưa có tài liệu nào
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

            {/* Load More */}
            {hasMore && (
                <div className="flex justify-center pt-4">
                    <Button variant="outline" onClick={loadMore} disabled={loadingMore}>
                        {loadingMore && <Loader2 className="h-4 w-4 mr-2 animate-spin" />}
                        Tải thêm
                    </Button>
                </div>
            )}
        </div>
    );
}
