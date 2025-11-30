"use client";

import { useState, useCallback } from "react";
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
  GetApiSubjectByIdResponse,
  PostApiSubjectResponse,
  PutApiSubjectByIdResponse,
  CreateSubjectCommand,
  UpdateSubjectCommand,
} from "@/src/api/database/types.gen";

export function useSubjects() {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetchSubjects = useCallback(
    async (params?: GetApiSubjectData["query"]): Promise<GetApiSubjectResponse | null> => {
      setLoading(true);
      setError(null);
      try {
        const response = await getApiSubject({ query: params });
        return (response as any)?.data || response;
      } catch (err: any) {
        const errorMessage =
          err?.response?.data?.message || err?.message || "Không thể tải danh sách môn học";
        setError(errorMessage);
        throw err;
      } finally {
        setLoading(false);
      }
    },
    []
  );

  const fetchSubjectById = useCallback(
    async (id: string): Promise<GetApiSubjectByIdResponse | null> => {
      setLoading(true);
      setError(null);
      try {
        const response = await getApiSubjectById({ path: { id } });
        return (response as any)?.data || response;
      } catch (err: any) {
        const errorMessage =
          err?.response?.data?.message || err?.message || "Không thể tải thông tin môn học";
        setError(errorMessage);
        throw err;
      } finally {
        setLoading(false);
      }
    },
    []
  );

  const createSubject = useCallback(
    async (data: CreateSubjectCommand): Promise<PostApiSubjectResponse | null> => {
      setLoading(true);
      setError(null);
      try {
        const response = await postApiSubject({ body: data });
        return (response as any)?.data || response;
      } catch (err: any) {
        const errorMessage =
          err?.response?.data?.message || err?.message || "Không thể tạo môn học";
        setError(errorMessage);
        throw err;
      } finally {
        setLoading(false);
      }
    },
    []
  );

  const updateSubject = useCallback(
    async (id: string, data: UpdateSubjectCommand): Promise<PutApiSubjectByIdResponse | null> => {
      setLoading(true);
      setError(null);
      try {
        const response = await putApiSubjectById({
          path: { id },
          body: data,
        });
        return (response as any)?.data || response;
      } catch (err: any) {
        const errorMessage =
          err?.response?.data?.message || err?.message || "Không thể cập nhật môn học";
        setError(errorMessage);
        throw err;
      } finally {
        setLoading(false);
      }
    },
    []
  );

  const deleteSubject = useCallback(
    async (id: string): Promise<void> => {
      setLoading(true);
      setError(null);
      try {
        await deleteApiSubjectById({ path: { id } });
      } catch (err: any) {
        const errorMessage =
          err?.response?.data?.message || err?.message || "Không thể xóa môn học";
        setError(errorMessage);
        throw err;
      } finally {
        setLoading(false);
      }
    },
    []
  );

  return {
    fetchSubjects,
    fetchSubjectById,
    createSubject,
    updateSubject,
    deleteSubject,
    loading,
    error,
  };
}

