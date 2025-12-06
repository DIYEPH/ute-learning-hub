"use client";

import { useState, useCallback } from "react";
import { useFileUpload } from "@/src/hooks/use-file-upload";
import { getFileUrlById } from "@/src/lib/file-url";

interface UseUploadLogoReturn {
  uploadLogo: (file: File) => Promise<string | null>;
  uploading: boolean;
  error: string | null;
}

export function useUploadLogo(): UseUploadLogoReturn {
  const { uploadFile, uploading, error } = useFileUpload();
  const [localError, setLocalError] = useState<string | null>(null);

  const uploadLogo = useCallback(
    async (file: File): Promise<string | null> => {
      setLocalError(null);
      try {
        // Không truyền category để FileController dùng rule ảnh mặc định
        const uploaded = await uploadFile(file);
        return uploaded.id ? getFileUrlById(uploaded.id) : null;
      } catch (err: any) {
        const message =
          err?.response?.data?.message ||
          err?.response?.data ||
          err?.message ||
          "Không thể upload file";
        setLocalError(message);
        throw err;
      }
    },
    [uploadFile],
  );

  return { uploadLogo, uploading, error: error ?? localError };
}
