"use client";

import { useCallback, useState } from "react";
import { postApiFile } from "@/src/api";
import type { FileDto } from "@/src/api/database/types.gen";
import { useNotification } from "@/src/components/providers/notification-provider";

export interface UploadedFile extends FileDto { }

interface UseFileUploadReturn {
  uploadFile: (file: File, category?: string) => Promise<UploadedFile>;
  uploadFiles: (files: File[], category?: string) => Promise<UploadedFile[]>;
  uploading: boolean;
  error: string | null;
}

export function useFileUpload(): UseFileUploadReturn {
  const [uploading, setUploading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const { error: notifyError } = useNotification();

  const uploadFile = useCallback(
    async (file: File, category?: string): Promise<UploadedFile> => {
      setUploading(true);
      setError(null);

      try {
        const response = await postApiFile<true>({
          query: category ? { category } : undefined,
          // hey-api formDataBodySerializer sẽ tự chuyển { file } -> multipart/form-data
          body: { file },
          throwOnError: true,
        });

        const data = (response.data ?? response) as FileDto | undefined;
        if (!data?.id) {
          throw new Error("Phản hồi upload file không hợp lệ");
        }

        return data;
      } catch (err: any) {
        const message =
          err?.response?.data?.message ||
          err?.response?.data ||
          err?.message ||
          "Không thể upload file";
        setError(message);
        notifyError(message);
        throw err;
      } finally {
        setUploading(false);
      }
    },
    [notifyError],
  );

  const uploadFiles = useCallback(
    async (files: File[], category?: string): Promise<UploadedFile[]> => {
      const results: UploadedFile[] = [];
      for (const file of files) {
        // Upload tuần tự để đơn giản, nếu cần có thể tối ưu thành Promise.all
        // nhưng vẫn reuse cùng state loading/error
        // eslint-disable-next-line no-await-in-loop
        const uploaded = await uploadFile(file, category);
        results.push(uploaded);
      }
      return results;
    },
    [uploadFile],
  );

  return { uploadFile, uploadFiles, uploading, error };
}


