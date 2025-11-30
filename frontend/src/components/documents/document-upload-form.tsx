"use client";

import { useState, useEffect } from "react";
import { Label } from "@/src/components/ui/label";
import { Input } from "@/src/components/ui/input";
import { Button } from "@/src/components/ui/button";
import { Upload, X, FileText } from "lucide-react";
import { useTranslations } from "next-intl";
import { useSubjects } from "@/src/hooks/use-subjects";
import { useTypes } from "@/src/hooks/use-types";
import { getApiTag } from "@/src/api/database/sdk.gen";
import type { SubjectDto2, TypeDto, TagDto } from "@/src/api/database/types.gen";

export interface DocumentUploadFormData {
  documentName?: string | null;
  description?: string | null;
  authorName?: string | null;
  descriptionAuthor?: string | null;
  subjectId?: string | null;
  typeId?: string | null;
  tagIds?: string[];
  isDownload?: boolean;
  visibility?: "Public" | "Private";
  files?: File[];
}

interface DocumentUploadFormProps {
  initialData?: DocumentUploadFormData;
  onSubmit: (data: DocumentUploadFormData) => void | Promise<void>;
  loading?: boolean;
}

export function DocumentUploadForm({
  initialData,
  onSubmit,
  loading,
}: DocumentUploadFormProps) {
  const t = useTranslations("documents");
  const { fetchSubjects, loading: loadingSubjects } = useSubjects();
  const { fetchTypes, loading: loadingTypes } = useTypes();
  
  const [formData, setFormData] = useState<DocumentUploadFormData>({
    documentName: null,
    description: null,
    authorName: null,
    descriptionAuthor: null,
    subjectId: null,
    typeId: null,
    tagIds: [],
    isDownload: true,
    visibility: "Public",
    files: [],
  });

  const [subjects, setSubjects] = useState<SubjectDto2[]>([]);
  const [types, setTypes] = useState<TypeDto[]>([]);
  const [tags, setTags] = useState<TagDto[]>([]);
  const [fileError, setFileError] = useState<string | null>(null);

  useEffect(() => {
    if (initialData) {
      setFormData(initialData);
    }
  }, [initialData]);

  useEffect(() => {
    const loadData = async () => {
      try {
        const [subjectsRes, typesRes, tagsRes] = await Promise.all([
          fetchSubjects({ Page: 1, PageSize: 1000 }),
          fetchTypes({ Page: 1, PageSize: 1000 }),
          getApiTag({ query: { Page: 1, PageSize: 1000 } }).then((res: any) => res?.data || res),
        ]);

        if (subjectsRes?.items) setSubjects(subjectsRes.items);
        if (typesRes?.items) setTypes(typesRes.items);
        if (tagsRes?.items) setTags(tagsRes.items);
      } catch (err) {
        console.error("Error loading data:", err);
      }
    };
    loadData();
  }, [fetchSubjects, fetchTypes]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setFileError(null);

    // Validation
    if (!formData.documentName?.trim()) {
      setFileError("Tên tài liệu không được để trống");
      return;
    }

    if (!formData.subjectId) {
      setFileError("Vui lòng chọn môn học");
      return;
    }

    if (!formData.typeId) {
      setFileError("Vui lòng chọn loại tài liệu");
      return;
    }

    if (!formData.files || formData.files.length === 0) {
      setFileError("Vui lòng chọn ít nhất 1 file");
      return;
    }

    if (formData.files.length > 3) {
      setFileError("Tối đa chỉ được upload 3 file");
      return;
    }

    await onSubmit(formData);
  };

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const selectedFiles = Array.from(e.target.files || []);
    const currentFiles = formData.files || [];
    const totalFiles = currentFiles.length + selectedFiles.length;

    if (totalFiles > 3) {
      setFileError("Tối đa chỉ được upload 3 file");
      return;
    }

    setFileError(null);
    setFormData((prev) => ({
      ...prev,
      files: [...currentFiles, ...selectedFiles],
    }));
  };

  const handleRemoveFile = (index: number) => {
    const newFiles = (formData.files || []).filter((_, i) => i !== index);
    setFormData((prev) => ({ ...prev, files: newFiles }));
    setFileError(null);
  };

  const handleTagChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
    const selectedOptions = Array.from(e.target.selectedOptions);
    const selectedIds = selectedOptions.map((option) => option.value);
    setFormData((prev) => ({ ...prev, tagIds: selectedIds }));
  };

  const isDisabled = loading || loadingSubjects || loadingTypes;
  const selectClassName = "mt-1 flex h-9 w-full rounded-md border border-input bg-background text-foreground px-3 py-1 text-sm shadow-sm transition-colors focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-50";
  const textareaClassName = "mt-1 flex w-full rounded-md border border-input bg-background text-foreground px-3 py-2 text-sm shadow-sm transition-colors focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-50";

  return (
    <form id="document-upload-form" onSubmit={handleSubmit} className="space-y-4">
      {fileError && (
        <div className="p-3 text-sm text-red-600 dark:text-red-400 bg-red-50 dark:bg-red-950 rounded">
          {fileError}
        </div>
      )}

      <div className="space-y-3 grid grid-cols-1 md:grid-cols-2 gap-4">
        <div>
          <Label htmlFor="documentName">
            {t("documentName")} <span className="text-red-500">*</span>
          </Label>
          <Input
            id="documentName"
            value={formData.documentName || ""}
            onChange={(e) =>
              setFormData((prev) => ({ ...prev, documentName: e.target.value }))
            }
            required
            disabled={isDisabled}
            className="mt-1"
          />
        </div>

        <div>
          <Label htmlFor="authorName">{t("author")}</Label>
          <Input
            id="authorName"
            value={formData.authorName || ""}
            onChange={(e) =>
              setFormData((prev) => ({ ...prev, authorName: e.target.value }))
            }
            disabled={isDisabled}
            className="mt-1"
          />
        </div>
      </div>

      <div>
        <Label htmlFor="description">{t("description")}</Label>
        <textarea
          id="description"
          value={formData.description || ""}
          onChange={(e) =>
            setFormData((prev) => ({ ...prev, description: e.target.value }))
          }
          rows={4}
          disabled={isDisabled}
          className={textareaClassName}
        />
      </div>

      <div>
        <Label htmlFor="descriptionAuthor">Mô tả tác giả</Label>
        <textarea
          id="descriptionAuthor"
          value={formData.descriptionAuthor || ""}
          onChange={(e) =>
            setFormData((prev) => ({ ...prev, descriptionAuthor: e.target.value }))
          }
          rows={3}
          disabled={isDisabled}
          className={textareaClassName}
        />
      </div>

      <div className="space-y-3 grid grid-cols-1 md:grid-cols-2 gap-4">
        <div>
          <Label htmlFor="subjectId">
            {t("subject")} <span className="text-red-500">*</span>
          </Label>
          <select
            id="subjectId"
            value={formData.subjectId || ""}
            onChange={(e) =>
              setFormData((prev) => ({ ...prev, subjectId: e.target.value || null }))
            }
            required
            disabled={isDisabled}
            className={selectClassName}
          >
            <option value="">Chọn môn học</option>
            {subjects
              .filter((s): s is SubjectDto2 & { id: string } => !!s?.id)
              .map((subject) => (
                <option key={subject.id} value={subject.id}>
                  {subject.subjectName || ""} ({subject.subjectCode || ""})
                </option>
              ))}
          </select>
        </div>

        <div>
          <Label htmlFor="typeId">
            {t("type")} <span className="text-red-500">*</span>
          </Label>
          <select
            id="typeId"
            value={formData.typeId || ""}
            onChange={(e) =>
              setFormData((prev) => ({ ...prev, typeId: e.target.value || null }))
            }
            required
            disabled={isDisabled}
            className={selectClassName}
          >
            <option value="">Chọn loại tài liệu</option>
            {types
              .filter((t): t is TypeDto & { id: string } => !!t?.id)
              .map((type) => (
                <option key={type.id} value={type.id}>
                  {type.typeName}
                </option>
              ))}
          </select>
        </div>
      </div>

      <div>
        <Label htmlFor="tagIds">{t("tags")} (Tùy chọn)</Label>
        <select
          id="tagIds"
          multiple
          value={formData.tagIds || []}
          onChange={handleTagChange}
          disabled={isDisabled}
          size={5}
          className={selectClassName}
        >
          {tags
            .filter((tag): tag is TagDto & { id: string } => !!tag?.id)
            .map((tag) => (
              <option key={tag.id} value={tag.id}>
                {tag.tagName}
              </option>
            ))}
        </select>
        <p className="mt-1 text-xs text-slate-500 dark:text-slate-400">
          Giữ Ctrl/Cmd để chọn nhiều thẻ
        </p>
      </div>

      <div>
        <Label htmlFor="files">
          Tệp đính kèm <span className="text-red-500">*</span> (Tối đa 3 file)
        </Label>
        <input
          type="file"
          id="files"
          multiple
          onChange={handleFileChange}
          className="hidden"
          disabled={isDisabled || (formData.files?.length || 0) >= 3}
        />
        <label
          htmlFor="files"
          className={`mt-1 flex items-center gap-2 px-4 py-3 border-2 border-dashed rounded-md transition-colors ${
            isDisabled || (formData.files?.length || 0) >= 3
              ? "border-slate-200 dark:border-slate-700 opacity-50 cursor-not-allowed"
              : "border-slate-300 dark:border-slate-700 cursor-pointer hover:bg-slate-50 dark:hover:bg-slate-800"
          }`}
        >
          <Upload size={20} />
          <span className="text-sm">
            {(formData.files?.length || 0) >= 3 ? "Đã đạt giới hạn 3 file" : "Chọn tệp"}
          </span>
        </label>
        {(formData.files?.length || 0) > 0 && (
          <div className="space-y-2 mt-2">
            {formData.files?.map((file, index) => (
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
                  disabled={isDisabled}
                >
                  <X size={16} />
                </Button>
              </div>
            ))}
          </div>
        )}
        <p className="mt-1 text-xs text-slate-500 dark:text-slate-400">
          Đã chọn {(formData.files?.length || 0)}/3 file
        </p>
      </div>

      <div className="space-y-3 grid grid-cols-1 md:grid-cols-2 gap-4">
        <div className="flex items-center gap-2">
          <input
            type="checkbox"
            id="isDownload"
            checked={formData.isDownload ?? true}
            onChange={(e) =>
              setFormData((prev) => ({ ...prev, isDownload: e.target.checked }))
            }
            disabled={isDisabled}
            className="cursor-pointer"
          />
          <Label htmlFor="isDownload" className="cursor-pointer">
            Cho phép tải xuống
          </Label>
        </div>

        <div>
          <Label htmlFor="visibility">{t("visibility")}</Label>
          <select
            id="visibility"
            value={formData.visibility || "Public"}
            onChange={(e) =>
              setFormData((prev) => ({
                ...prev,
                visibility: e.target.value as "Public" | "Private",
              }))
            }
            disabled={isDisabled}
            className={selectClassName}
          >
            <option value="Public">Công khai</option>
            <option value="Private">Riêng tư</option>
          </select>
        </div>
      </div>
    </form>
  );
}

