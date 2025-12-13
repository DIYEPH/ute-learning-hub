"use client";

import { useEffect, useState, useCallback } from "react";
import Link from "next/link";
import { ChevronRight, BookOpen, Clock, TrendingUp } from "lucide-react";
import { getApiDocument, getApiSubject, getApiDocumentReadingHistory } from "@/src/api/database/sdk.gen";
import { DocumentCard } from "@/src/components/documents/document-card";
import { ScrollArea, ScrollBar } from "@/src/components/ui/scroll-area";
import type { DocumentDto, SubjectDto2, ReadingHistoryItemDto } from "@/src/api/database/types.gen";
import { useTranslations } from "next-intl";

interface PagedResponse<T> {
    items?: T[];
    totalCount?: number;
}

interface SubjectWithDocs {
    subject: SubjectDto2;
    documents: DocumentDto[];
}

export function HomePageSections() {
    const t = useTranslations("home");
    const [subjectsWithDocs, setSubjectsWithDocs] = useState<SubjectWithDocs[]>([]);
    const [recentDocs, setRecentDocs] = useState<ReadingHistoryItemDto[]>([]);
    const [latestDocs, setLatestDocs] = useState<DocumentDto[]>([]);
    const [loading, setLoading] = useState(true);

    const loadData = useCallback(async () => {
        setLoading(true);
        try {
            // 1. Get subjects with document count
            const subjectsRes = await getApiSubject({ query: { PageSize: 20 } });
            const subjectsData = (subjectsRes as unknown as { data: PagedResponse<SubjectDto2> })?.data || subjectsRes as PagedResponse<SubjectDto2>;
            const subjects = subjectsData.items || [];

            // 2. Get reading history for "Recent" section
            try {
                const historyRes = await getApiDocumentReadingHistory({ query: { PageSize: 8 } });
                const historyData = (historyRes as unknown as { data: PagedResponse<ReadingHistoryItemDto> })?.data || historyRes as PagedResponse<ReadingHistoryItemDto>;
                setRecentDocs(historyData.items || []);
            } catch {
                // Not logged in or no history
                setRecentDocs([]);
            }

            // 3. Get latest documents
            const latestRes = await getApiDocument({ query: { PageSize: 12, SortBy: "createdAt", SortDescending: true } });
            const latestData = (latestRes as unknown as { data: PagedResponse<DocumentDto> })?.data || latestRes as PagedResponse<DocumentDto>;
            setLatestDocs(latestData.items || []);

            // 4. Get documents for top 5 subjects
            const topSubjects = subjects.slice(0, 5);
            const subjectsWithDocsList: SubjectWithDocs[] = [];

            for (const subject of topSubjects) {
                if (!subject.id) continue;
                const docsRes = await getApiDocument({ query: { SubjectId: subject.id, PageSize: 10 } });
                const docsData = (docsRes as unknown as { data: PagedResponse<DocumentDto> })?.data || docsRes as PagedResponse<DocumentDto>;
                if ((docsData.items?.length || 0) > 0) {
                    subjectsWithDocsList.push({
                        subject,
                        documents: docsData.items || [],
                    });
                }
            }

            setSubjectsWithDocs(subjectsWithDocsList);
        } catch (err) {
            console.error("Error loading homepage data:", err);
        } finally {
            setLoading(false);
        }
    }, []);

    useEffect(() => {
        loadData();
    }, [loadData]);

    if (loading) {
        return (
            <div className="space-y-8 w-full">
                {[1, 2, 3].map((i) => (
                    <div key={i} className="animate-pulse">
                        <div className="h-6 w-40 bg-slate-200 dark:bg-slate-700 rounded mb-4"></div>
                        <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 xl:grid-cols-6 gap-3">
                            {[1, 2, 3, 4, 5, 6].map((j) => (
                                <div key={j} className="aspect-[3/4] bg-slate-200 dark:bg-slate-700 rounded"></div>
                            ))}
                        </div>
                    </div>
                ))}
            </div>
        );
    }

    return (
        <div className="space-y-8">
            {/* Recent Documents - Reading History */}
            {recentDocs.length > 0 && (
                <Section
                    title={t("recentlyViewed")}
                    icon={<Clock className="h-5 w-5 text-orange-500" />}
                    href="/recent"
                >
                    <HorizontalScroll>
                        {recentDocs.map((item) => (
                            <DocumentCard
                                key={item.documentFileId || item.documentId}
                                id={item.documentId}
                                title={item.documentName || ""}
                                subjectName={item.subjectName || undefined}
                                thumbnailFileId={item.coverFileId}
                                className="w-44 flex-shrink-0"
                            />
                        ))}
                    </HorizontalScroll>
                </Section>
            )}

            {/* Latest Documents */}
            {latestDocs.length > 0 && (
                <Section
                    title={t("latestDocuments")}
                    icon={<TrendingUp className="h-5 w-5 text-emerald-500" />}
                    href="/library"
                >
                    <HorizontalScroll>
                        {latestDocs.map((doc) => (
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
                                className="w-44 flex-shrink-0"
                            />
                        ))}
                    </HorizontalScroll>
                </Section>
            )}

            {/* Documents by Subject */}
            {subjectsWithDocs.map(({ subject, documents }) => (
                <Section
                    key={subject.id}
                    title={subject.subjectName || ""}
                    subtitle={subject.subjectCode}
                    icon={<BookOpen className="h-5 w-5 text-sky-500" />}
                    href={`/library?subjectId=${subject.id}`}
                >
                    <HorizontalScroll>
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
                                className="w-44 flex-shrink-0"
                            />
                        ))}
                    </HorizontalScroll>
                </Section>
            ))}

            {/* Empty state */}
            {subjectsWithDocs.length === 0 && latestDocs.length === 0 && recentDocs.length === 0 && (
                <div className="text-center py-12 text-slate-500 dark:text-slate-400">
                    <BookOpen className="h-12 w-12 mx-auto mb-4 opacity-50" />
                    <p>{t("noDocuments")}</p>
                </div>
            )}
        </div>
    );
}

interface SectionProps {
    title: string;
    subtitle?: string;
    icon?: React.ReactNode;
    href?: string;
    children: React.ReactNode;
}

function Section({ title, subtitle, icon, href, children }: SectionProps) {
    const t = useTranslations("home");

    return (
        <section>
            <div className="flex items-center justify-between mb-3">
                <div className="flex items-center gap-2">
                    {icon}
                    <h2 className="text-lg font-semibold text-slate-900 dark:text-slate-100">
                        {title}
                    </h2>
                    {subtitle && (
                        <span className="text-sm text-slate-500 dark:text-slate-400">
                            ({subtitle})
                        </span>
                    )}
                </div>
                {href && (
                    <Link
                        href={href}
                        className="flex items-center gap-1 text-sm text-sky-600 hover:text-sky-700 dark:text-sky-400 dark:hover:text-sky-300"
                    >
                        {t("viewMore")}
                        <ChevronRight className="h-4 w-4" />
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
            <div className="flex gap-3 pb-4">
                {children}
            </div>
            <ScrollBar orientation="horizontal" />
        </ScrollArea>
    );
}

