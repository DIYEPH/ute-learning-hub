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
      const formDataToSend = new FormData();
      
      // Add files
      if (data.files && data.files.length > 0) {
        data.files.forEach((file) => {
          formDataToSend.append("Files", file);
        });
      }

      // Add form fields
      formDataToSend.append("DocumentName", data.documentName || "");
      formDataToSend.append("Description", data.description || "");
      formDataToSend.append("AuthorName", data.authorName || "");
      formDataToSend.append("DescriptionAuthor", data.descriptionAuthor || "");
      
      if (data.subjectId) {
        formDataToSend.append("SubjectId", data.subjectId);
      }
      if (data.typeId) {
        formDataToSend.append("TypeId", data.typeId);
      }
      
      if (data.tagIds && data.tagIds.length > 0) {
        data.tagIds.forEach((tagId) => {
          formDataToSend.append("TagIds", tagId);
        });
      }
      
      formDataToSend.append("IsDownload", (data.isDownload ?? true).toString());
      formDataToSend.append("Visibility", data.visibility || "Public");

      const token = getBearerToken();
      const response = await axios.post("/api/Document", formDataToSend, {
        headers: {
          "Content-Type": "multipart/form-data",
          ...(token && { Authorization: token }),
        },
      });

      if (response.data) {
        router.push(`/documents/${response.data.id}`);
      }
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
