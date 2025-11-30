"use client";

import { useState, useEffect } from "react";
import { useRouter } from "next/navigation";
import { Button } from "@/src/components/ui/button";
import { Input } from "@/src/components/ui/input";
import { Label } from "@/src/components/ui/label";
import { useTranslations } from "next-intl";
import { postApiDocument } from "@/src/api/database/sdk.gen";
import { getBearerToken } from "@/src/api/client";
import axios from "axios";
import { Upload, X, FileText } from "lucide-react";

export default function UploadDocumentPage() {
  const t = useTranslations("documents");
  const tCommon = useTranslations("common");
  const router = useRouter();
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const [formData, setFormData] = useState({
    documentName: "",
    description: "",
    authorName: "",
    descriptionAuthor: "",
    subjectId: "",
    typeId: "",
    tagIds: [] as string[],
    isDownload: true,
    visibility: "Public",
  });

  const [files, setFiles] = useState<File[]>([]);

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const selectedFiles = Array.from(e.target.files || []);
    setFiles((prev) => [...prev, ...selectedFiles]);
  };

  const handleRemoveFile = (index: number) => {
    setFiles((prev) => prev.filter((_, i) => i !== index));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError(null);

    try {
      const formDataToSend = new FormData();
      
      // Add files
      files.forEach((file) => {
        formDataToSend.append("Files", file);
      });

      // Add form fields
      formDataToSend.append("DocumentName", formData.documentName);
      formDataToSend.append("Description", formData.description);
      formDataToSend.append("AuthorName", formData.authorName);
      formDataToSend.append("DescriptionAuthor", formData.descriptionAuthor);
      
      if (formData.subjectId) {
        formDataToSend.append("SubjectId", formData.subjectId);
      }
      if (formData.typeId) {
        formDataToSend.append("TypeId", formData.typeId);
      }
      
      formData.tagIds.forEach((tagId) => {
        formDataToSend.append("TagIds", tagId);
      });
      
      formDataToSend.append("IsDownload", formData.isDownload.toString());
      formDataToSend.append("Visibility", formData.visibility);

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

      <form onSubmit={handleSubmit} className="space-y-6">
        <div className="space-y-2">
          <Label htmlFor="documentName">
            {t("documentName")} <span className="text-red-500">*</span>
          </Label>
          <Input
            id="documentName"
            value={formData.documentName}
            onChange={(e) =>
              setFormData({ ...formData, documentName: e.target.value })
            }
            required
            className="bg-background text-foreground"
          />
        </div>

        <div className="space-y-2">
          <Label htmlFor="description">{t("description")}</Label>
          <textarea
            id="description"
            value={formData.description}
            onChange={(e) =>
              setFormData({ ...formData, description: e.target.value })
            }
            rows={4}
            className="w-full rounded-md border border-input bg-background text-foreground px-3 py-2 text-sm shadow-sm transition-colors focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring"
          />
        </div>

        <div className="space-y-2">
          <Label htmlFor="authorName">{t("author")}</Label>
          <Input
            id="authorName"
            value={formData.authorName}
            onChange={(e) =>
              setFormData({ ...formData, authorName: e.target.value })
            }
            className="bg-background text-foreground"
          />
        </div>

        <div className="space-y-2">
          <Label htmlFor="descriptionAuthor">Mô tả tác giả</Label>
          <Input
            id="descriptionAuthor"
            value={formData.descriptionAuthor}
            onChange={(e) =>
              setFormData({ ...formData, descriptionAuthor: e.target.value })
            }
            className="bg-background text-foreground"
          />
        </div>

        <div className="space-y-2">
          <Label htmlFor="files">
            Tệp đính kèm <span className="text-red-500">*</span>
          </Label>
          <input
            type="file"
            id="files"
            multiple
            onChange={handleFileChange}
            className="hidden"
          />
          <label
            htmlFor="files"
            className="flex items-center gap-2 px-4 py-3 border-2 border-dashed border-slate-300 dark:border-slate-700 rounded-md cursor-pointer hover:bg-slate-50 dark:hover:bg-slate-800 transition-colors"
          >
            <Upload size={20} />
            <span className="text-sm">Chọn tệp</span>
          </label>
          {files.length > 0 && (
            <div className="space-y-2 mt-2">
              {files.map((file, index) => (
                <div
                  key={index}
                  className="flex items-center justify-between p-3 bg-slate-100 dark:bg-slate-800 rounded-md"
                >
                  <div className="flex items-center gap-2">
                    <FileText size={16} />
                    <span className="text-sm text-foreground">{file.name}</span>
                    <span className="text-xs text-slate-500">
                      ({(file.size / 1024 / 1024).toFixed(2)} MB)
                    </span>
                  </div>
                  <Button
                    type="button"
                    variant="ghost"
                    size="sm"
                    onClick={() => handleRemoveFile(index)}
                  >
                    <X size={16} />
                  </Button>
                </div>
              ))}
            </div>
          )}
        </div>

        <div className="flex items-center gap-2">
          <input
            type="checkbox"
            id="isDownload"
            checked={formData.isDownload}
            onChange={(e) =>
              setFormData({ ...formData, isDownload: e.target.checked })
            }
            className="cursor-pointer"
          />
          <Label htmlFor="isDownload" className="cursor-pointer">
            Cho phép tải xuống
          </Label>
        </div>

        <div className="flex gap-4">
          <Button
            type="button"
            variant="outline"
            onClick={() => router.back()}
            disabled={loading}
          >
            {tCommon("cancel")}
          </Button>
          <Button type="submit" disabled={loading || files.length === 0}>
            {loading ? tCommon("loading") : t("upload")}
          </Button>
        </div>
      </form>
    </div>
  );
}

