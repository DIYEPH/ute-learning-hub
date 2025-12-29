"use client";

import { useEffect, useState, useCallback } from "react";
import Link from "next/link";
import { ChevronRight, Loader2, FileText } from "lucide-react";
import { getApiDocumentHomepage, getApiDocumentReadingHistory } from "@/src/api/database/sdk.gen";
import { DocumentCard } from "@/src/components/documents/document-card";
import { ScrollArea, ScrollBar } from "@/src/components/ui/scroll-area";
import type { HomepageDto, ReadingHistoryItemDto, DocumentDto, SubjectWithDocsDto } from "@/src/api/database/types.gen";
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
                    const historyRes = await getApiDocumentReadingHistory({ query: { PageSize: 8 } });
                    const historyData = (historyRes as unknown as { data: PagedResponse<ReadingHistoryItemDto> })?.data || historyRes as PagedResponse<ReadingHistoryItemDto>;
                    setRecentDocs(historyData.items || []);
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
    const popularDocs = homepageData?.popularDocuments || [];
    const mostViewedDocs = homepageData?.mostViewedDocuments || [];
    const topSubjects = homepageData?.topSubjects || [];

    return (
        <div className="space-y-6">
            {/* Đọc gần đây */}
            {recentDocs.length > 0 && (
                <Section title={t("recentlyViewed")} href="/recent">
                    <HorizontalScroll>
                        {recentDocs.map((item: ReadingHistoryItemDto) => (
                            <DocumentCard
                                key={item.documentFileId || item.documentId}
                                id={item.documentId}
                                title={item.documentName || ""}
                                subjectName={item.subjectName || undefined}
                                thumbnailFileId={item.coverFileId}
                                totalViewCount={item.totalViewCount}
                                usefulCount={item.usefulCount}
                                notUsefulCount={item.notUsefulCount}
                                lastPage={item.lastPage}
                                totalPages={item.totalPages}
                                className="w-40 flex-shrink-0"
                            />
                        ))}
                    </HorizontalScroll>
                </Section>
            )}

            {/* Được yêu thích */}
            {popularDocs.length > 0 && (
                <Section title="Được yêu thích" href="/search">
                    <HorizontalScroll>
                        {(popularDocs as DocumentDto[]).map((doc: DocumentDto) => (
                            <DocumentCard
                                key={doc.id}
                                id={doc.id}
                                title={doc.documentName || ""}
                                subjectName={doc.subject?.subjectName || undefined}
                                thumbnailFileId={doc.thumbnailFileId}
                                tags={doc.tags?.map((tag) => tag.tagName || "").filter(Boolean)}
                                fileCount={doc.fileCount}
                                usefulCount={doc.usefulCount}
                                totalViewCount={doc.totalViewCount}
                                className="w-40 flex-shrink-0"
                            />
                        ))}
                    </HorizontalScroll>
                </Section>
            )}

            {/* Xem nhiều nhất */}
            {mostViewedDocs.length > 0 && (
                <Section title="Xem nhiều nhất" href="/search">
                    <HorizontalScroll>
                        {(mostViewedDocs as DocumentDto[]).map((doc: DocumentDto) => (
                            <DocumentCard
                                key={doc.id}
                                id={doc.id}
                                title={doc.documentName || ""}
                                subjectName={doc.subject?.subjectName || undefined}
                                thumbnailFileId={doc.thumbnailFileId}
                                tags={doc.tags?.map((tag) => tag.tagName || "").filter(Boolean)}
                                fileCount={doc.fileCount}
                                usefulCount={doc.usefulCount}
                                notUsefulCount={doc.notUsefulCount}
                                totalViewCount={doc.totalViewCount}
                                className="w-40 flex-shrink-0"
                            />
                        ))}
                    </HorizontalScroll>
                </Section>
            )}

            {/* Mới nhất */}
            {latestDocs.length > 0 && (
                <Section title={t("latestDocuments")} href="/search">
                    <HorizontalScroll>
                        {latestDocs.map((doc: DocumentDto) => (
                            <DocumentCard
                                key={doc.id}
                                id={doc.id}
                                title={doc.documentName || ""}
                                subjectName={doc.subject?.subjectName || undefined}
                                thumbnailFileId={doc.thumbnailFileId}
                                tags={doc.tags?.map((tag) => tag.tagName || "").filter(Boolean)}
                                fileCount={doc.fileCount}
                                usefulCount={doc.usefulCount}
                                totalViewCount={doc.totalViewCount}
                                notUsefulCount={doc.notUsefulCount}
                                className="w-40 flex-shrink-0"
                            />
                        ))}
                    </HorizontalScroll>
                </Section>
            )}

            {/* Theo môn học */}
            {topSubjects.map((item: SubjectWithDocsDto) => (
                <Section
                    key={item.subjectId}
                    title={item.subjectName || ""}
                    href={`/search?subject=${item.subjectId}`}
                >
                    <HorizontalScroll>
                        {((item.documents || []) as DocumentDto[]).map((doc: DocumentDto) => (
                            <DocumentCard
                                key={doc.id}
                                id={doc.id}
                                title={doc.documentName || ""}
                                subjectName={doc.subject?.subjectName || undefined}
                                thumbnailFileId={doc.thumbnailFileId}
                                tags={doc.tags?.map((tag) => tag.tagName || "").filter(Boolean)}
                                fileCount={doc.fileCount}
                                usefulCount={doc.usefulCount}
                                notUsefulCount={doc.notUsefulCount}
                                totalViewCount={doc.totalViewCount}
                                className="w-40 flex-shrink-0"
                            />
                        ))}
                    </HorizontalScroll>
                </Section>
            ))}

            {/* Empty state */}
            {topSubjects.length === 0 && latestDocs.length === 0 && popularDocs.length === 0 && mostViewedDocs.length === 0 && (
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
            <div className="flex items-center justify-between mb-3">
                <h2 className="text-base font-semibold text-foreground">{title}</h2>
                {href && (
                    <Link href={href} className="flex items-center gap-1 text-xs text-primary hover:underline">
                        {t("viewMore")}
                        <ChevronRight className="h-3 w-3" />
                    </Link>
                )}
            </div>
            {children}
        </section>
    );
}

function HorizontalScroll({ children }: { children: React.ReactNode }) {
    return (
        <ScrollArea className="w-full whitespace-nowrap">
            <div className="flex gap-3 pb-2">{children}</div>
            <ScrollBar orientation="horizontal" />
        </ScrollArea>
    );
}
