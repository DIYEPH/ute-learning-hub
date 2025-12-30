"use client";

import { useEffect, useState, useCallback } from "react";
import Link from "next/link";
import { ChevronRight, Loader2, FileText } from "lucide-react";
import { getApiDocumentHomepage, getApiDocumentReadingHistory } from "@/src/api";
import { DocumentCard } from "@/src/components/documents/document-card";
import { ScrollArea, ScrollBar } from "@/src/components/ui/scroll-area";
import type { HomepageDto, ReadingHistoryItemDto, DocumentDto } from "@/src/api/database/types.gen";
import { useTranslations } from "next-intl";
import { useAuthState } from "@/src/hooks/use-auth-state";

interface PagedResponse<T> {
    items?: T[];
    totalCount?: number;
}

export function HomePageSections() {
    const t = useTranslations("home");
    const { authenticated: isAuthenticated } = useAuthState();
    const [homepageData, setHomepageData] = useState<HomepageDto | null>(null);
    const [recentDocs, setRecentDocs] = useState<ReadingHistoryItemDto[]>([]);
    const [loading, setLoading] = useState(true);

    const loadData = useCallback(async () => {
        setLoading(true);
        try {
            const homepageRes = await getApiDocumentHomepage();
            const homepage = (homepageRes as any)?.data || homepageRes as HomepageDto;
            setHomepageData(homepage);

            if (isAuthenticated) {
                try {
                    const historyRes = await getApiDocumentReadingHistory({ query: { PageSize: 10 } });
                    const historyData = (historyRes as unknown as { data: PagedResponse<ReadingHistoryItemDto> })?.data || historyRes as PagedResponse<ReadingHistoryItemDto>;
                    // Filter distinct documents by documentId for homepage
                    const seen = new Set<string>();
                    const distinctDocs = (historyData.items || []).filter(item => {
                        if (seen.has(item.documentId!)) return false;
                        seen.add(item.documentId!);
                        return true;
                    });
                    setRecentDocs(distinctDocs);
                } catch {
                    setRecentDocs([]);
                }
            } else {
                setRecentDocs([]);
            }
        } catch (err) {
            console.error("Error loading homepage data:", err);
        } finally {
            setLoading(false);
        }
    }, [isAuthenticated]);

    useEffect(() => {
        loadData();
    }, [loadData]);

    if (loading) {
        return (
            <div className="flex items-center justify-center py-12">
                <Loader2 className="h-6 w-6 animate-spin text-primary" />
            </div>
        );
    }

    const latestDocs = homepageData?.latestDocuments || [];
    const mostViewedDocs = homepageData?.mostViewedDocuments || [];

    return (
        <div className="space-y-10">
            {/* 1. Tài liệu mới nhất */}
            {latestDocs.length > 0 && (
                <Section title={t("latestDocuments")} href="/search?sort=date">
                    <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
                        {latestDocs.slice(0, 4).map((doc: DocumentDto) => (
                            <DocumentCard
                                key={doc.id}
                                id={doc.id}
                                title={doc.documentName || ""}
                                subjectName={doc.subject?.subjectName || undefined}
                                thumbnailFileId={doc.thumbnailFileId}
                                tags={doc.tags?.map((t) => t.tagName || "").filter(Boolean)}
                                fileCount={doc.fileCount}
                                usefulCount={doc.usefulCount}
                                notUsefulCount={doc.notUsefulCount}
                                totalViewCount={doc.totalViewCount}
                            />
                        ))}
                    </div>
                </Section>
            )}

            {/* 2. Tài liệu phổ biến (theo lượt xem) */}
            {mostViewedDocs.length > 0 && (
                <Section title="Tài liệu phổ biến" href="/search?sort=popular">
                    <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
                        {(mostViewedDocs as DocumentDto[]).slice(0, 4).map((doc: DocumentDto) => (
                            <DocumentCard
                                key={doc.id}
                                id={doc.id}
                                title={doc.documentName || ""}
                                subjectName={doc.subject?.subjectName || undefined}
                                thumbnailFileId={doc.thumbnailFileId}
                                tags={doc.tags?.map((t) => t.tagName || "").filter(Boolean)}
                                fileCount={doc.fileCount}
                                usefulCount={doc.usefulCount}
                                notUsefulCount={doc.notUsefulCount}
                                totalViewCount={doc.totalViewCount}
                            />
                        ))}
                    </div>
                </Section>
            )}

            {/* 3. Đọc gần đây - cuối cùng */}
            {recentDocs.length > 0 && (
                <Section title={t("recentlyViewed")} href="/recent">
                    <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
                        {recentDocs.slice(0, 4).map((item: ReadingHistoryItemDto) => (
                            <DocumentCard
                                key={item.documentFileId || item.documentId}
                                id={item.documentId}
                                title={item.documentName || ""}
                                subjectName={item.subjectName || undefined}
                                thumbnailFileId={item.coverFileId}
                                usefulCount={item.usefulCount}
                                notUsefulCount={item.notUsefulCount}
                                totalPages={item.totalPages}
                                totalViewCount={item.totalViewCount}
                            />
                        ))}
                    </div>
                </Section>
            )}

            {/* Empty state */}
            {latestDocs.length === 0 && mostViewedDocs.length === 0 && (
                <div className="text-center py-12 text-muted-foreground">
                    <FileText className="h-12 w-12 mx-auto mb-3 opacity-40" />
                    <p>{t("noDocuments")}</p>
                </div>
            )}
        </div>
    );
}

function Section({ title, href, children }: { title: string; href?: string; children: React.ReactNode }) {
    const t = useTranslations("home");
    return (
        <section>
            <div className="flex items-center justify-between mb-4">
                <h2 className="text-lg font-semibold text-foreground">{title}</h2>
                {href && (
                    <Link href={href} className="flex items-center gap-1 text-sm text-primary hover:underline font-medium">
                        {t("viewMore")}
                        <ChevronRight className="h-4 w-4" />
                    </Link>
                )}
            </div>
            {children}
        </section>
    );
}
