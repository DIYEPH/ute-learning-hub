"use client";

import { useState, useEffect, useCallback } from "react";
import { Label } from "@/src/components/ui/label";
import { Input } from "@/src/components/ui/input";
import { useTranslations } from "next-intl";
import { useDebounce } from "@/src/hooks/use-debounce";
import { getApiNotification, getApiUser } from "@/src/api/database/sdk.gen";
import type { CreateNotificationCommand, UpdateNotificationRequest, NotificationDto, UserDto } from "@/src/api/database/types.gen";
import { AlertCircle, Loader2 } from "lucide-react";
// (Auto-generated types like Message, Comment, Document, UserAction, Conversation are excluded)
const NotificationTypes = [
    { value: 0, label: "System" },
    { value: 6, label: "Event" },
    { value: 7, label: "Announcement" },
];

// NotificationPriorityType enum
const PriorityTypes = [
    { value: 0, label: "Low" },
    { value: 1, label: "Normal" },
    { value: 2, label: "High" },
];

export interface NotificationFormData {
    id?: string;
    objectId?: string;
    title?: string | null;
    content?: string | null;
    link?: string | null;
    isGlobal?: boolean;
    expiredAt?: string | null;
    notificationType?: number;
    notificationPriorityType?: number;
    recipientIds?: string[];
}

interface NotificationFormProps {
    initialData?: NotificationFormData;
    onSubmit: (data: CreateNotificationCommand | UpdateNotificationRequest) => void | Promise<void>;
    loading?: boolean;
    isEditMode?: boolean;
}

