"use client";

import { useState, useEffect } from "react";
import { Label } from "@/src/components/ui/label";
import { Input } from "@/src/components/ui/input";
import { Button } from "@/src/components/ui/button";
import { Upload, X } from "lucide-react";
import { useTranslations } from "next-intl";
import { useSubjects } from "@/src/hooks/use-subjects";
import { useTypes } from "@/src/hooks/use-types";
import { getApiTag, getApiAuthor } from "@/src/api/database/sdk.gen";
import type {
  SubjectDto2,
  TypeDto,
  TagDto,
  CreateDocumentCommand,
  AuthorListDto,
  AuthorInput,
} from "@/src/api/database/types.gen";
import { TagPicker } from "@/src/components/ui/tag-picker";
import { AuthorPicker } from "@/src/components/ui/author-picker";
import { ImageUpload } from "@/src/components/ui/image-upload";

type ApiDocumentBody = CreateDocumentCommand;
export type DocumentUploadFormData = {
  documentName?: ApiDocumentBody["documentName"] | null;
  description?: ApiDocumentBody["description"] | null;
  authorIds?: ApiDocumentBody["authorIds"];
  authors?: ApiDocumentBody["authors"];
  subjectId?: ApiDocumentBody["subjectId"] | null;
  typeId?: ApiDocumentBody["typeId"] | null;
  tagIds?: ApiDocumentBody["tagIds"];
  tagNames?: ApiDocumentBody["tagNames"];
  visibility?: number;
  // File không bắt buộc - nếu không có file thì không tạo document
  file?: File | null;
  coverFile?: File | null;
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
    authorIds: [],
    authors: [],
    subjectId: null,
    typeId: null,
    tagIds: [],
    tagNames: [],
    visibility: 2,
    file: null,
    coverFile: null,
  });

  const [subjects, setSubjects] = useState<SubjectDto2[]>([]);
  const [types, setTypes] = useState<TypeDto[]>([]);
  const [tags, setTags] = useState<TagDto[]>([]);
  const [authors, setAuthors] = useState<AuthorListDto[]>([]);
  const [fileError, setFileError] = useState<string | null>(null);

  useEffect(() => {
    if (initialData) {
      setFormData(initialData);
    }
  }, [initialData]);

  useEffect(() => {
    const loadData = async () => {
      try {
        const [subjectsRes, typesRes, tagsRes, authorsRes] = await Promise.all([
          fetchSubjects({ Page: 1, PageSize: 1000 }),
          fetchTypes({ Page: 1, PageSize: 1000 }),
          getApiTag({ query: { Page: 1, PageSize: 1000 } }).then(
            (res: any) => res?.data || res
          ),
          getApiAuthor({ query: { Page: 1, PageSize: 1000 } }).then(
            (res: any) => res?.data || res
          ),
        ]);

        if (subjectsRes?.items) setSubjects(subjectsRes.items);
        if (typesRes?.items) setTypes(typesRes.items);
        if (tagsRes?.items) setTags(tagsRes.items);
        if (authorsRes?.items) setAuthors(authorsRes.items);
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

    // File không bắt buộc - nếu không có file thì không tạo document
    // Logic này sẽ được xử lý ở parent component

    await onSubmit(formData);
  };

  const isDisabled = loading || loadingSubjects || loadingTypes;
  const selectClassName =
    "mt-1 flex h-9 w-full  border border-input bg-background text-foreground px-3 py-1 text-sm shadow-sm transition-colors focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-50";
  const textareaClassName =
    "mt-1 flex w-full  border border-input bg-background text-foreground px-3 py-2 text-sm shadow-sm transition-colors focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-50";

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

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
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
            rows={3}
            required
            disabled={isDisabled}
            className={textareaClassName}
          />
        </div>
      </div>

      {/* Author section */}
      <div>
        <Label htmlFor="authors">{t("author")} (Tùy chọn)</Label>
        <AuthorPicker
          options={authors
            .filter((a): a is AuthorListDto & { id: string } => !!a?.id)
            .map((author) => ({
              value: author.id,
              label: author.fullName || "",
              description: author.description,
            }))}
          selected={formData.authorIds || []}
          onChange={(values) => {
            setFormData((prev) => ({ ...prev, authorIds: values }));
          }}
          onAddNew={(author) => {
            setFormData((prev) => ({
              ...prev,
              authors: [...(prev.authors || []), author],
            }));
          }}
          newAuthors={formData.authors || []}
          onRemoveNewAuthor={(index) => {
            setFormData((prev) => ({
              ...prev,
              authors: (prev.authors || []).filter((_, i) => i !== index),
            }));
          }}
          disabled={isDisabled}
          className="mt-2"
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
        <TagPicker
          options={tags
            .filter((tag): tag is TagDto & { id: string } => !!tag?.id)
            .map((tag) => ({
              value: tag.id,
              label: tag.tagName || "",
            }))}
          selected={formData.tagIds || []}
          onChange={(values) => {
            setFormData((prev) => ({ ...prev, tagIds: values }));
          }}
          onAddNew={(tagName) => {
            setFormData((prev) => ({
              ...prev,
              tagNames: [...(prev.tagNames || []), tagName],
            }));
          }}
          newTags={formData.tagNames || []}
          onRemoveNewTag={(tagName) => {
            setFormData((prev) => ({
              ...prev,
              tagNames: prev.tagNames?.filter((t) => t !== tagName) || [],
            }));
          }}
          disabled={isDisabled}
          className="mt-2"
        />
      </div>

      <div>
        <Label>Ảnh bìa tài liệu (Tùy chọn)</Label>
        <ImageUpload
          value={formData.coverFile}
          onChange={(file) => setFormData((prev) => ({ ...prev, coverFile: file }))}
          disabled={isDisabled}
          aspectRatio="cover"
          placeholder="Chọn ảnh bìa"
          className="mt-2"
        />
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
    </form >
  );
}

