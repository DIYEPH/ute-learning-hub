"use client";

import { useCallback } from "react";
import {
    getApiNotification,
    postApiNotification,
    putApiNotificationById,
    deleteApiNotificationById,
} from "@/src/api/database/sdk.gen";
import type {
    GetApiNotificationData,
    GetApiNotificationResponse,
    CreateNotificationCommand,
    UpdateNotificationCommandRequest,
    NotificationDto,
} from "@/src/api/database/types.gen";
import { useCrud } from "./use-crud";

export function useNotifications() {
    const crud = useCrud<NotificationDto, CreateNotificationCommand, UpdateNotificationCommandRequest, GetApiNotificationData["query"]>({
        fetchAll: async (params) => {
            const response = await getApiNotification({ query: params });
            return (response as unknown as { data: GetApiNotificationResponse })?.data || response as GetApiNotificationResponse;
        },
        create: async (data) => {
            const response = await postApiNotification({ body: data });
            return (response as unknown as { data: NotificationDto })?.data || response as NotificationDto;
        },
        update: async (id, data) => {
            const response = await putApiNotificationById({ path: { id }, body: data });
            return (response as unknown as { data: NotificationDto })?.data || response as NotificationDto;
        },
        delete: async (id) => {
            await deleteApiNotificationById({ path: { id } });
        },
        errorMessages: {
            fetch: "Không thể tải danh sách thông báo",
            create: "Không thể tạo thông báo",
            update: "Không thể cập nhật thông báo",
            delete: "Không thể xóa thông báo",
        },
    });

    // Check if notification title exists (for duplicate check)
    const checkTitleExists = useCallback(
        async (title: string, excludeId?: string): Promise<boolean> => {
            try {
                // Fetch all notifications and filter client-side since API doesn't support SearchTerm
                const response = await getApiNotification({ query: { Page: 1, PageSize: 100 } });
                const data = (response as unknown as { data: GetApiNotificationResponse })?.data || response as GetApiNotificationResponse;
                const items = data?.items || [];

                return items.some(
                    (item) =>
                        item.title?.toLowerCase() === title.toLowerCase() &&
                        item.id !== excludeId
                );
            } catch {
                return false;
            }
        },
        []
    );

    return {
        // CRUD operations
        fetchNotifications: crud.fetchItems,
        createNotification: crud.createItem,
        updateNotification: crud.updateItem,
        deleteNotification: crud.deleteItem,

        // State
        items: crud.items,
        totalCount: crud.totalCount,
        loading: crud.loading,
        error: crud.error,

        // Duplicate check
        checkTitleExists,
    };
}
