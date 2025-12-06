"use client";

import { useState, useEffect, useCallback } from "react";
import { Label } from "@/src/components/ui/label";
import { Input } from "@/src/components/ui/input";
import { useTranslations } from "next-intl";
import { useDebounce } from "@/src/hooks/use-debounce";
import { getApiTag } from "@/src/api/database/sdk.gen";
import type { TagDto } from "@/src/api/database/types.gen";
import type { CreateTagCommand, UpdateTagCommand } from "@/src/hooks/use-tags";
import { AlertCircle, Loader2 } from "lucide-react";

export interface TagFormData {
    id?: string;
    tagName?: string | null;
}

interface TagFormProps {
    initialData?: TagFormData;
    onSubmit: (data: CreateTagCommand | UpdateTagCommand) => void | Promise<void>;
    loading?: boolean;
}

export function TagForm({
    initialData,
    onSubmit,
    loading,
}: TagFormProps) {
    const t = useTranslations("admin.tags");
    const [formData, setFormData] = useState<TagFormData>({
        tagName: null,
    });

    const [searching, setSearching] = useState(false);
    const [matchingTags, setMatchingTags] = useState<TagDto[]>([]);
    const [isDuplicate, setIsDuplicate] = useState(false);

    const debouncedName = useDebounce(formData.tagName || "", 400);

    useEffect(() => {
        if (initialData) {
            setFormData(initialData);
        }
    }, [initialData]);

    const searchTags = useCallback(async (searchTerm: string) => {
        if (!searchTerm.trim()) {
            setMatchingTags([]);
            setIsDuplicate(false);
            return;
        }

        setSearching(true);
        try {
            const response = await getApiTag({ query: { SearchTerm: searchTerm, Page: 1, PageSize: 5 } });
            const data = (response as unknown as { data: { items?: TagDto[] } })?.data || response as { items?: TagDto[] };
            const items = data?.items || [];

            const filtered = initialData?.id
                ? items.filter(item => item.id !== initialData.id)
                : items;

            setMatchingTags(filtered);

            const exactMatch = filtered.some(
                item => item.tagName?.toLowerCase() === searchTerm.toLowerCase()
            );
            setIsDuplicate(exactMatch);
        } catch (error) {
            console.error("Error searching tags:", error);
            setMatchingTags([]);
            setIsDuplicate(false);
        } finally {
            setSearching(false);
        }
    }, [initialData?.id]);

    useEffect(() => {
        searchTags(debouncedName);
    }, [debouncedName, searchTags]);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        if (isDuplicate) {
            return;
        }
        const command: CreateTagCommand | UpdateTagCommand = {
            tagName: formData.tagName || "",
        };
        await onSubmit(command);
    };

    const isDisabled = loading;

    return (
        <form id="tag-form" onSubmit={handleSubmit} className="space-y-4">
            <div className="space-y-3">
                <div>
                    <Label htmlFor="tagName">{t("form.tagName")} *</Label>
                    <div className="relative">
                        <Input
                            id="tagName"
                            value={formData.tagName || ""}
                            onChange={(e) =>
                                setFormData((prev) => ({ ...prev, tagName: e.target.value }))
                            }
                            required
                            disabled={isDisabled}
                            className={`mt-1 ${isDuplicate ? "border-red-500 focus-visible:ring-red-500" : ""}`}
                            placeholder={t("form.tagNamePlaceholder")}
                        />
                        {searching && (
                            <div className="absolute right-3 top-1/2 -translate-y-1/2 mt-0.5">
                                <Loader2 className="h-4 w-4 animate-spin text-slate-400" />
                            </div>
                        )}
                    </div>

                    {isDuplicate && (
                        <div className="mt-2 flex items-center gap-1.5 text-sm text-red-600 dark:text-red-400">
                            <AlertCircle className="h-4 w-4" />
                            <span>{t("form.duplicateWarning")}</span>
                        </div>
                    )}

                    {matchingTags.length > 0 && !isDuplicate && (
                        <div className="mt-2 p-2 bg-slate-50 dark:bg-slate-800 rounded-md border border-slate-200 dark:border-slate-700">
                            <p className="text-xs text-slate-500 dark:text-slate-400 mb-1">
                                {t("form.similarTags")}:
                            </p>
                            <ul className="space-y-0.5">
                                {matchingTags.map((tag) => (
                                    <li key={tag.id} className="text-sm text-slate-700 dark:text-slate-300">
                                        • {tag.tagName}
                                    </li>
                                ))}
                            </ul>
                        </div>
                    )}
                </div>
            </div>
        </form>
    );
}
