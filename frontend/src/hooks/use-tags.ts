"use client";

import { useCallback } from "react";
import {
    getApiTag,
    getApiTagById,
    postApiTag,
    putApiTagById,
    deleteApiTagById,
} from "@/src/api/database/sdk.gen";
import type {
    GetApiTagData,
    GetApiTagResponse,
    TagDto,
    TagDetailDto,
} from "@/src/api/database/types.gen";
import { useCrud } from "./use-crud";

// Types not exported from SDK, defined based on backend API
export interface CreateTagCommand {
    tagName: string;
}

export interface UpdateTagCommand {
    id?: string;
    tagName: string;
}

export function useTags() {
    const crud = useCrud<TagDto, CreateTagCommand, UpdateTagCommand, GetApiTagData["query"]>({
        fetchAll: async (params) => {
            const response = await getApiTag({ query: params });
            return (response as unknown as { data: GetApiTagResponse })?.data || response as GetApiTagResponse;
        },
        fetchById: async (id) => {
            const response = await getApiTagById({ path: { id } });
            return (response as unknown as { data: TagDetailDto })?.data || response as TagDetailDto;
        },
        create: async (data) => {
            const response = await postApiTag({ body: data });
            return (response as unknown as { data: TagDetailDto })?.data || response as TagDetailDto;
        },
        update: async (id, data) => {
            const response = await putApiTagById({ path: { id }, body: data });
            return (response as unknown as { data: TagDetailDto })?.data || response as TagDetailDto;
        },
        delete: async (id) => {
            await deleteApiTagById({ path: { id } });
        },
        errorMessages: {
            fetch: "Không thể tải danh sách tag",
            fetchById: "Không thể tải thông tin tag",
            create: "Không thể tạo tag",
            update: "Không thể cập nhật tag",
            delete: "Không thể xóa tag",
        },
    });

    const checkNameExists = useCallback(
        async (name: string, excludeId?: string): Promise<boolean> => {
            try {
                const response = await getApiTag({ query: { SearchTerm: name, Page: 1, PageSize: 10 } });
                const data = (response as unknown as { data: GetApiTagResponse })?.data || response as GetApiTagResponse;
                const items = data?.items || [];

                return items.some(
                    (item) =>
                        item.tagName?.toLowerCase() === name.toLowerCase() &&
                        item.id !== excludeId
                );
            } catch {
                return false;
            }
        },
        []
    );

    return {
        fetchTags: crud.fetchItems,
        fetchTagById: crud.fetchItemById,
        createTag: crud.createItem,
        updateTag: crud.updateItem,
        deleteTag: crud.deleteItem,
        items: crud.items,
        totalCount: crud.totalCount,
        loading: crud.loading,
        error: crud.error,
        checkNameExists,
    };
}
