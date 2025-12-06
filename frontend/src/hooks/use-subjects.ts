"use client";

import { useCallback } from "react";
import {
  getApiSubject,
  getApiSubjectById,
  postApiSubject,
  putApiSubjectById,
  deleteApiSubjectById,
} from "@/src/api/database/sdk.gen";
import type {
  GetApiSubjectData,
  GetApiSubjectResponse,
  CreateSubjectCommand,
  UpdateSubjectCommand,
  SubjectDto2,
} from "@/src/api/database/types.gen";
import { useCrud } from "./use-crud";

export function useSubjects() {
  const crud = useCrud<SubjectDto2, CreateSubjectCommand, UpdateSubjectCommand, GetApiSubjectData["query"]>({
    fetchAll: async (params) => {
      const response = await getApiSubject({ query: params });
      return (response as unknown as { data: GetApiSubjectResponse })?.data || response as GetApiSubjectResponse;
    },
    fetchById: async (id) => {
      const response = await getApiSubjectById({ path: { id } });
      return (response as unknown as { data: SubjectDto2 })?.data || response as SubjectDto2;
    },
    create: async (data) => {
      const response = await postApiSubject({ body: data });
      return (response as unknown as { data: SubjectDto2 })?.data || response as SubjectDto2;
    },
    update: async (id, data) => {
      const response = await putApiSubjectById({ path: { id }, body: data });
      return (response as unknown as { data: SubjectDto2 })?.data || response as SubjectDto2;
    },
    delete: async (id) => {
      await deleteApiSubjectById({ path: { id } });
    },
    errorMessages: {
      fetch: "Không thể tải danh sách môn học",
      fetchById: "Không thể tải thông tin môn học",
      create: "Không thể tạo môn học",
      update: "Không thể cập nhật môn học",
      delete: "Không thể xóa môn học",
    },
  });

  // Check if subject name exists (for duplicate check)
  const checkNameExists = useCallback(
    async (name: string, excludeId?: string): Promise<boolean> => {
      try {
        const response = await getApiSubject({ query: { SearchTerm: name, Page: 1, PageSize: 10 } });
        const data = (response as unknown as { data: GetApiSubjectResponse })?.data || response as GetApiSubjectResponse;
        const items = data?.items || [];

        return items.some(
          (item) =>
            item.subjectName?.toLowerCase() === name.toLowerCase() &&
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
    fetchSubjects: crud.fetchItems,
    fetchSubjectById: crud.fetchItemById,
    createSubject: crud.createItem,
    updateSubject: crud.updateItem,
    deleteSubject: crud.deleteItem,

    // State
    items: crud.items,
    totalCount: crud.totalCount,
    loading: crud.loading,
    error: crud.error,

    // Duplicate check
    checkNameExists,
  };
}
