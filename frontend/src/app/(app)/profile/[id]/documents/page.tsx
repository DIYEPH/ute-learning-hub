"use client";

import { useEffect, useState } from "react";
import { useParams, useRouter } from "next/navigation";
import Link from "next/link";
import { ArrowLeft, Loader2, FileText, User } from "lucide-react";

import {
    getApiDocument,
    getApiAccountProfileByUserId,
} from "@/src/api";

import type { DocumentDto, ProfileDetailDto } from "@/src/api/database/types.gen";
import { DocumentCard } from "@/src/components/documents/document-card";
import { Button } from "@/src/components/ui/button";
import { Avatar, AvatarFallback, AvatarImage } from "@/src/components/ui/avatar";

const PAGE_SIZE = 24;

export default function UserDocumentsPage() {
    const { id: userId } = useParams<{ id: string }>();
    const router = useRouter();

    const [profile, setProfile] = useState<ProfileDetailDto | null>(null);
    const [docs, setDocs] = useState<DocumentDto[]>([]);
    const [page, setPage] = useState(1);
    const [hasMore, setHasMore] = useState(false);
    const [total, setTotal] = useState(0);
    const [loading, setLoading] = useState(true);
    const [loadingMore, setLoadingMore] = useState(false);

    useEffect(() => {
        if (!userId) return;
        (async () => {
            setLoading(true);
            const [p, d] = await Promise.all([
                getApiAccountProfileByUserId({ path: { userId } }),
                getApiDocument({
                    query: { AuthorId: userId, Page: 1, PageSize: PAGE_SIZE, SortDescending: true },
                }),
            ]);
            const profileData = (p as any)?.data ?? p;
            const docsData = (d as any)?.data ?? d;
            setProfile(profileData);
            setDocs(docsData.items ?? []);
            setTotal(docsData.totalCount ?? 0);
            setHasMore(!!docsData.hasNextPage);
            setPage(1);
            setLoading(false);
        })();
    }, [userId]);

    const loadMore = async () => {
        if (!hasMore || loadingMore) return;
        setLoadingMore(true);
        const next = page + 1;
        const res = await getApiDocument({
            query: { AuthorId: userId, Page: next, PageSize: PAGE_SIZE, SortDescending: true },
        });
        const data = (res as any)?.data ?? res;
        setDocs(v => [...v, ...(data.items ?? [])]);
        setHasMore(!!data.hasNextPage);
        setPage(next);
        setLoadingMore(false);
    };

    if (loading)
        return (
            <div className="flex h-[50vh] items-center justify-center">
                <Loader2 className="h-6 w-6 animate-spin text-primary" />
            </div>
        );

    const initials =
        (profile?.fullName || profile?.username || "?")
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

            <div className="flex items-center gap-4 border p-4 bg-card">
                <Link href={`/profile/${userId}`}>
                    <Avatar className="h-12 w-12">
                        <AvatarImage src={profile?.avatarUrl || undefined} />
                        <AvatarFallback>{initials}</AvatarFallback>
                    </Avatar>
                </Link>
                <div className="flex-1 min-w-0">
                    <Link href={`/profile/${userId}`} className="hover:underline">
                        <h1 className="text-lg font-semibold truncate">
                            {profile?.fullName || profile?.username}
                        </h1>
                    </Link>
                    <p className="text-sm text-muted-foreground">
                        <FileText className="inline h-4 w-4 mr-1" />
                        {total} tài liệu
                    </p>
                </div>
                <Link href={`/profile/${userId}`}>
                    <Button variant="outline" size="sm">
                        <User className="h-4 w-4 mr-2" /> Hồ sơ
                    </Button>
                </Link>
            </div>

            {docs.length === 0 ? (
                <div className="text-center py-12 text-muted-foreground">
                    Người dùng chưa có tài liệu
                </div>
            ) : (
                <div className="grid gap-4 grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
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
                    <Button variant="outline" onClick={loadMore} disabled={loadingMore}>
                        {loadingMore && <Loader2 className="h-4 w-4 mr-2 animate-spin" />}
                        Tải thêm
                    </Button>
                </div>
            )}

        </div>
    );
}
