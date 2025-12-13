"use client";

import { useCallback } from "react";
import {
    getApiDocument,
    getApiDocumentById,
    putApiDocumentById,
    deleteApiDocumentById,
    postApiDocumentFilesByFileIdReview,
} from "@/src/api/database/sdk.gen";
import type {
    DocumentDto,
    DocumentDetailDto,
    GetApiDocumentData,
    UpdateDocumentCommand,
} from "@/src/api/database/types.gen";
import type { ReviewDocumentFileCommand } from "@/src/components/admin/documents/review-modal";
import { useCrud } from "./use-crud";

interface PagedResponse<T> {
    items?: T[];
    totalCount?: number;
    page?: number;
    pageSize?: number;
}

export function useDocuments() {
    const crud = useCrud<DocumentDto, object, UpdateDocumentCommand, GetApiDocumentData["query"]>({
        fetchAll: async (params) => {
            const response = await getApiDocument({ query: params });
            return (response as unknown as { data: PagedResponse<DocumentDto> })?.data || response as PagedResponse<DocumentDto>;
        },
        create: async () => {
            // Admin cannot create documents - documents are created by users uploading files
            throw new Error("Not supported");
        },
        update: async (id, command) => {
            const response = await putApiDocumentById({
                path: { id },
                body: { ...command, id },
            });
            return (response as unknown as { data: DocumentDto })?.data || response as DocumentDto;
        },
        delete: async (id) => {
            await deleteApiDocumentById({ path: { id } });
        },
        errorMessages: {
            fetch: "Không thể tải danh sách tài liệu",
            update: "Không thể cập nhật tài liệu",
            delete: "Không thể xóa tài liệu",
        },
    });

    // Fetch single document by ID (includes files)
    const fetchDocumentById = useCallback(
        async (id: string): Promise<DocumentDetailDto | null> => {
            try {
                const response = await getApiDocumentById({ path: { id } });
                return (response as unknown as { data: DocumentDetailDto })?.data || response as DocumentDetailDto;
            } catch {
                return null;
            }
        },
        []
    );

    // Review document file (approve/reject)
    const reviewDocumentFile = useCallback(
        async (command: ReviewDocumentFileCommand & { fileId: string }): Promise<boolean> => {
            try {
                await postApiDocumentFilesByFileIdReview({
                    path: { fileId: command.fileId },
                    body: command,
                });
                return true;
            } catch {
                return false;
            }
        },
        []
    );

    return {
        // CRUD operations
        fetchDocuments: crud.fetchItems,
        fetchDocumentById,
        updateDocument: crud.updateItem,
        deleteDocument: crud.deleteItem,
        reviewDocumentFile,

        // State
        items: crud.items,
        totalCount: crud.totalCount,
        loading: crud.loading,
        error: crud.error,
    };
}
