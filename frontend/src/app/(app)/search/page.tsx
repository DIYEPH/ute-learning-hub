"use client";

import { useEffect, useState, useCallback } from "react";
import { useSearchParams, useRouter } from "next/navigation";
import { Search, Filter, X, Loader2, ChevronDown, ChevronUp } from "lucide-react";
import { getApiDocument, getApiSubject, getApiTag, getApiAuthor, getApiType } from "@/src/api";
import { DocumentCard } from "@/src/components/documents/document-card";
import { Input } from "@/src/components/ui/input";
import { Button } from "@/src/components/ui/button";
import { useTranslations } from "next-intl";
import type { DocumentDto, SubjectDto2, TagDto, AuthorDto, TypeDto } from "@/src/api/database/types.gen";

interface PagedResponse<T> {
    items?: T[];
    totalCount?: number;
    hasNextPage?: boolean;
}

const PAGE_SIZE = 24;

export default function SearchPage() {
    const t = useTranslations("search");
    const router = useRouter();
    const searchParams = useSearchParams();

    // Search & filters state
    const [searchTerm, setSearchTerm] = useState(searchParams.get("q") || "");
    const [debouncedSearch, setDebouncedSearch] = useState(searchTerm);
    const [selectedSubjectId, setSelectedSubjectId] = useState<string | null>(searchParams.get("subject"));
    const [noSubject, setNoSubject] = useState(false); // Filter for docs without subject
    const [selectedTagIds, setSelectedTagIds] = useState<string[]>(searchParams.get("tags")?.split(",").filter(Boolean) || []);
    const [selectedAuthorId, setSelectedAuthorId] = useState<string | null>(searchParams.get("author"));
    const [selectedTypeId, setSelectedTypeId] = useState<string | null>(searchParams.get("type"));
    const [sortBy, setSortBy] = useState<string>(searchParams.get("sort") || "date");

    // Filter search state
    const [subjectSearch, setSubjectSearch] = useState("");
    const [authorSearch, setAuthorSearch] = useState("");

    // Filter options
    const [subjects, setSubjects] = useState<SubjectDto2[]>([]);
    const [tags, setTags] = useState<TagDto[]>([]);
    const [authors, setAuthors] = useState<AuthorDto[]>([]);
    const [types, setTypes] = useState<TypeDto[]>([]);

    // Results
    const [documents, setDocuments] = useState<DocumentDto[]>([]);
    const [totalCount, setTotalCount] = useState(0);
    const [page, setPage] = useState(1);
    const [hasMore, setHasMore] = useState(false);
    const [loading, setLoading] = useState(false);
    const [loadingMore, setLoadingMore] = useState(false);

    // Sidebar collapse state
    const [showFilters, setShowFilters] = useState(false);
    const [expandedSections, setExpandedSections] = useState({
        subject: true,
        tag: true,
        author: false,
        type: false,
    });

    // Filtered options based on search
    const filteredSubjects = subjectSearch
        ? subjects.filter((s) => s.subjectName?.toLowerCase().includes(subjectSearch.toLowerCase()))
        : subjects.slice(0, 20); // Show max 20 if no search

    const filteredAuthors = authorSearch
        ? authors.filter((a) => a.fullName?.toLowerCase().includes(authorSearch.toLowerCase()))
        : authors.slice(0, 20); // Show max 20 if no search

    // Load filter options on mount
    useEffect(() => {
        void loadFilterOptions();
    }, []);

    // Sync searchTerm with URL query param when it changes
    useEffect(() => {
        const urlQuery = searchParams.get("q") || "";
        if (urlQuery !== searchTerm) {
            setSearchTerm(urlQuery);
            setDebouncedSearch(urlQuery);
        }
    }, [searchParams]);

    // Debounce search (for filter changes, not URL)
    useEffect(() => {
        const timer = setTimeout(() => {
            setDebouncedSearch(searchTerm);
        }, 400);
        return () => clearTimeout(timer);
    }, [searchTerm]);

    // Search when filters change
    useEffect(() => {
        setPage(1);
        void searchDocuments(1, false);
    }, [debouncedSearch, selectedSubjectId, noSubject, selectedTagIds, selectedAuthorId, selectedTypeId, sortBy]);

    const loadFilterOptions = async () => {
        try {
            const [subjectsRes, tagsRes, authorsRes, typesRes] = await Promise.all([
                getApiSubject({ query: { PageSize: 100 } }),
                getApiTag({ query: { PageSize: 100 } }),
                getApiAuthor({ query: { PageSize: 100 } }),
                getApiType({ query: { PageSize: 50 } }),
            ]);

            setSubjects(((subjectsRes as any)?.data?.items || (subjectsRes as any)?.items) || []);
            setTags(((tagsRes as any)?.data?.items || (tagsRes as any)?.items) || []);
            setAuthors(((authorsRes as any)?.data?.items || (authorsRes as any)?.items) || []);
            setTypes(((typesRes as any)?.data?.items || (typesRes as any)?.items) || []);
        } catch (err) {
            console.error("Error loading filter options:", err);
        }
    };

    const searchDocuments = async (pageNum: number, append: boolean) => {
        if (append) {
            setLoadingMore(true);
        } else {
            setLoading(true);
        }

        try {
            const res = await getApiDocument({
                query: {
                    SearchTerm: debouncedSearch || undefined,
                    SubjectId: selectedSubjectId || undefined,
                    TagIds: selectedTagIds.length > 0 ? selectedTagIds : undefined,
                    AuthorId: selectedAuthorId || undefined,
                    TypeId: selectedTypeId || undefined,
                    Page: pageNum,
                    PageSize: PAGE_SIZE,
                    SortBy: sortBy || "date",
                    SortDescending: true,
                },
            });

            const data = (res as unknown as { data: PagedResponse<DocumentDto> })?.data || res as PagedResponse<DocumentDto>;
            const items = data.items || [];

            if (append) {
                setDocuments((prev) => [...prev, ...items]);
            } else {
                setDocuments(items);
            }

            setTotalCount(data.totalCount || 0);
            setHasMore(!!data.hasNextPage);
            setPage(pageNum);
        } catch (err) {
            console.error("Error searching documents:", err);
        } finally {
            setLoading(false);
            setLoadingMore(false);
        }
    };

    const handleLoadMore = () => {
        if (hasMore && !loadingMore) {
            void searchDocuments(page + 1, true);
        }
    };

    const handleTagToggle = (tagId: string) => {
        setSelectedTagIds((prev) =>
            prev.includes(tagId) ? prev.filter((id) => id !== tagId) : [...prev, tagId]
        );
    };

    const clearFilters = () => {
        setSearchTerm("");
        setSelectedSubjectId(null);
        setNoSubject(false);
        setSelectedTagIds([]);
        setSelectedAuthorId(null);
        setSelectedTypeId(null);
        setSubjectSearch("");
        setAuthorSearch("");
    };

    const hasActiveFilters = selectedSubjectId || noSubject || selectedTagIds.length > 0 || selectedAuthorId || selectedTypeId;

    const toggleSection = (section: keyof typeof expandedSections) => {
        setExpandedSections((prev) => ({ ...prev, [section]: !prev[section] }));
    };

    return (
        <div className="flex flex-col lg:flex-row gap-6">
            {/* Filter Sidebar - Hidden on mobile unless toggled */}
            <aside className={`shrink-0 transition-all ${showFilters
                ? "fixed inset-0 z-40 bg-background lg:relative lg:inset-auto lg:z-auto lg:bg-transparent w-full lg:w-64 p-4 lg:p-0 overflow-auto"
                : "hidden lg:block lg:w-64"
                }`}>
                <div className="sticky top-0 space-y-4">
                    <div className="flex items-center justify-between">
                        <div className="flex items-center gap-2">
                            {/* Close button for mobile - inline */}
                            <Button
                                variant="ghost"
                                size="sm"
                                onClick={() => setShowFilters(false)}
                                className="lg:hidden -ml-2"
                            >
                                <X className="h-4 w-4" />
                            </Button>
                            <h2 className="font-semibold text-foreground">{t("filters")}</h2>
                        </div>
                        {hasActiveFilters && (
                            <button onClick={clearFilters} className="text-xs text-primary hover:underline">{t("clearAll")}
                            </button>
                        )}
                    </div>

                    {/* Subject Filter */}
                    <FilterSection
                        title={t("subject")}
                        expanded={expandedSections.subject}
                        onToggle={() => toggleSection("subject")}
                    >
                        <div className="space-y-2">
                            {/* Subject search input */}
                            <Input
                                type="text"
                                placeholder={t("searchSubject")}
                                value={subjectSearch}
                                onChange={(e) => setSubjectSearch(e.target.value)}
                                className="h-8 text-sm"
                            />
                            {/* Uncategorized option */}
                            <button
                                onClick={() => {
                                    setNoSubject(!noSubject);
                                    setSelectedSubjectId(null);
                                }}
                                className={`w-full text-left px-2 py-1.5 rounded text-sm transition-colors ${noSubject
                                    ? "bg-muted text-foreground"
                                    : "hover:bg-muted text-muted-foreground italic"
                                    }`}
                            >
                                {t("noSubjectFilter")}
                            </button>
                            {/* Subject list */}
                            <div className="space-y-1 max-h-40 overflow-y-auto">
                                {filteredSubjects.map((subject) => (
                                    <button
                                        key={subject.id}
                                        onClick={() => {
                                            setSelectedSubjectId(selectedSubjectId === subject.id ? null : subject.id || null);
                                            setNoSubject(false);
                                        }}
                                        className={`w-full text-left px-2 py-1.5 rounded text-sm transition-colors ${selectedSubjectId === subject.id
                                            ? "bg-primary/10 text-primary"
                                            : "hover:bg-muted text-foreground"
                                            }`}
                                    >
                                        {subject.subjectName}
                                    </button>
                                ))}
                                {subjectSearch && filteredSubjects.length === 0 && (
                                    <div className="text-sm text-muted-foreground px-2 py-1">{t("noSubjectFound")}</div>
                                )}
                                {!subjectSearch && subjects.length > 20 && (
                                    <div className="text-xs text-muted-foreground px-2 py-1">{t("typeToSearch")}</div>
                                )}
                            </div>
                        </div>
                    </FilterSection>

                    {/* Tag Filter */}
                    <FilterSection
                        title={t("tags")}
                        expanded={expandedSections.tag}
                        onToggle={() => toggleSection("tag")}
                        badge={selectedTagIds.length > 0 ? selectedTagIds.length : undefined}
                    >
                        <div className="flex flex-wrap gap-1.5 max-h-48 overflow-y-auto">
                            {tags.map((tag) => (
                                <button
                                    key={tag.id}
                                    onClick={() => tag.id && handleTagToggle(tag.id)}
                                    className={`px-2 py-1 rounded-full text-xs transition-colors ${tag.id && selectedTagIds.includes(tag.id)
                                        ? "bg-violet-100 text-violet-700 dark:bg-violet-900 dark:text-violet-300"
                                        : "bg-muted hover:bg-muted/80 text-muted-foreground"
                                        }`}
                                >
                                    {tag.tagName}
                                </button>
                            ))}
                        </div>
                    </FilterSection>

                    {/* Author Filter */}
                    <FilterSection
                        title={t("author")}
                        expanded={expandedSections.author}
                        onToggle={() => toggleSection("author")}
                    >
                        <div className="space-y-2">
                            {/* Author search input */}
                            <Input
                                type="text"
                                placeholder={t("searchAuthor")}
                                value={authorSearch}
                                onChange={(e) => setAuthorSearch(e.target.value)}
                                className="h-8 text-sm"
                            />
                            <div className="space-y-1 max-h-40 overflow-y-auto">
                                {filteredAuthors.map((author) => (
                                    <button
                                        key={author.id}
                                        onClick={() => setSelectedAuthorId(selectedAuthorId === author.id ? null : author.id || null)}
                                        className={`w-full text-left px-2 py-1.5 rounded text-sm transition-colors ${selectedAuthorId === author.id
                                            ? "bg-pink-100 text-pink-700 dark:bg-pink-900 dark:text-pink-300"
                                            : "hover:bg-muted text-foreground"
                                            }`}
                                    >
                                        {author.fullName}
                                    </button>
                                ))}
                                {authorSearch && filteredAuthors.length === 0 && (
                                    <div className="text-sm text-muted-foreground px-2 py-1">{t("noAuthorFound")}</div>
                                )}
                                {!authorSearch && authors.length > 20 && (
                                    <div className="text-xs text-muted-foreground px-2 py-1">{t("typeToSearch")}</div>
                                )}
                            </div>
                        </div>
                    </FilterSection>

                    {/* Type Filter */}
                    <FilterSection
                        title={t("type")}
                        expanded={expandedSections.type}
                        onToggle={() => toggleSection("type")}
                    >
                        <div className="space-y-1">
                            {types.map((type) => (
                                <button
                                    key={type.id}
                                    onClick={() => setSelectedTypeId(selectedTypeId === type.id ? null : type.id || null)}
                                    className={`w-full text-left px-2 py-1.5 rounded text-sm transition-colors ${selectedTypeId === type.id
                                        ? "bg-emerald-100 text-emerald-700 dark:bg-emerald-900 dark:text-emerald-300"
                                        : "hover:bg-muted text-foreground"
                                        }`}
                                >
                                    {type.typeName}
                                </button>
                            ))}
                        </div>
                    </FilterSection>
                </div>
            </aside>

            {/* Main Content */}
            <main className="flex-1 min-w-0 space-y-4">
                {/* Filter Toggle for mobile */}
                <div className="lg:hidden">
                    <Button
                        variant="outline"
                        size="sm"
                        onClick={() => setShowFilters(!showFilters)}
                        className="w-full"
                    >
                        <Filter className="h-4 w-4 mr-2" />
                        {t("filters")}
                    </Button>
                </div>

                {/* Active Filters */}
                {hasActiveFilters && (
                    <div className="flex flex-wrap gap-2 items-center">
                        <span className="text-sm text-muted-foreground">{t("activeFilters")}:</span>
                        {selectedSubjectId && (
                            <FilterBadge
                                label={subjects.find((s) => s.id === selectedSubjectId)?.subjectName || ""}
                                onRemove={() => setSelectedSubjectId(null)}
                                color="sky"
                            />
                        )}
                        {selectedTagIds.map((tagId) => (
                            <FilterBadge
                                key={tagId}
                                label={tags.find((t) => t.id === tagId)?.tagName || ""}
                                onRemove={() => handleTagToggle(tagId)}
                                color="violet"
                            />
                        ))}
                        {selectedAuthorId && (
                            <FilterBadge
                                label={authors.find((a) => a.id === selectedAuthorId)?.fullName || ""}
                                onRemove={() => setSelectedAuthorId(null)}
                                color="pink"
                            />
                        )}
                        {selectedTypeId && (
                            <FilterBadge
                                label={types.find((t) => t.id === selectedTypeId)?.typeName || ""}
                                onRemove={() => setSelectedTypeId(null)}
                                color="emerald"
                            />
                        )}
                    </div>
                )}

                {/* Results Count */}
                <div className="text-sm text-muted-foreground">
                    {loading ? t("searching") : t("resultsCount", { count: totalCount })}
                </div>

                {/* Results */}
                {loading && documents.length === 0 ? (
                    <div className="flex items-center justify-center py-12">
                        <Loader2 className="h-6 w-6 animate-spin text-primary" />
                    </div>
                ) : documents.length === 0 ? (
                    <div className="text-center py-12 text-muted-foreground">
                        {t("noResults")}
                    </div>
                ) : (
                    <div className="grid gap-4 grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
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
                                totalViewCount={doc.totalViewCount}
                            />
                        ))}
                    </div>
                )}

                {/* Load More */}
                {hasMore && (
                    <div className="flex justify-center pt-4">
                        <Button variant="outline" onClick={handleLoadMore} disabled={loadingMore}>
                            {loadingMore && <Loader2 className="h-4 w-4 mr-2 animate-spin" />}
                            {t("loadMore")}
                        </Button>
                    </div>
                )}
            </main>
        </div>
    );
}

