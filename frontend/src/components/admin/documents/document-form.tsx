"use client";

import { useState, useEffect } from "react";
import { Label } from "@/src/components/ui/label";
import { Input } from "@/src/components/ui/input";
import { useTranslations } from "next-intl";
import { useSubjects } from "@/src/hooks/use-subjects";
import { useTags } from "@/src/hooks/use-tags";
import type { UpdateDocumentCommandRequest, SubjectDto2, TagDto } from "@/src/api/database/types.gen";

// VisibilityStatus enum mapping
const VisibilityOptions = [
    { value: 0, label: "Private" },
    { value: 1, label: "Public" },
];

export interface DocumentFormData {
    id?: string;
    documentName?: string | null;
    description?: string | null;
    subjectId?: string | null;
    subject?: { id?: string } | null;
    typeId?: string | null;
    type?: { id?: string } | null;
    tagIds?: string[] | null;
    tags?: { id?: string }[] | null;
    visibility?: number | null;
}

interface DocumentFormProps {
    initialData?: DocumentFormData;
    onSubmit: (data: UpdateDocumentCommandRequest) => void | Promise<void>;
    loading?: boolean;
}

export function DocumentForm({
    initialData,
    onSubmit,
    loading,
}: DocumentFormProps) {
    const t = useTranslations("admin.documents");
    const { fetchSubjects, loading: loadingSubjects } = useSubjects();
    const { fetchTags, loading: loadingTags } = useTags();

    const [formData, setFormData] = useState<DocumentFormData>({
        documentName: null,
        description: null,
        subjectId: null,
        typeId: null,
        tagIds: null,
        visibility: 0,
    });
    const [subjects, setSubjects] = useState<SubjectDto2[]>([]);
    const [allTags, setAllTags] = useState<TagDto[]>([]);

    useEffect(() => {
        if (initialData) {
            setFormData({
                ...initialData,
                subjectId: initialData.subjectId || initialData.subject?.id || null,
                typeId: initialData.typeId || initialData.type?.id || null,
                tagIds: initialData.tagIds || initialData.tags?.map(tag => tag.id || "").filter(Boolean) || null,
            });
        }
    }, [initialData]);

    useEffect(() => {
        const loadData = async () => {
            try {
                const [subjectsRes, tagsRes] = await Promise.all([
                    fetchSubjects({ Page: 1, PageSize: 1000 }),
                    fetchTags({ Page: 1, PageSize: 1000 }),
                ]);
                if (subjectsRes?.items) setSubjects(subjectsRes.items);
                if (tagsRes?.items) setAllTags(tagsRes.items);
            } catch (err) {
                console.error("Error loading data:", err);
            }
        };
        loadData();
    }, [fetchSubjects, fetchTags]);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        const command: UpdateDocumentCommandRequest = {
            documentName: formData.documentName || undefined,
            description: formData.description || undefined,
            subjectId: formData.subjectId || undefined,
            typeId: formData.typeId || undefined,
            tagIds: formData.tagIds || undefined,
            visibility: formData.visibility ?? undefined,
        };
        await onSubmit(command);
    };

    const handleTagToggle = (tagId: string) => {
        setFormData((prev) => {
            const currentTags = prev.tagIds || [];
            const newTags = currentTags.includes(tagId)
                ? currentTags.filter((id) => id !== tagId)
                : [...currentTags, tagId];
            return { ...prev, tagIds: newTags };
        });
    };

    const isDisabled = loading || loadingSubjects || loadingTags;
    const selectClassName = "mt-1 flex h-9 w-full  border border-input bg-background text-foreground px-3 py-1 text-sm shadow-sm transition-colors focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-50";

    return (
        <form id="document-form" onSubmit={handleSubmit} className="space-y-4">
            <div className="space-y-3 grid grid-cols-1 md:grid-cols-2 gap-4">
                <div className="md:col-span-2">
                    <Label htmlFor="documentName">{t("form.documentName")} *</Label>
                    <Input
                        id="documentName"
                        value={formData.documentName || ""}
                        onChange={(e) =>
                            setFormData((prev) => ({ ...prev, documentName: e.target.value }))
                        }
                        required
                        disabled={isDisabled}
                        className="mt-1"
                        placeholder={t("form.documentNamePlaceholder")}
                    />
                </div>

                <div className="md:col-span-2">
                    <Label htmlFor="description">{t("form.description")}</Label>
                    <textarea
                        id="description"
                        value={formData.description || ""}
                        onChange={(e) =>
                            setFormData((prev) => ({ ...prev, description: e.target.value }))
                        }
                        disabled={isDisabled}
                        rows={3}
                        className="mt-1 flex w-full  border border-input bg-background text-foreground px-3 py-2 text-sm shadow-sm transition-colors placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-50 resize-y"
                        placeholder={t("form.descriptionPlaceholder")}
                    />
                </div>

                <div>
                    <Label htmlFor="subjectId">{t("form.subject")}</Label>
                    <select
                        id="subjectId"
                        value={formData.subjectId || ""}
                        onChange={(e) =>
                            setFormData((prev) => ({ ...prev, subjectId: e.target.value || null }))
                        }
                        disabled={isDisabled}
                        className={selectClassName}
                    >
                        <option value="">{t("form.selectSubject")}</option>
                        {subjects
                            .filter((s): s is SubjectDto2 & { id: string } => !!s?.id)
                            .map((subject) => (
                                <option key={subject.id} value={subject.id}>
                                    {subject.subjectName || ""} ({subject.subjectCode || ""})
                                </option>
                            ))}
                    </select>
                </div>

                <div>
                    <Label htmlFor="visibility">{t("form.visibility")}</Label>
                    <select
                        id="visibility"
                        value={formData.visibility ?? ""}
                        onChange={(e) =>
                            setFormData((prev) => ({
                                ...prev,
                                visibility: e.target.value !== "" ? parseInt(e.target.value) : null,
                            }))
                        }
                        disabled={isDisabled}
                        className={selectClassName}
                    >
                        <option value="">{t("form.selectVisibility")}</option>
                        {VisibilityOptions.map((option) => (
                            <option key={option.value} value={option.value}>
                                {option.label}
                            </option>
                        ))}
                    </select>
                </div>

                <div className="md:col-span-2">
                    <Label>{t("form.tags")}</Label>
                    <div className="mt-2 flex flex-wrap gap-2">
                        {allTags.map((tag) => (
                            <label
                                key={tag.id}
                                className={`inline-flex items-center px-3 py-1 rounded-full text-sm cursor-pointer transition-colors ${formData.tagIds?.includes(tag.id || "")
                                    ? "bg-primary text-primary-foreground"
                                    : "bg-secondary text-muted-foreground hover:bg-muted"
                                    }`}
                            >
                                <input
                                    type="checkbox"
                                    checked={formData.tagIds?.includes(tag.id || "") || false}
                                    onChange={() => tag.id && handleTagToggle(tag.id)}
                                    disabled={isDisabled}
                                    className="sr-only"
                                />
                                {tag.tagName}
                            </label>
                        ))}
                    </div>
                </div>
            </div>
        </form>
    );
}

