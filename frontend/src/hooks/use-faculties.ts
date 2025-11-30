"use client";

import { useState, useCallback } from "react";
import {
  getApiFaculty,
  getApiFacultyById,
  postApiFaculty,
  putApiFacultyById,
  deleteApiFacultyById,
} from "@/src/api/database/sdk.gen";
import type {
  GetApiFacultyData,
  GetApiFacultyResponse,
  GetApiFacultyByIdResponse,
  PostApiFacultyResponse,
  PutApiFacultyByIdResponse,
  CreateFacultyCommand,
  UpdateFacultyCommand,
} from "@/src/api/database/types.gen";
import { useUploadLogo } from "./use-upload-logo";

export function useFaculties() {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const { uploadLogo, uploading: uploadingLogo, error: uploadError } = useUploadLogo();

  const fetchFaculties = useCallback(
    async (params?: GetApiFacultyData["query"]): Promise<GetApiFacultyResponse | null> => {
      setLoading(true);
      setError(null);
      try {
        const response = await getApiFaculty({ query: params });
        return (response as any)?.data || response;
      } catch (err: any) {
        const errorMessage =
          err?.response?.data?.message || err?.message || "Không thể tải danh sách khoa";
        setError(errorMessage);
        throw err;
      } finally {
        setLoading(false);
      }
    },
    []
  );

  const fetchFacultyById = useCallback(
    async (id: string): Promise<GetApiFacultyByIdResponse | null> => {
      setLoading(true);
      setError(null);
      try {
        const response = await getApiFacultyById({ path: { id } });
        return (response as any)?.data || response;
      } catch (err: any) {
        const errorMessage =
          err?.response?.data?.message || err?.message || "Không thể tải thông tin khoa";
        setError(errorMessage);
        throw err;
      } finally {
        setLoading(false);
      }
    },
    []
  );

  const createFaculty = useCallback(
    async (data: CreateFacultyCommand): Promise<PostApiFacultyResponse | null> => {
      setLoading(true);
      setError(null);
      try {
        const response = await postApiFaculty({ body: data });
        return (response as any)?.data || response;
      } catch (err: any) {
        const errorMessage =
          err?.response?.data?.message || err?.message || "Không thể tạo khoa";
        setError(errorMessage);
        throw err;
      } finally {
        setLoading(false);
      }
    },
    []
  );

  const updateFaculty = useCallback(
    async (id: string, data: UpdateFacultyCommand): Promise<PutApiFacultyByIdResponse | null> => {
      setLoading(true);
      setError(null);
      try {
        const response = await putApiFacultyById({
          path: { id },
          body: data,
        });
        return (response as any)?.data || response;
      } catch (err: any) {
        const errorMessage =
          err?.response?.data?.message || err?.message || "Không thể cập nhật khoa";
        setError(errorMessage);
        throw err;
      } finally {
        setLoading(false);
      }
    },
    []
  );

  const deleteFaculty = useCallback(
    async (id: string): Promise<void> => {
      setLoading(true);
      setError(null);
      try {
        await deleteApiFacultyById({ path: { id } });
      } catch (err: any) {
        const errorMessage =
          err?.response?.data?.message || err?.message || "Không thể xóa khoa";
        setError(errorMessage);
        throw err;
      } finally {
        setLoading(false);
      }
    },
    []
  );

  return {
    fetchFaculties,
    fetchFacultyById,
    createFaculty,
    updateFaculty,
    deleteFaculty,
    uploadLogo,
    loading,
    error,
    uploadingLogo,
    uploadError,
  };
}

