"use client";

import { useState, useCallback } from "react";
import {
  getApiUser,
  getApiUserById,
  putApiUserById,
  postApiUserByIdBan,
  postApiUserByIdUnban,
  putApiUserByIdTrustScore,
} from "@/src/api";
import type {
  GetApiUserData,
  GetApiUserResponse,
  GetApiUserByIdResponse,
  PutApiUserByIdResponse,
  PutApiUserByIdTrustScoreResponse,
  UpdateUserRequest,
  BanUserCommand,
  ManageTrustScoreRequest,
} from "@/src/api/database/types.gen";

export function useUsers() {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetchUsers = useCallback(
    async (params?: GetApiUserData["query"]): Promise<GetApiUserResponse | null> => {
      setLoading(true);
      setError(null);
      try {
        const response = await getApiUser({ query: params });
        return (response as any)?.data || response;
      } catch (err: any) {
        const errorMessage =
          err?.response?.data?.message || err?.message || "Không thể tải danh sách người dùng";
        setError(errorMessage);
        throw err;
      } finally {
        setLoading(false);
      }
    },
    []
  );

  const fetchUserById = useCallback(
    async (id: string): Promise<GetApiUserByIdResponse | null> => {
      setLoading(true);
      setError(null);
      try {
        const response = await getApiUserById({ path: { id } });
        // @hey-api/client-axios returns { data: ... } structure
        return (response as any)?.data || response;
      } catch (err: any) {
        const errorMessage =
          err?.response?.data?.message || err?.message || "Không thể tải thông tin người dùng";
        setError(errorMessage);
        throw err;
      } finally {
        setLoading(false);
      }
    },
    []
  );

  const updateUser = useCallback(
    async (id: string, data: UpdateUserRequest): Promise<PutApiUserByIdResponse | null> => {
      setLoading(true);
      setError(null);
      try {
        const response = await putApiUserById({
          path: { id },
          body: data,
        });
        // @hey-api/client-axios returns { data: ... } structure
        return (response as any)?.data || response;
      } catch (err: any) {
        const errorMessage =
          err?.response?.data?.message || err?.message || "Không thể cập nhật người dùng";
        setError(errorMessage);
        throw err;
      } finally {
        setLoading(false);
      }
    },
    []
  );

  const banUser = useCallback(
    async (id: string, banUntil?: string | null): Promise<void> => {
      setLoading(true);
      setError(null);
      try {
        const command: BanUserCommand = {
          userId: id,
          banUntil: banUntil || null,
        };
        await postApiUserByIdBan({
          path: { id },
          body: command,
        });
      } catch (err: any) {
        const errorMessage =
          err?.response?.data?.message || err?.message || "Không thể khóa người dùng";
        setError(errorMessage);
        throw err;
      } finally {
        setLoading(false);
      }
    },
    []
  );

  const unbanUser = useCallback(
    async (id: string): Promise<void> => {
      setLoading(true);
      setError(null);
      try {
        await postApiUserByIdUnban({
          path: { id },
        });
      } catch (err: any) {
        const errorMessage =
          err?.response?.data?.message || err?.message || "Không thể mở khóa người dùng";
        setError(errorMessage);
        throw err;
      } finally {
        setLoading(false);
      }
    },
    []
  );

  const updateTrustScore = useCallback(
    async (
      id: string,
      trustScore: number,
      reason?: string | null
    ): Promise<PutApiUserByIdTrustScoreResponse | null> => {
      setLoading(true);
      setError(null);
      try {
        const command: ManageTrustScoreRequest = {
          trustScore,
          reason: reason || null,
        };
        const response = await putApiUserByIdTrustScore({
          path: { id },
          body: command,
        });
        // @hey-api/client-axios returns { data: ... } structure
        return (response as any)?.data || response;
      } catch (err: any) {
        const errorMessage =
          err?.response?.data?.message || err?.message || "Không thể cập nhật điểm tin cậy";
        setError(errorMessage);
        throw err;
      } finally {
        setLoading(false);
      }
    },
    []
  );

  return {
    fetchUsers,
    fetchUserById,
    updateUser,
    banUser,
    unbanUser,
    updateTrustScore,
    loading,
    error,
  };
}

