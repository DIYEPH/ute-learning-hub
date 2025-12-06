"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { Button } from "@/src/components/ui/button";
import { useTranslations } from "next-intl";
import { DocumentUploadForm, type DocumentUploadFormData } from "@/src/components/documents/document-upload-form";
import { useFileUpload } from "@/src/hooks/use-file-upload";
import { postApiDocument } from "@/src/api/database/sdk.gen";
import type { CreateDocumentCommand, DocumentDetailDto } from "@/src/api/database/types.gen";

export default function UploadDocumentPage() {
  const t = useTranslations("documents");
  const tCommon = useTranslations("common");
  const router = useRouter();
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const { uploadFile } = useFileUpload();

  const handleSubmit = async (data: DocumentUploadFormData) => {
    setLoading(true);
    setError(null);

    try {
      // Upload ảnh bìa nếu có
      let coverFileId: string | null = null;
      if (data.coverFile) {
        const uploadedCover = await uploadFile(data.coverFile, "DocumentCover");
        coverFileId = uploadedCover.id ?? null;
      }

      const body: CreateDocumentCommand = {
        documentName: data.documentName || "",
        description: data.description || "",
        subjectId: data.subjectId || null,
        typeId: data.typeId!,
        authorIds:
          data.authorIds && data.authorIds.length > 0
            ? data.authorIds
            : null,
        authors:
          data.authors && data.authors.length > 0
            ? data.authors
            : null,
        tagIds:
          data.tagIds && data.tagIds.length > 0 ? data.tagIds : null,
        tagNames:
          data.tagNames && data.tagNames.length > 0 ? data.tagNames : null,
        isDownload: data.isDownload ?? true,
        visibility: (data.visibility as any) ?? 0,
        coverFileId,
      };

      const response = await postApiDocument({
        body,
      });

      const created = (response.data ?? response) as DocumentDetailDto | undefined;
      if (!created?.id) {
        throw new Error("Không thể tạo tài liệu");
      }

      router.push(`/documents/${created.id}`);
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
