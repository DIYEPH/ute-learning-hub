"use client";

import { useCallback } from "react";
import {
    getApiConversation,
    getApiConversationById,
    putApiConversationById,
    deleteApiConversationById,
} from "@/src/api/database/sdk.gen";
import type {
    ConversationDto,
    ConversationDetailDto,
    PagedResponseOfConversationDto,
    GetApiConversationData,
    UpdateConversationCommandRequest,
} from "@/src/api/database/types.gen";
import { useCrud } from "./use-crud";

export function useConversations() {
    const crud = useCrud<ConversationDto, object, UpdateConversationCommandRequest, GetApiConversationData["query"]>({
        fetchAll: async (params) => {
            const response = await getApiConversation({ query: params });
            return (response as unknown as { data: PagedResponseOfConversationDto })?.data || response as PagedResponseOfConversationDto;
        },
        create: async () => {
            // Admin cannot create conversations
            throw new Error("Not supported");
        },
        update: async (id, command) => {
            const response = await putApiConversationById({
                path: { id },
                body: command,
            });
            return (response as unknown as { data: ConversationDto })?.data || response as ConversationDto;
        },
        delete: async (id) => {
            await deleteApiConversationById({ path: { id } });
        },
        errorMessages: {
            fetch: "Không thể tải danh sách cuộc trò chuyện",
            update: "Không thể cập nhật cuộc trò chuyện",
            delete: "Không thể xóa cuộc trò chuyện",
        },
    });

    // Fetch single conversation by ID
    const fetchConversationById = useCallback(
        async (id: string): Promise<ConversationDetailDto | null> => {
            try {
                const response = await getApiConversationById({ path: { id } });
                return (response as unknown as { data: ConversationDetailDto })?.data || response as ConversationDetailDto;
            } catch {
                return null;
            }
        },
        []
    );

    // Check if conversation name exists (for duplicate check)
    const checkNameExists = useCallback(
        async (name: string, excludeId?: string): Promise<boolean> => {
            try {
                const response = await getApiConversation({ query: { SearchTerm: name, Page: 1, PageSize: 10 } });
                const data = (response as unknown as { data: PagedResponseOfConversationDto })?.data || response as PagedResponseOfConversationDto;
                const items = data?.items || [];

                return items.some(
                    (item) =>
                        item.conversationName?.toLowerCase() === name.toLowerCase() &&
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
        fetchConversations: crud.fetchItems,
        fetchConversationById,
        updateConversation: crud.updateItem,
        deleteConversation: crud.deleteItem,

        // State
        items: crud.items,
        totalCount: crud.totalCount,
        loading: crud.loading,
        error: crud.error,

        // Duplicate check
        checkNameExists,
    };
}
