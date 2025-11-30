"use client";

import { useState, useCallback } from "react";
import {
  getApiType,
  getApiTypeById,
  postApiType,
  putApiTypeById,
  deleteApiTypeById,
} from "@/src/api/database/sdk.gen";
import type {
  GetApiTypeData,
  GetApiTypeResponse,
  GetApiTypeByIdResponse,
  PostApiTypeResponse,
  PutApiTypeByIdResponse,
  CreateTypeCommand,
  UpdateTypeCommand,
} from "@/src/api/database/types.gen";

export function useTypes() {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetchTypes = useCallback(
    async (params?: GetApiTypeData["query"]): Promise<GetApiTypeResponse | null> => {
      setLoading(true);
      setError(null);
      try {
        const response = await getApiType({ query: params });
        return (response as any)?.data || response;
      } catch (err: any) {
        const errorMessage =
          err?.response?.data?.message || err?.message || "Không thể tải danh sách loại tài liệu";
        setError(errorMessage);
        throw err;
      } finally {
        setLoading(false);
      }
    },
    []
  );

  const fetchTypeById = useCallback(
    async (id: string): Promise<GetApiTypeByIdResponse | null> => {
      setLoading(true);
      setError(null);
      try {
        const response = await getApiTypeById({ path: { id } });
        return (response as any)?.data || response;
      } catch (err: any) {
        const errorMessage =
          err?.response?.data?.message || err?.message || "Không thể tải thông tin loại tài liệu";
        setError(errorMessage);
        throw err;
      } finally {
        setLoading(false);
      }
    },
    []
  );

  const createType = useCallback(
    async (data: CreateTypeCommand): Promise<PostApiTypeResponse | null> => {
      setLoading(true);
      setError(null);
      try {
        const response = await postApiType({ body: data });
        return (response as any)?.data || response;
      } catch (err: any) {
        const errorMessage =
          err?.response?.data?.message || err?.message || "Không thể tạo loại tài liệu";
        setError(errorMessage);
        throw err;
      } finally {
        setLoading(false);
      }
    },
    []
  );

  const updateType = useCallback(
    async (id: string, data: UpdateTypeCommand): Promise<PutApiTypeByIdResponse | null> => {
      setLoading(true);
      setError(null);
      try {
        const response = await putApiTypeById({
          path: { id },
          body: data,
        });
        return (response as any)?.data || response;
      } catch (err: any) {
        const errorMessage =
          err?.response?.data?.message || err?.message || "Không thể cập nhật loại tài liệu";
        setError(errorMessage);
        throw err;
      } finally {
        setLoading(false);
      }
    },
    []
  );

  const deleteType = useCallback(
    async (id: string): Promise<void> => {
      setLoading(true);
      setError(null);
      try {
        await deleteApiTypeById({ path: { id } });
      } catch (err: any) {
        const errorMessage =
          err?.response?.data?.message || err?.message || "Không thể xóa loại tài liệu";
        setError(errorMessage);
        throw err;
      } finally {
        setLoading(false);
      }
    },
    []
  );

  return {
    fetchTypes,
    fetchTypeById,
    createType,
    updateType,
    deleteType,
    loading,
    error,
  };
}

