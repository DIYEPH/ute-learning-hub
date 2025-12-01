"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { Button } from "@/src/components/ui/button";
import { useTranslations } from "next-intl";
import { getBearerToken } from "@/src/api/client";
import { DocumentUploadForm, type DocumentUploadFormData } from "@/src/components/documents/document-upload-form";
import axios from "axios";

export default function UploadDocumentPage() {
  const t = useTranslations("documents");
  const tCommon = useTranslations("common");
  const router = useRouter();
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async (data: DocumentUploadFormData) => {
    setLoading(true);
    setError(null);

    try {
      const apiBaseUrl = process.env.NEXT_PUBLIC_API_URL || "https://localhost:7080";
      const token = getBearerToken();
      const headers = token ? { Authorization: token } : {};

      // Chỉ tạo document (vỏ) - không upload file ở đây
      // File/chương sẽ được upload sau ở trang chi tiết document
      const documentFormData = new FormData();
      documentFormData.append("DocumentName", data.documentName || "");
      documentFormData.append("Description", data.description || "");
      
      if (data.coverFile) {
        documentFormData.append("CoverFile", data.coverFile);
      }
      
      if (data.subjectId) {
        documentFormData.append("SubjectId", data.subjectId);
      }
      if (data.typeId) {
        documentFormData.append("TypeId", data.typeId);
      }
      
      if (data.authorNames && data.authorNames.length > 0) {
        data.authorNames.forEach((authorName) => {
          documentFormData.append("AuthorNames", authorName);
        });
      }
      
      if (data.tagIds && data.tagIds.length > 0) {
        data.tagIds.forEach((tagId) => {
          documentFormData.append("TagIds", tagId);
        });
      }
      if (data.tagNames && data.tagNames.length > 0) {
        data.tagNames.forEach((tagName) => {
          documentFormData.append("TagNames", tagName);
        });
      }
      
      documentFormData.append("IsDownload", (data.isDownload ?? true).toString());
      documentFormData.append("Visibility", (data.visibility ?? 0).toString());

      const createResponse = await axios.post(
        `${apiBaseUrl}/api/Document`,
        documentFormData,
        { headers }
      );

      if (!createResponse.data?.id) {
        throw new Error("Không thể tạo tài liệu");
      }

      const documentId = createResponse.data.id;

      // Chuyển đến trang chi tiết document - ở đó user sẽ upload từng chương/file
      router.push(`/documents/${documentId}`);
    } catch (err: any) {
      const errorMessage =
        err?.response?.data?.message ||
        err?.response?.data ||
        err?.message ||
        "Không thể tạo tài liệu";
      setError(errorMessage);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="p-4 md:p-6 max-w-4xl mx-auto">
      <h1 className="text-2xl font-semibold text-foreground mb-6">
        {t("uploadTitle")}
      </h1>

      {error && (
        <div className="mb-4 p-3 text-sm text-red-600 dark:text-red-400 bg-red-50 dark:bg-red-950 rounded">
          {error}
        </div>
      )}

      <DocumentUploadForm onSubmit={handleSubmit} loading={loading} />

      <div className="flex gap-4 mt-6">
        <Button
          type="button"
          variant="outline"
          onClick={() => router.back()}
          disabled={loading}
        >
          {tCommon("cancel")}
        </Button>
        <Button
          type="button"
          onClick={() => {
            const form = document.getElementById("document-upload-form") as HTMLFormElement;
            if (form) {
              form.requestSubmit();
            }
          }}
          disabled={loading}
        >
          {loading ? tCommon("loading") : t("upload")}
        </Button>
      </div>
    </div>
  );
}
