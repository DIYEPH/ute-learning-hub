"use client";

import { useState, useEffect } from "react";
import { Label } from "@/src/components/ui/label";
import { Input } from "@/src/components/ui/input";
import { useTranslations } from "next-intl";
import { useSubjects } from "@/src/hooks/use-subjects";
import type { UpdateConversationCommand, SubjectDto2 } from "@/src/api/database/types.gen";

// ConversationType enum mapping
const ConversationTypeOptions = [
    { value: 0, label: "Private" },
    { value: 1, label: "Group" },
    { value: 2, label: "AI" },
];

// ConversationStatus enum mapping
const ConversationStatusOptions = [
    { value: 0, label: "Active" },
    { value: 1, label: "Inactive" },
    { value: 2, label: "Archived" },
];

export interface ConversationFormData {
    id?: string;
    conversationName?: string | null;
    conversationType?: number | null;
    conversationStatus?: number | null;
    subjectId?: string | null;
    subject?: { id?: string } | null;
    isAllowMemberPin?: boolean | null;
    avatarUrl?: string | null;
}

interface ConversationFormProps {
    initialData?: ConversationFormData;
    onSubmit: (data: UpdateConversationCommand) => void | Promise<void>;
    loading?: boolean;
}

export function ConversationForm({
    initialData,
    onSubmit,
    loading,
}: ConversationFormProps) {
    const t = useTranslations("admin.conversations");
    const { fetchSubjects, loading: loadingSubjects } = useSubjects();
    const [formData, setFormData] = useState<ConversationFormData>({
        conversationName: null,
        conversationType: null,
        conversationStatus: null,
        subjectId: null,
        isAllowMemberPin: false,
        avatarUrl: null,
    });
    const [subjects, setSubjects] = useState<SubjectDto2[]>([]);

    useEffect(() => {
        if (initialData) {
            setFormData({
                ...initialData,
                subjectId: initialData.subjectId || initialData.subject?.id || null,
            });
        }
    }, [initialData]);

    useEffect(() => {
        const loadSubjects = async () => {
            try {
                const response = await fetchSubjects({ Page: 1, PageSize: 1000 });
                if (response?.items) {
                    setSubjects(response.items);
                }
            } catch (err) {
                console.error("Error loading subjects:", err);
            }
        };
        loadSubjects();
    }, [fetchSubjects]);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        const command: UpdateConversationCommand = {
            conversationName: formData.conversationName || undefined,
            conversationType: formData.conversationType ?? undefined,
            conversationStatus: formData.conversationStatus ?? undefined,
            subjectId: formData.subjectId || undefined,
            isAllowMemberPin: formData.isAllowMemberPin ?? undefined,
            avatarUrl: formData.avatarUrl || undefined,
        };
        await onSubmit(command);
    };

    const isDisabled = loading || loadingSubjects;
    const selectClassName = "mt-1 flex h-9 w-full  border border-input bg-background text-foreground px-3 py-1 text-sm shadow-sm transition-colors focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-50";

    return (
        <form id="conversation-form" onSubmit={handleSubmit} className="space-y-4">
            <div className="space-y-3 grid grid-cols-1 md:grid-cols-2 gap-4">
                <div className="md:col-span-2">
                    <Label htmlFor="conversationName">{t("form.conversationName")} *</Label>
                    <Input
                        id="conversationName"
                        value={formData.conversationName || ""}
                        onChange={(e) =>
                            setFormData((prev) => ({ ...prev, conversationName: e.target.value }))
                        }
                        required
                        disabled={isDisabled}
                        className="mt-1"
                        placeholder={t("form.conversationNamePlaceholder")}
                    />
                </div>

                <div>
                    <Label htmlFor="conversationType">{t("form.conversationType")}</Label>
                    <select
                        id="conversationType"
                        value={formData.conversationType ?? ""}
                        onChange={(e) =>
                            setFormData((prev) => ({
                                ...prev,
                                conversationType: e.target.value !== "" ? parseInt(e.target.value) : null,
                            }))
                        }
                        disabled={isDisabled}
                        className={selectClassName}
                    >
                        <option value="">{t("form.selectType")}</option>
                        {ConversationTypeOptions.map((option) => (
                            <option key={option.value} value={option.value}>
                                {option.label}
                            </option>
                        ))}
                    </select>
                </div>

                <div>
                    <Label htmlFor="conversationStatus">{t("form.conversationStatus")}</Label>
                    <select
                        id="conversationStatus"
                        value={formData.conversationStatus ?? ""}
                        onChange={(e) =>
                            setFormData((prev) => ({
                                ...prev,
                                conversationStatus: e.target.value !== "" ? parseInt(e.target.value) : null,
                            }))
                        }
                        disabled={isDisabled}
                        className={selectClassName}
                    >
                        <option value="">{t("form.selectStatus")}</option>
                        {ConversationStatusOptions.map((option) => (
                            <option key={option.value} value={option.value}>
                                {option.label}
                            </option>
                        ))}
                    </select>
                </div>

                <div className="md:col-span-2">
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
                            .filter((subject): subject is SubjectDto2 & { id: string } => !!subject?.id)
                            .map((subject) => (
                                <option key={subject.id} value={subject.id}>
                                    {subject.subjectName || ""} ({subject.subjectCode || ""})
                                </option>
                            ))}
                    </select>
                </div>

                <div>
                    <Label htmlFor="avatarUrl">{t("form.avatarUrl")}</Label>
                    <Input
                        id="avatarUrl"
                        type="url"
                        value={formData.avatarUrl || ""}
                        onChange={(e) =>
                            setFormData((prev) => ({ ...prev, avatarUrl: e.target.value }))
                        }
                        disabled={isDisabled}
                        className="mt-1"
                        placeholder="https://..."
                    />
                </div>

                <div className="flex items-center">
                    <label className="flex items-center gap-2 mt-6">
                        <input
                            type="checkbox"
                            checked={formData.isAllowMemberPin ?? false}
                            onChange={(e) =>
                                setFormData((prev) => ({ ...prev, isAllowMemberPin: e.target.checked }))
                            }
                            disabled={isDisabled}
                            className="cursor-pointer"
                        />
                        <span className="text-sm">{t("form.isAllowMemberPin")}</span>
                    </label>
                </div>
            </div>
        </form>
    );
}

