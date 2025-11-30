"use client";

import { useState, useCallback } from "react";
import axios from "axios";
import { getBearerToken } from "@/src/api/client";

interface UseUploadLogoReturn {
  uploadLogo: (file: File) => Promise<string | null>;
  uploading: boolean;
  error: string | null;
}

export function useUploadLogo(): UseUploadLogoReturn {
  const [uploading, setUploading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const uploadLogo = useCallback(async (file: File): Promise<string | null> => {
    setUploading(true);
    setError(null);

    try {
      const formData = new FormData();
      formData.append("file", file);

      const token = getBearerToken();

      const response = await axios.post("/api/faculty/upload-logo", formData, {
        headers: {
          "Content-Type": "multipart/form-data",
          ...(token && { Authorization: token }),
        },
      });

      if (response.data?.url) {
        return response.data.url;
      }

      return null;
    } catch (err: any) {
      const errorMessage =
        err?.response?.data?.message ||
        err?.response?.data ||
        err?.message ||
        "Không thể upload file";
      setError(errorMessage);
      throw err;
    } finally {
      setUploading(false);
    }
  }, []);

  return { uploadLogo, uploading, error };
}

