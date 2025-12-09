"use client";

import { useCallback } from "react";
import {
    getApiAuthor,
    getApiAuthorById,
    postApiAuthor,
    putApiAuthorById,
    deleteApiAuthorById,
} from "@/src/api/database/sdk.gen";
import type {
    AuthorListDto,
    AuthorDetailDto,
    AuthorInput,
    UpdateAuthorCommand,
    GetApiAuthorData,
} from "@/src/api/database/types.gen";
import { useCrud } from "./use-crud";

interface PagedResponse<T> {
    items?: T[];
    totalCount?: number;
    page?: number;
    pageSize?: number;
}

export function useAuthors() {
    const crud = useCrud<AuthorListDto, AuthorInput, UpdateAuthorCommand, GetApiAuthorData["query"]>({
        fetchAll: async (params) => {
            const response = await getApiAuthor({ query: params });
            return (response as unknown as { data: PagedResponse<AuthorListDto> })?.data || response as PagedResponse<AuthorListDto>;
        },
        fetchById: async (id) => {
            const response = await getApiAuthorById({ path: { id } });
            return (response as unknown as { data: AuthorDetailDto })?.data || response as AuthorDetailDto;
        },
        create: async (data) => {
            const response = await postApiAuthor({ body: data });
            return (response as unknown as { data: AuthorDetailDto })?.data || response as AuthorDetailDto;
        },
        update: async (id, command) => {
            const response = await putApiAuthorById({
                path: { id },
                body: { ...command, id },
            });
            return (response as unknown as { data: AuthorDetailDto })?.data || response as AuthorDetailDto;
        },
        delete: async (id) => {
            await deleteApiAuthorById({ path: { id } });
        },
        errorMessages: {
            fetch: "Không thể tải danh sách tác giả",
            fetchById: "Không thể tải thông tin tác giả",
            create: "Không thể tạo tác giả",
            update: "Không thể cập nhật tác giả",
            delete: "Không thể xóa tác giả",
        },
    });

    // Check if author name exists (for duplicate check)
    const checkNameExists = useCallback(
        async (name: string, excludeId?: string): Promise<boolean> => {
            try {
                const response = await getApiAuthor({ query: { SearchTerm: name, Page: 1, PageSize: 10 } });
                const data = (response as unknown as { data: PagedResponse<AuthorListDto> })?.data || response as PagedResponse<AuthorListDto>;
                const items = data?.items || [];

                return items.some(
                    (item) =>
                        item.fullName?.toLowerCase() === name.toLowerCase() &&
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
        fetchAuthors: crud.fetchItems,
        fetchAuthorById: crud.fetchItemById,
        createAuthor: crud.createItem,
        updateAuthor: crud.updateItem,
        deleteAuthor: crud.deleteItem,

        // State
        items: crud.items,
        totalCount: crud.totalCount,
        loading: crud.loading,
        error: crud.error,

        // Utils
        checkNameExists,
    };
}
