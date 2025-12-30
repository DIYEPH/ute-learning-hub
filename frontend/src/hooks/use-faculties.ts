"use client";

import { useCallback } from "react";
import {
  getApiFaculty,
  getApiFacultyById,
  postApiFaculty,
  putApiFacultyById,
  deleteApiFacultyById,
} from "@/src/api";
import type {
  GetApiFacultyData,
  GetApiFacultyResponse,
  CreateFacultyCommand,
  UpdateFacultyCommandRequest,
  FacultyDetailDto,
} from "@/src/api/database/types.gen";
import { useCrud } from "./use-crud";
import { useUploadLogo } from "./use-upload-logo";

export function useFaculties() {
  const { uploadLogo, uploading: uploadingLogo, error: uploadError } = useUploadLogo();

  const crud = useCrud<FacultyDetailDto, CreateFacultyCommand, UpdateFacultyCommandRequest, GetApiFacultyData["query"]>({
    fetchAll: async (params) => {
      const response = await getApiFaculty({ query: params });
      return (response as unknown as { data: GetApiFacultyResponse })?.data || response as GetApiFacultyResponse;
    },
    fetchById: async (id) => {
      const response = await getApiFacultyById({ path: { id } });
      return (response as unknown as { data: FacultyDetailDto })?.data || response as FacultyDetailDto;
    },
    create: async (data) => {
      const response = await postApiFaculty({ body: data });
      return (response as unknown as { data: FacultyDetailDto })?.data || response as FacultyDetailDto;
    },
    update: async (id, data) => {
      const response = await putApiFacultyById({ path: { id }, body: data });
      return (response as unknown as { data: FacultyDetailDto })?.data || response as FacultyDetailDto;
    },
    delete: async (id) => {
      await deleteApiFacultyById({ path: { id } });
    },
    errorMessages: {
      fetch: "Không thể tải danh sách khoa",
      fetchById: "Không thể tải thông tin khoa",
      create: "Không thể tạo khoa",
      update: "Không thể cập nhật khoa",
      delete: "Không thể xóa khoa",
    },
  });

  // Check if faculty name exists (for duplicate check)
  const checkNameExists = useCallback(
    async (name: string, excludeId?: string): Promise<boolean> => {
      try {
        const response = await getApiFaculty({ query: { SearchTerm: name, Page: 1, PageSize: 10 } });
        const data = (response as unknown as { data: GetApiFacultyResponse })?.data || response as GetApiFacultyResponse;
        const items = data?.items || [];

        return items.some(
          (item) =>
            item.facultyName?.toLowerCase() === name.toLowerCase() &&
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
    fetchFaculties: crud.fetchItems,
    fetchFacultyById: crud.fetchItemById,
    createFaculty: crud.createItem,
    updateFaculty: crud.updateItem,
    deleteFaculty: crud.deleteItem,

    // State
    items: crud.items,
    totalCount: crud.totalCount,
    loading: crud.loading,
    error: crud.error,

    // Upload
    uploadLogo,
    uploadingLogo,
    uploadError,

    // Duplicate check
    checkNameExists,
  };
}
