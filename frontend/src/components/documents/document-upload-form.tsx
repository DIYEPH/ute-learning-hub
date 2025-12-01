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
import type {
  SubjectDto2,
  TypeDto,
  TagDto,
  PostApiDocumentData,
} from "@/src/api/database/types.gen";

type ApiDocumentBody = PostApiDocumentData["body"];
export type DocumentUploadFormData = {
  documentName?: ApiDocumentBody["DocumentName"] | null;
  description?: ApiDocumentBody["Description"] | null;
  authorName?: ApiDocumentBody["AuthorName"] | null;
  descriptionAuthor?: ApiDocumentBody["DescriptionAuthor"] | null;
  subjectId?: ApiDocumentBody["SubjectId"] | null;
  typeId?: ApiDocumentBody["TypeId"] | null;
  tagIds?: ApiDocumentBody["TagIds"];
  tagNames?: ApiDocumentBody["TagNames"];
  isDownload?: ApiDocumentBody["IsDownload"];
  visibility?: number;
  // 1 document = 1 file
  file?: File | null;
};

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
    tagNames: [],
    isDownload: true,
    visibility: 2,
    file: null,
  });

  const [subjects, setSubjects] = useState<SubjectDto2[]>([]);
  const [types, setTypes] = useState<TypeDto[]>([]);
  const [tags, setTags] = useState<TagDto[]>([]);
  const [fileError, setFileError] = useState<string | null>(null);
  const [newTagInput, setNewTagInput] = useState("");

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
          getApiTag({ query: { Page: 1, PageSize: 1000 } }).then(
            (res: any) => res?.data || res
          ),
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

    if (!formData.documentName?.trim()) {
      setFileError("Tên tài liệu không được để trống");
      return;
    }

    if (!formData.description?.trim()) {
      setFileError("Mô tả không được để trống");
      return;
    }

    if (!formData.typeId) {
      setFileError("Vui lòng chọn loại tài liệu");
      return;
    }

    if (
      (!formData.tagIds || formData.tagIds.length === 0) &&
      (!formData.tagNames || formData.tagNames.length === 0)
    ) {
      setFileError("Vui lòng chọn hoặc thêm ít nhất 1 tag");
      return;
    }

    // 1 file bắt buộc
    if (!formData.file) {
      setFileError("Vui lòng chọn 1 file");
      return;
    }

    await onSubmit(formData);
  };

  // chỉ lấy 1 file đầu tiên, mỗi lần chọn sẽ ghi đè file cũ
  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const selectedFile = e.target.files?.[0] || null;

    if (!selectedFile) {
      setFormData((prev) => ({ ...prev, file: null }));
      setFileError("Vui lòng chọn 1 file");
      return;
    }

    setFileError(null);
    setFormData((prev) => ({
      ...prev,
      file: selectedFile,
    }));
  };

  // xóa file đã chọn
  const handleRemoveFile = () => {
    setFormData((prev) => ({ ...prev, file: null }));
    setFileError(null);
  };

  // chọn tag có sẵn (multi-select)
  const handleTagChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
    const selectedOptions = Array.from(e.target.selectedOptions);
    const selectedIds = selectedOptions.map((option) => option.value);
    setFormData((prev) => ({ ...prev, tagIds: selectedIds }));
  };

  // thêm tag mới (nếu đã tồn tại thì tự động gắn vào tagIds)
  const handleAddNewTag = () => {
    const tagName = newTagInput.trim();
    if (!tagName) return;

    const existingTag = tags.find(
      (tag) => tag.tagName?.toLowerCase() === tagName.toLowerCase() && tag.id
    );

    if (existingTag && existingTag.id) {
      const tagId = existingTag.id;
      if (!formData.tagIds?.includes(tagId)) {
        setFormData((prev) => ({
          ...prev,
          tagIds: [...(prev.tagIds || []), tagId],
        }));
      }
    } else {
      if (!formData.tagNames?.some((n) => n.toLowerCase() === tagName.toLowerCase())) {
        setFormData((prev) => ({
          ...prev,
          tagNames: [...(prev.tagNames || []), tagName],
        }));
      }
    }

    setNewTagInput("");
  };

  const handleTagInputKeyDown = (e: React.KeyboardEvent<HTMLInputElement>) => {
    if (e.key === "Enter") {
      e.preventDefault();
      handleAddNewTag();
    }
  };

  const handleRemoveTag = (tagIdOrName: string, isName: boolean) => {
    if (isName) {
      setFormData((prev) => ({
        ...prev,
        tagNames: (prev.tagNames || []).filter((name) => name !== tagIdOrName),
      }));
    } else {
      setFormData((prev) => ({
        ...prev,
        tagIds: (prev.tagIds || []).filter((id) => id !== tagIdOrName),
      }));
    }
  };

  const isDisabled = loading || loadingSubjects || loadingTypes;
  const selectClassName =
    "mt-1 flex h-9 w-full rounded-md border border-input bg-background text-foreground px-3 py-1 text-sm shadow-sm transition-colors focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-50";
  const textareaClassName =
    "mt-1 flex w-full rounded-md border border-input bg-background text-foreground px-3 py-2 text-sm shadow-sm transition-colors focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-50";

  return (
    <form
      id="document-upload-form"
      onSubmit={handleSubmit}
      className="space-y-4"
    >
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
          <Label htmlFor="description">
            {t("description")} <span className="text-red-500">*</span>
          </Label>
          <textarea
            id="description"
            value={formData.description || ""}
            onChange={(e) =>
              setFormData((prev) => ({ ...prev, description: e.target.value }))
            }
            rows={4}
            required
            disabled={isDisabled}
            className={textareaClassName}
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
        <Label htmlFor="descriptionAuthor">Mô tả tác giả</Label>
        <textarea
          id="descriptionAuthor"
          value={formData.descriptionAuthor || ""}
          onChange={(e) =>
            setFormData((prev) => ({
              ...prev,
              descriptionAuthor: e.target.value,
            }))
          }
          rows={3}
          disabled={isDisabled}
          className={textareaClassName}
        />
      </div>

      <div className="space-y-3 grid grid-cols-1 md:grid-cols-2 gap-4">
        <div>
          <Label htmlFor="subjectId">{t("subject")} (Tùy chọn)</Label>
          <select
            id="subjectId"
            value={formData.subjectId || ""}
            onChange={(e) =>
              setFormData((prev) => ({
                ...prev,
                subjectId: e.target.value || null,
              }))
            }
            disabled={isDisabled}
            className={selectClassName}
          >
            <option value="">Chọn môn học (tùy chọn)</option>
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
              setFormData((prev) => ({
                ...prev,
                typeId: e.target.value || null,
              }))
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
        <Label htmlFor="tagIds">
          {t("tags")} <span className="text-red-500">*</span>
        </Label>
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

        {/* Input để thêm tag mới */}
        <div className="mt-2 flex gap-2">
          <Input
            type="text"
            placeholder="Nhập tên tag mới và nhấn Enter"
            value={newTagInput}
            onChange={(e) => setNewTagInput(e.target.value)}
            onKeyDown={handleTagInputKeyDown}
            disabled={isDisabled}
            className="flex-1"
          />
          <Button
            type="button"
            onClick={handleAddNewTag}
            disabled={isDisabled || !newTagInput.trim()}
            size="sm"
          >
            Thêm
          </Button>
        </div>

        {/* Hiển thị tags đã chọn */}
        {((formData.tagIds?.length || 0) > 0 ||
          (formData.tagNames?.length || 0) > 0) && (
          <div className="mt-2 flex flex-wrap gap-2">
            {/* Tags từ tagIds (tag có sẵn) */}
            {formData.tagIds?.map((tagId) => {
              const tag = tags.find((t) => t.id === tagId);
              if (!tag) return null;
              return (
                <div
                  key={tagId}
                  className="flex items-center gap-1 px-2 py-1 bg-blue-100 dark:bg-blue-900 rounded-md text-sm"
                >
                  <span>{tag.tagName}</span>
                  <button
                    type="button"
                    onClick={() => handleRemoveTag(tagId, false)}
                    disabled={isDisabled}
                    className="text-blue-600 dark:text-blue-400 hover:text-blue-800 dark:hover:text-blue-200"
                  >
                    <X size={14} />
                  </button>
                </div>
              );
            })}
            {/* Tags từ tagNames (tag mới) */}
            {formData.tagNames?.map((tagName) => (
              <div
                key={tagName}
                className="flex items-center gap-1 px-2 py-1 bg-green-100 dark:bg-green-900 rounded-md text-sm"
              >
                <span>{tagName}</span>
                <span className="text-xs text-green-600 dark:text-green-400">
                  (mới)
                </span>
                <button
                  type="button"
                  onClick={() => handleRemoveTag(tagName, true)}
                  disabled={isDisabled}
                  className="text-green-600 dark:text-green-400 hover:text-green-800 dark:hover:text-green-200"
                >
                  <X size={14} />
                </button>
              </div>
            ))}
          </div>
        )}
      </div>

      <div>
        <Label htmlFor="file">
          Tệp đính kèm <span className="text-red-500">*</span>
        </Label>
        <input
          type="file"
          id="file"
          accept=".doc,.docx,.pdf,image/*"
          onChange={handleFileChange}
          className="hidden"
          disabled={isDisabled}
        />
        <label
          htmlFor="file"
          className={`mt-1 flex items-center gap-2 px-4 py-3 border-2 border-dashed rounded-md transition-colors ${
            isDisabled
              ? "border-slate-200 dark:border-slate-700 opacity-50 cursor-not-allowed"
              : "border-slate-300 dark:border-slate-700 cursor-pointer hover:bg-slate-50 dark:hover:bg-slate-800"
          }`}
        >
          <Upload size={20} />
          <span className="text-sm">
            {formData.file ? "Đã chọn 1 file" : "Chọn tệp"}
          </span>
        </label>

        {formData.file && (
          <div className="space-y-2 mt-2">
            <div className="flex items-center justify-between p-3 bg-slate-100 dark:bg-slate-800 rounded-md">
              <div className="flex items-center gap-2">
                <FileText size={16} />
                <span className="text-sm text-foreground">{formData.file.name}</span>
                <span className="text-xs text-slate-500">
                  ({(formData.file.size / 1024 / 1024).toFixed(2)} MB)
                </span>
              </div>
              <Button
                type="button"
                variant="ghost"
                size="sm"
                onClick={handleRemoveFile}
                disabled={isDisabled}
              >
                <X size={16} />
              </Button>
            </div>
          </div>
        )}

        <p className="mt-1 text-xs text-slate-500 dark:text-slate-400">
          {formData.file ? "Đã chọn 1 file" : "Chưa chọn file nào"}
        </p>
      </div>

      <div>
        <Label htmlFor="visibility">{t("visibility")}</Label>
        <select
          id="visibility"
          value={formData.visibility ?? 0}
          onChange={(e) =>
            setFormData((prev) => ({
              ...prev,
              visibility: parseInt(e.target.value, 10),
            }))
          }
          disabled={isDisabled}
          className={selectClassName}
        >
          <option value={0}>Công khai</option>
          <option value={1}>Riêng tư</option>
          <option value={2}>Nội bộ</option>
        </select>
      </div>
    </form>
  );
}