export function NotificationForm({
    initialData,
    onSubmit,
    loading,
    isEditMode = false,
}: NotificationFormProps) {
    const t = useTranslations("admin.notifications");
    const [formData, setFormData] = useState<NotificationFormData>({
        title: null,
        content: null,
        link: null,
        isGlobal: true,
        expiredAt: null,
        notificationType: 0,
        notificationPriorityType: 1,
        recipientIds: [],
    });

    // Users for recipient selection
    const [users, setUsers] = useState<UserDto[]>([]);
    const [loadingUsers, setLoadingUsers] = useState(false);

    // Debounce search state
    const [searching, setSearching] = useState(false);
    const [matchingNotifications, setMatchingNotifications] = useState<NotificationDto[]>([]);
    const [isDuplicate, setIsDuplicate] = useState(false);

    const debouncedTitle = useDebounce(formData.title || "", 400);

    useEffect(() => {
        if (initialData) {
            setFormData({
                ...initialData,
                objectId: initialData.objectId || crypto.randomUUID(),
            });
        } else {
            setFormData((prev) => ({
                ...prev,
                objectId: crypto.randomUUID(),
            }));
        }
    }, [initialData]);

    // Load users for recipient selection
    useEffect(() => {
        if (!formData.isGlobal && !isEditMode) {
            const loadUsers = async () => {
                setLoadingUsers(true);
                try {
                    const response = await getApiUser({ query: { Page: 1, PageSize: 100 } });
                    const data = (response as unknown as { data: { items?: UserDto[] } })?.data || response as { items?: UserDto[] };
                    setUsers(data?.items || []);
                } catch (err) {
                    console.error("Error loading users:", err);
                } finally {
                    setLoadingUsers(false);
                }
            };
            loadUsers();
        }
    }, [formData.isGlobal, isEditMode]);

    // Search for matching notifications when title changes
    const searchNotifications = useCallback(async (searchTerm: string) => {
        if (!searchTerm.trim()) {
            setMatchingNotifications([]);
            setIsDuplicate(false);
            return;
        }

        setSearching(true);
        try {
            // Fetch all and filter client-side since API doesn't support SearchTerm
            const response = await getApiNotification({ query: { Page: 1, PageSize: 100 } });
            const data = (response as unknown as { data: { items?: NotificationDto[] } })?.data || response as { items?: NotificationDto[] };
            const items = (data?.items || []).filter(
                (item) => item.title?.toLowerCase().includes(searchTerm.toLowerCase())
            );

            // Filter out current item if editing
            const filtered = initialData?.id
                ? items.filter(item => item.id !== initialData.id)
                : items;

            // Limit to 5 results
            setMatchingNotifications(filtered.slice(0, 5));

            // Check for exact duplicate
            const exactMatch = filtered.some(
                (item) => item.title?.toLowerCase() === searchTerm.toLowerCase()
            );
            setIsDuplicate(exactMatch);
        } catch (error) {
            console.error("Error searching notifications:", error);
            setMatchingNotifications([]);
            setIsDuplicate(false);
        } finally {
            setSearching(false);
        }
    }, [initialData?.id]);

    useEffect(() => {
        searchNotifications(debouncedTitle);
    }, [debouncedTitle, searchNotifications]);

    const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();
        if (isDuplicate) return;

        // Default expiredAt to 30 days from now if not set
        const expiredAt = formData.expiredAt || new Date(Date.now() + 30 * 24 * 60 * 60 * 1000).toISOString();

        if (isEditMode) {
            const command: UpdateNotificationRequest = {
                title: formData.title || "",
                content: formData.content || "",
                link: formData.link || undefined,
                expiredAt: expiredAt,
                notificationType: formData.notificationType ?? 0,
                notificationPriorityType: formData.notificationPriorityType ?? 1,
            };
            await onSubmit(command);
        } else {
            const command: CreateNotificationCommand = {
                objectId: formData.objectId || crypto.randomUUID(),
                title: formData.title || "",
                content: formData.content || "",
                link: formData.link || "",
                isGlobal: formData.isGlobal ?? true,
                expiredAt: expiredAt,
                notificationType: formData.notificationType ?? 0,
                notificationPriorityType: formData.notificationPriorityType ?? 1,
                recipientIds: formData.isGlobal ? undefined : formData.recipientIds,
            };
            await onSubmit(command);
        }
    };

    const handleRecipientChange = (userId: string, checked: boolean) => {
        setFormData((prev) => ({
            ...prev,
            recipientIds: checked
                ? [...(prev.recipientIds || []), userId]
                : (prev.recipientIds || []).filter((id) => id !== userId),
        }));
    };

    const isDisabled = loading;

    return (
        <form id="notification-form" onSubmit={handleSubmit} className="space-y-4">
            <div className="space-y-4">
                {/* Title */}
                <div>
                    <Label htmlFor="title">{t("form.title")} *</Label>
                    <div className="relative">
                        <Input
                            id="title"
                            value={formData.title || ""}
                            onChange={(e) =>
                                setFormData((prev) => ({ ...prev, title: e.target.value }))
                            }
                            required
                            disabled={isDisabled}
                            className={`mt-1 ${isDuplicate ? "border-red-500 focus-visible:ring-red-500" : ""}`}
                            placeholder={t("form.titlePlaceholder")}
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

                    {/* Matching notifications list */}
                    {matchingNotifications.length > 0 && !isDuplicate && (
                        <div className="mt-2 p-2 bg-slate-50 dark:bg-slate-800 rounded-md border border-slate-200 dark:border-slate-700">
                            <p className="text-xs text-slate-500 dark:text-slate-400 mb-1">
                                {t("form.similarNotifications")}:
                            </p>
                            <ul className="space-y-0.5">
                                {matchingNotifications.map((notif) => (
                                    <li key={notif.id} className="text-sm text-slate-700 dark:text-slate-300">
                                        • {notif.title}
                                    </li>
                                ))}
                            </ul>
                        </div>
                    )}
                </div>

                {/* Content */}
                <div>
                    <Label htmlFor="content">{t("form.content")} *</Label>
                    <textarea
                        id="content"
                        value={formData.content || ""}
                        onChange={(e) =>
                            setFormData((prev) => ({ ...prev, content: e.target.value }))
                        }
                        required
                        disabled={isDisabled}
                        className="mt-1 flex min-h-[80px] w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
                        rows={3}
                        placeholder={t("form.contentPlaceholder")}
                    />
                </div>

                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                    {/* Link */}
                    <div>
                        <Label htmlFor="link">{t("form.link")}</Label>
                        <Input
                            id="link"
                            value={formData.link || ""}
                            onChange={(e) =>
                                setFormData((prev) => ({ ...prev, link: e.target.value }))
                            }
                            disabled={isDisabled}
                            className="mt-1"
                            placeholder={t("form.linkPlaceholder")}
                        />
                    </div>

                    {/* ExpiredAt */}
                    <div>
                        <Label htmlFor="expiredAt">{t("form.expiredAt")}</Label>
                        <Input
                            id="expiredAt"
                            type="datetime-local"
                            value={formData.expiredAt ? formData.expiredAt.slice(0, 16) : ""}
                            onChange={(e) =>
                                setFormData((prev) => ({ ...prev, expiredAt: e.target.value ? new Date(e.target.value).toISOString() : null }))
                            }
                            disabled={isDisabled}
                            className="mt-1"
                        />
                    </div>
                </div>

                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                    {/* NotificationType */}
                    <div>
                        <Label htmlFor="notificationType">{t("form.type")}</Label>
                        <select
                            id="notificationType"
                            value={formData.notificationType ?? 0}
                            onChange={(e) =>
                                setFormData((prev) => ({ ...prev, notificationType: parseInt(e.target.value) }))
                            }
                            disabled={isDisabled}
                            className="mt-1 flex h-9 w-full rounded-md border border-input bg-background text-foreground px-3 py-1 text-sm shadow-sm transition-colors focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-50"
                        >
                            {NotificationTypes.map((type) => (
                                <option key={type.value} value={type.value}>
                                    {type.label}
                                </option>
                            ))}
                        </select>
                    </div>

                    {/* Priority */}
                    <div>
                        <Label htmlFor="priority">{t("form.priority")}</Label>
                        <select
                            id="priority"
                            value={formData.notificationPriorityType ?? 1}
                            onChange={(e) =>
                                setFormData((prev) => ({ ...prev, notificationPriorityType: parseInt(e.target.value) }))
                            }
                            disabled={isDisabled}
                            className="mt-1 flex h-9 w-full rounded-md border border-input bg-background text-foreground px-3 py-1 text-sm shadow-sm transition-colors focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-50"
                        >
                            {PriorityTypes.map((type) => (
                                <option key={type.value} value={type.value}>
                                    {type.label}
                                </option>
                            ))}
                        </select>
                    </div>
                </div>

                {/* IsGlobal - Only show when creating */}
                {!isEditMode && (
                    <div className="flex items-center space-x-2">
                        <input
                            type="checkbox"
                            id="isGlobal"
                            checked={formData.isGlobal}
                            onChange={(e) =>
                                setFormData((prev) => ({ ...prev, isGlobal: e.target.checked }))
                            }
                            disabled={isDisabled}
                            className="h-4 w-4 rounded border-gray-300 text-primary focus:ring-primary disabled:cursor-not-allowed disabled:opacity-50"
                        />
                        <Label htmlFor="isGlobal" className="cursor-pointer">
                            {t("form.isGlobal")}
                        </Label>
                    </div>
                )}

                {/* Recipients - Only show when not global and creating */}
                {!isEditMode && !formData.isGlobal && (
                    <div>
                        <Label>{t("form.recipients")} *</Label>
                        {loadingUsers ? (
                            <div className="mt-2 flex items-center gap-2 text-sm text-slate-500">
                                <Loader2 className="h-4 w-4 animate-spin" />
                                {t("form.loadingUsers")}
                            </div>
                        ) : (
                            <div className="mt-2 max-h-48 overflow-y-auto border border-input rounded-md p-2 space-y-2">
                                {users.map((user) => (
                                    <div key={user.id} className="flex items-center space-x-2">
                                        <input
                                            type="checkbox"
                                            id={`user-${user.id}`}
                                            checked={formData.recipientIds?.includes(user.id || "") || false}
                                            onChange={(e) =>
                                                handleRecipientChange(user.id || "", e.target.checked)
                                            }
                                            disabled={isDisabled}
                                            className="h-4 w-4 rounded border-gray-300 text-primary focus:ring-primary disabled:cursor-not-allowed disabled:opacity-50"
                                        />
                                        <Label htmlFor={`user-${user.id}`} className="cursor-pointer text-sm">
                                            {user.fullName || user.email} ({user.email})
                                        </Label>
                                    </div>
                                ))}
                            </div>
                        )}
                        {!formData.isGlobal && (formData.recipientIds?.length || 0) === 0 && (
                            <p className="mt-1 text-xs text-amber-600 dark:text-amber-400">
                                {t("form.recipientsRequired")}
                            </p>
                        )}
                    </div>
                )}
            </div>
        </form>
    );
}