// Filter Section Component
function FilterSection({
    title,
    expanded,
    onToggle,
    badge,
    children,
}: {
    title: string;
    expanded: boolean;
    onToggle: () => void;
    badge?: number;
    children: React.ReactNode;
}) {
    return (
        <div className="border-b border-border pb-3">
            <button
                onClick={onToggle}
                className="flex items-center justify-between w-full text-left py-1"
            >
                <span className="text-sm font-medium text-foreground flex items-center gap-2">
                    {title}
                    {badge && (
                        <span className="px-1.5 py-0.5 rounded-full bg-primary/10 text-primary text-xs">
                            {badge}
                        </span>
                    )}
                </span>
                {expanded ? (
                    <ChevronUp className="h-4 w-4 text-muted-foreground" />
                ) : (
                    <ChevronDown className="h-4 w-4 text-muted-foreground" />
                )}
            </button>
            {expanded && <div className="mt-2">{children}</div>}
        </div>
    );
}

// Filter Badge Component
function FilterBadge({
    label,
    onRemove,
    color,
}: {
    label: string;
    onRemove: () => void;
    color: "sky" | "violet" | "pink" | "emerald";
}) {
    const colorClasses = {
        sky: "bg-primary/10 text-primary",
        violet: "bg-violet-100 text-violet-700 dark:bg-violet-900 dark:text-violet-300",
        pink: "bg-pink-100 text-pink-700 dark:bg-pink-900 dark:text-pink-300",
        emerald: "bg-emerald-100 text-emerald-700 dark:bg-emerald-900 dark:text-emerald-300",
    };

    return (
        <span className={`inline-flex items-center gap-1 px-2 py-1 rounded-full text-xs ${colorClasses[color]}`}>
            {label}
            <button onClick={onRemove} className="hover:opacity-70">
                <X className="h-3 w-3" />
            </button>
        </span>
    );
}

