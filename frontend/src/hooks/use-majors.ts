"use client";

import { useState, useCallback } from "react";
import {
  getApiMajor,
  getApiMajorById,
  postApiMajor,
  putApiMajorById,
  deleteApiMajorById,
} from "@/src/api/database/sdk.gen";
import type {
  GetApiMajorData,
  GetApiMajorResponse,
  GetApiMajorByIdResponse,
  PostApiMajorResponse,
  PutApiMajorByIdResponse,
  CreateMajorCommand,
  UpdateMajorCommand,
} from "@/src/api/database/types.gen";

export function useMajors() {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetchMajors = useCallback(
    async (params?: GetApiMajorData["query"]): Promise<GetApiMajorResponse | null> => {
      setLoading(true);
      setError(null);
      try {
        const response = await getApiMajor({ query: params });
        return (response as any)?.data || response;
      } catch (err: any) {
        const errorMessage =
          err?.response?.data?.message || err?.message || "Không thể tải danh sách ngành";
        setError(errorMessage);
        throw err;
      } finally {
        setLoading(false);
      }
    },
    []
  );

  const fetchMajorById = useCallback(
    async (id: string): Promise<GetApiMajorByIdResponse | null> => {
      setLoading(true);
      setError(null);
      try {
        const response = await getApiMajorById({ path: { id } });
        return (response as any)?.data || response;
      } catch (err: any) {
        const errorMessage =
          err?.response?.data?.message || err?.message || "Không thể tải thông tin ngành";
        setError(errorMessage);
        throw err;
      } finally {
        setLoading(false);
      }
    },
    []
  );

  const createMajor = useCallback(
    async (data: CreateMajorCommand): Promise<PostApiMajorResponse | null> => {
      setLoading(true);
      setError(null);
      try {
        const response = await postApiMajor({ body: data });
        return (response as any)?.data || response;
      } catch (err: any) {
        const errorMessage =
          err?.response?.data?.message || err?.message || "Không thể tạo ngành";
        setError(errorMessage);
        throw err;
      } finally {
        setLoading(false);
      }
    },
    []
  );

  const updateMajor = useCallback(
    async (id: string, data: UpdateMajorCommand): Promise<PutApiMajorByIdResponse | null> => {
      setLoading(true);
      setError(null);
      try {
        const response = await putApiMajorById({
          path: { id },
          body: data,
        });
        return (response as any)?.data || response;
      } catch (err: any) {
        const errorMessage =
          err?.response?.data?.message || err?.message || "Không thể cập nhật ngành";
        setError(errorMessage);
        throw err;
      } finally {
        setLoading(false);
      }
    },
    []
  );

  const deleteMajor = useCallback(
    async (id: string): Promise<void> => {
      setLoading(true);
      setError(null);
      try {
        await deleteApiMajorById({ path: { id } });
      } catch (err: any) {
        const errorMessage =
          err?.response?.data?.message || err?.message || "Không thể xóa ngành";
        setError(errorMessage);
        throw err;
      } finally {
        setLoading(false);
      }
    },
    []
  );

  return {
    fetchMajors,
    fetchMajorById,
    createMajor,
    updateMajor,
    deleteMajor,
    loading,
    error,
  };
}

