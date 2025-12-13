"use client";

import { useState, useEffect, useCallback } from "react";
import { Label } from "@/src/components/ui/label";
import { Input } from "@/src/components/ui/input";
import { useTranslations } from "next-intl";
import { AlertCircle, Loader2 } from "lucide-react";
import { useDebounce } from "@/src/hooks/use-debounce";
import { useAuthors } from "@/src/hooks/use-authors";
import type { AuthorInput, UpdateAuthorCommand, AuthorListDto } from "@/src/api/database/types.gen";

export interface AuthorFormData {
    id?: string;
    fullName?: string | null;
    description?: string | null;
}

interface AuthorFormProps {
    initialData?: AuthorFormData;
    onSubmit: (data: AuthorInput | UpdateAuthorCommand) => void | Promise<void>;
    loading?: boolean;
}

export function AuthorForm({
    initialData,
    onSubmit,
    loading,
}: AuthorFormProps) {
    const t = useTranslations("admin.authors");
    const { checkNameExists } = useAuthors();
    const [formData, setFormData] = useState<AuthorFormData>({
        fullName: null,
        description: null,
    });
    const [searching, setSearching] = useState(false);
    const [matchingAuthors, setMatchingAuthors] = useState<AuthorListDto[]>([]);
    const [isDuplicate, setIsDuplicate] = useState(false);

    const debouncedName = useDebounce(formData.fullName || "", 400);

    useEffect(() => {
        if (initialData) {
            setFormData(initialData);
        }
    }, [initialData]);

    // Search for matching authors when name changes
    const searchAuthors = useCallback(async (searchTerm: string) => {
        if (!searchTerm.trim()) {
            setMatchingAuthors([]);
            setIsDuplicate(false);
            return;
        }

        setSearching(true);
        try {
            const exists = await checkNameExists(searchTerm, initialData?.id);
            setIsDuplicate(exists);
        } catch (error) {
            console.error("Error searching authors:", error);
            setIsDuplicate(false);
        } finally {
            setSearching(false);
        }
    }, [checkNameExists, initialData?.id]);

    useEffect(() => {
        searchAuthors(debouncedName);
    }, [debouncedName, searchAuthors]);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        if (isDuplicate) return;
        const command: AuthorInput | UpdateAuthorCommand = {
            fullName: formData.fullName || undefined,
            description: formData.description || undefined,
        };
        await onSubmit(command);
    };

    const isDisabled = loading;

    return (
        <form id="author-form" onSubmit={handleSubmit} className="space-y-4">
            <div className="space-y-3">
                <div>
                    <Label htmlFor="fullName">{t("form.fullName")} *</Label>
                    <div className="relative">
                        <Input
                            id="fullName"
                            value={formData.fullName || ""}
                            onChange={(e) =>
                                setFormData((prev) => ({ ...prev, fullName: e.target.value }))
                            }
                            required
                            disabled={isDisabled}
                            className={`mt-1 ${isDuplicate ? "border-red-500 focus-visible:ring-red-500" : ""}`}
                            placeholder={t("form.fullNamePlaceholder")}
                        />
                        {searching && (
                            <div className="absolute right-3 top-1/2 -translate-y-1/2 mt-0.5">
                                <Loader2 className="h-4 w-4 animate-spin text-slate-400" />
                            </div>
                        )}
                    </div>

                    {/* Duplicate warning */}
                    {isDuplicate && (
                        <div className="mt-2 flex items-center gap-1.5 text-sm text-red-600 dark:text-red-400">
                            <AlertCircle className="h-4 w-4" />
                            <span>{t("form.duplicateWarning")}</span>
                        </div>
                    )}
                </div>

                <div>
                    <Label htmlFor="description">{t("form.description")}</Label>
                    <textarea
                        id="description"
                        value={formData.description || ""}
                        onChange={(e) =>
                            setFormData((prev) => ({ ...prev, description: e.target.value }))
                        }
                        disabled={isDisabled}
                        rows={4}
                        className="mt-1 flex w-full  border border-input bg-background text-foreground px-3 py-2 text-sm shadow-sm transition-colors placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-50 resize-y"
                        placeholder={t("form.descriptionPlaceholder")}
                    />
                </div>
            </div>
        </form>
    );
}

