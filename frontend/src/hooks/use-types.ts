"use client";

import { useCallback } from "react";
import {
  getApiType,
  getApiTypeById,
  postApiType,
  putApiTypeById,
  deleteApiTypeById,
} from "@/src/api";
import type {
  GetApiTypeData,
  GetApiTypeResponse,
  CreateTypeCommand,
  UpdateTypeCommandRequest,
  TypeDto,
} from "@/src/api/database/types.gen";
import { useCrud } from "./use-crud";

export function useTypes() {
  const crud = useCrud<TypeDto, CreateTypeCommand, UpdateTypeCommandRequest, GetApiTypeData["query"]>({
    fetchAll: async (params) => {
      const response = await getApiType({ query: params });
      return (response as unknown as { data: GetApiTypeResponse })?.data || response as GetApiTypeResponse;
    },
    fetchById: async (id) => {
      const response = await getApiTypeById({ path: { id } });
      return (response as unknown as { data: TypeDto })?.data || response as TypeDto;
    },
    create: async (data) => {
      const response = await postApiType({ body: data });
      return (response as unknown as { data: TypeDto })?.data || response as TypeDto;
    },
    update: async (id, data) => {
      const response = await putApiTypeById({ path: { id }, body: data });
      return (response as unknown as { data: TypeDto })?.data || response as TypeDto;
    },
    delete: async (id) => {
      await deleteApiTypeById({ path: { id } });
    },
    errorMessages: {
      fetch: "Không thể tải danh sách loại",
      fetchById: "Không thể tải thông tin loại",
      create: "Không thể tạo loại",
      update: "Không thể cập nhật loại",
      delete: "Không thể xóa loại",
    },
  });

  // Check if type name exists (for duplicate check)
  const checkNameExists = useCallback(
    async (name: string, excludeId?: string): Promise<boolean> => {
      try {
        const response = await getApiType({ query: { SearchTerm: name, Page: 1, PageSize: 10 } });
        const data = (response as unknown as { data: GetApiTypeResponse })?.data || response as GetApiTypeResponse;
        const items = data?.items || [];

        return items.some(
          (item) =>
            item.typeName?.toLowerCase() === name.toLowerCase() &&
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
    fetchTypes: crud.fetchItems,
    fetchTypeById: crud.fetchItemById,
    createType: crud.createItem,
    updateType: crud.updateItem,
    deleteType: crud.deleteItem,

    // State
    items: crud.items,
    totalCount: crud.totalCount,
    loading: crud.loading,
    error: crud.error,

    // Duplicate check
    checkNameExists,
  };
}
