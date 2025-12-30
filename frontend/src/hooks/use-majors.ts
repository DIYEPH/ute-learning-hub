"use client";

import { useCallback } from "react";
import {
  getApiMajor,
  getApiMajorById,
  postApiMajor,
  putApiMajorById,
  deleteApiMajorById,
} from "@/src/api";
import type {
  GetApiMajorData,
  GetApiMajorResponse,
  CreateMajorCommand,
  UpdateMajorCommandRequest,
  MajorDetailDto,
} from "@/src/api/database/types.gen";
import { useCrud } from "./use-crud";

export function useMajors() {
  const crud = useCrud<MajorDetailDto, CreateMajorCommand, UpdateMajorCommandRequest, GetApiMajorData["query"]>({
    fetchAll: async (params) => {
      const response = await getApiMajor({ query: params });
      return (response as unknown as { data: GetApiMajorResponse })?.data || response as GetApiMajorResponse;
    },
    fetchById: async (id) => {
      const response = await getApiMajorById({ path: { id } });
      return (response as unknown as { data: MajorDetailDto })?.data || response as MajorDetailDto;
    },
    create: async (data) => {
      const response = await postApiMajor({ body: data });
      return (response as unknown as { data: MajorDetailDto })?.data || response as MajorDetailDto;
    },
    update: async (id, data) => {
      const response = await putApiMajorById({ path: { id }, body: data });
      return (response as unknown as { data: MajorDetailDto })?.data || response as MajorDetailDto;
    },
    delete: async (id) => {
      await deleteApiMajorById({ path: { id } });
    },
    errorMessages: {
      fetch: "Không thể tải danh sách ngành",
      fetchById: "Không thể tải thông tin ngành",
      create: "Không thể tạo ngành",
      update: "Không thể cập nhật ngành",
      delete: "Không thể xóa ngành",
    },
  });

  // Check if major name exists (for duplicate check)
  const checkNameExists = useCallback(
    async (name: string, excludeId?: string): Promise<boolean> => {
      try {
        const response = await getApiMajor({ query: { SearchTerm: name, Page: 1, PageSize: 10 } });
        const data = (response as unknown as { data: GetApiMajorResponse })?.data || response as GetApiMajorResponse;
        const items = data?.items || [];

        return items.some(
          (item) =>
            item.majorName?.toLowerCase() === name.toLowerCase() &&
            item.id !== excludeId
        );
      } catch {
        return false;
      }
    },
    []
  );

  return {
    // CRUD operations (renamed for backward compatibility)
    fetchMajors: crud.fetchItems,
    fetchMajorById: crud.fetchItemById,
    createMajor: crud.createItem,
    updateMajor: crud.updateItem,
    deleteMajor: crud.deleteItem,

    // State
    items: crud.items,
    totalCount: crud.totalCount,
    loading: crud.loading,
    error: crud.error,

    // Duplicate check
    checkNameExists,
  };
}
