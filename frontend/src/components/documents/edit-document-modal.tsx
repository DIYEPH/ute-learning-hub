"use client";

import { useState, useEffect } from "react";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from "@/src/components/ui/dialog";
import { Button } from "@/src/components/ui/button";
import { Label } from "@/src/components/ui/label";
import { Input } from "@/src/components/ui/input";
import { Loader2, X, Upload } from "lucide-react";
import { useSubjects } from "@/src/hooks/use-subjects";
import { useTypes } from "@/src/hooks/use-types";
import { getApiTag, getApiAuthor, putApiDocumentById } from "@/src/api/database/sdk.gen";
import type { DocumentDetailDto, SubjectDto2, TypeDto, TagDto, PutApiDocumentByIdData, AuthorListDto, AuthorInput } from "@/src/api/database/types.gen";
import { TagPicker } from "@/src/components/ui/tag-picker";
import { AuthorPicker } from "@/src/components/ui/author-picker";
import { getFileUrlById } from "@/src/lib/file-url";

type UpdateDocumentBody = PutApiDocumentByIdData["body"];

interface EditDocumentModalProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  document: DocumentDetailDto;
  onSuccess?: () => void;
}

export function EditDocumentModal({
  open,
  onOpenChange,
  document,
  onSuccess,
}: EditDocumentModalProps) {
  // Guard against invalid document object (e.g., error response)
  if (!document || !('id' in document) || !document.id) {
    return null;
  }

  const { fetchSubjects, loading: loadingSubjects } = useSubjects();
  const { fetchTypes, loading: loadingTypes } = useTypes();

  const [formData, setFormData] = useState<{
    documentName?: string;
    description?: string;
    subjectId?: string | null;
    typeId?: string | null;
    tagIds?: string[];
    tagNames?: string[];
    authorIds?: string[];
    newAuthors?: AuthorInput[];
    visibility?: number;
    coverFile?: File | null;
  }>({
    documentName: document.documentName || "",
    description: document.description || "",
    subjectId: document.subject?.id || null,
    typeId: document.type?.id || null,
    tagIds: document.tags?.map((t) => t.id || "").filter(Boolean) || [],
    tagNames: [],
    authorIds: document.authors?.map((a) => a.id || "").filter(Boolean) || [],
    newAuthors: [],
    visibility: document.visibility ?? 2,
    coverFile: null,
  });

  const [subjects, setSubjects] = useState<SubjectDto2[]>([]);
  const [types, setTypes] = useState<TypeDto[]>([]);
  const [tags, setTags] = useState<TagDto[]>([]);
  const [authors, setAuthors] = useState<AuthorListDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (open) {
      setFormData({
        documentName: document.documentName || "",
        description: document.description || "",
        subjectId: document.subject?.id || null,
        typeId: document.type?.id || null,
        tagIds: document.tags?.map((t) => t.id || "").filter(Boolean) || [],
        tagNames: [],
        authorIds: document.authors?.map((a) => a.id || "").filter(Boolean) || [],
        newAuthors: [],
        visibility: document.visibility ?? 2,
        coverFile: null,
      });
      setError(null);
    }
  }, [open, document]);

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
    if (open) {
      loadData();
    }
  }, [open, fetchSubjects, fetchTypes]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    if (!formData.documentName?.trim()) {
      setError("Tên tài liệu không được để trống");
      return;
    }

    if (!formData.description?.trim()) {
      setError("Mô tả không được để trống");
      return;
    }

    if (!formData.typeId) {
      setError("Vui lòng chọn loại tài liệu");
      return;
    }

    if (
      (!formData.tagIds || formData.tagIds.length === 0) &&
      (!formData.tagNames || formData.tagNames.length === 0)
    ) {
      setError("Vui lòng chọn hoặc thêm ít nhất 1 chủ đề");
      return;
    }

    setLoading(true);

    try {
      await putApiDocumentById({
        path: { id: document.id! },
        body: {
          id: document.id,
          documentName: formData.documentName,
          description: formData.description,
          subjectId: formData.subjectId,
          typeId: formData.typeId,
          tagIds: formData.tagIds && formData.tagIds.length > 0 ? formData.tagIds : null,
          authorIds: formData.authorIds && formData.authorIds.length > 0 ? formData.authorIds : null,
          authors: formData.newAuthors && formData.newAuthors.length > 0 ? formData.newAuthors : null,
          visibility: formData.visibility,
        },
        throwOnError: true,
      });

      onSuccess?.();
      onOpenChange(false);
    } catch (err: any) {
      const errorMessage =
        err?.response?.data?.message ||
        err?.error?.message ||
        err?.message ||
        "Không thể cập nhật tài liệu";
      setError(errorMessage);
    } finally {
      setLoading(false);
    }
  };

  const selectClassName =
    "mt-1 flex h-9 w-full  border border-input bg-background text-foreground px-3 py-1 text-sm shadow-sm transition-colors focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-50";
  const textareaClassName =
    "mt-1 flex w-full  border border-input bg-background text-foreground px-3 py-2 text-sm shadow-sm transition-colors focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-50";

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-3xl max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle>Chỉnh sửa tài liệu</DialogTitle>
        </DialogHeader>

        <form onSubmit={handleSubmit} className="space-y-4">
          {error && (
            <div className="p-3 text-sm text-red-600 dark:text-red-400 bg-red-50 dark:bg-red-950 rounded">
              {error}
            </div>
          )}

          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <Label htmlFor="edit-documentName">
                Tên tài liệu <span className="text-red-500">*</span>
              </Label>
              <Input
                id="edit-documentName"
                value={formData.documentName || ""}
                onChange={(e) =>
                  setFormData((prev) => ({ ...prev, documentName: e.target.value }))
                }
                required
                disabled={loading}
                className="mt-1"
              />
            </div>
            <div>
              <Label htmlFor="edit-typeId">
                Loại tài liệu <span className="text-red-500">*</span>
              </Label>
              <select
                id="edit-typeId"
                value={formData.typeId || ""}
                onChange={(e) =>
                  setFormData((prev) => ({
                    ...prev,
                    typeId: e.target.value || null,
                  }))
                }
                required
                disabled={loading || loadingTypes}
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
            <Label htmlFor="edit-description">
              Mô tả <span className="text-red-500">*</span>
            </Label>
            <textarea
              id="edit-description"
              value={formData.description || ""}
              onChange={(e) =>
                setFormData((prev) => ({ ...prev, description: e.target.value }))
              }
              rows={4}
              required
              disabled={loading}
              className={textareaClassName}
            />
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <Label htmlFor="edit-subjectId">Môn học (Tùy chọn)</Label>
              <select
                id="edit-subjectId"
                value={formData.subjectId || ""}
                onChange={(e) =>
                  setFormData((prev) => ({
                    ...prev,
                    subjectId: e.target.value || null,
                  }))
                }
                disabled={loading || loadingSubjects}
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
              <Label htmlFor="edit-coverFile">Ảnh bìa (Tùy chọn)</Label>
              {/* Hiển thị ảnh bìa hiện tại */}
              {(formData.coverFile || document.coverFileId) && (
                <div className="mt-2 relative inline-block">
                  <img
                    src={formData.coverFile
                      ? URL.createObjectURL(formData.coverFile)
                      : getFileUrlById(document.coverFileId || "")}
                    alt="Cover preview"
                    className="max-w-[200px] h-auto border border-input object-contain"
                  />
                  {formData.coverFile && (
                    <button
                      type="button"
                      onClick={() => setFormData((prev) => ({ ...prev, coverFile: null }))}
                      className="absolute -top-2 -right-2 bg-destructive text-white rounded-full p-0.5"
                    >
                      <X size={14} />
                    </button>
                  )}
                </div>
              )}
              <input
                type="file"
                accept="image/*"
                onChange={(e) => {
                  const file = e.target.files?.[0] || null;
                  setFormData((prev) => ({ ...prev, coverFile: file }));
                }}
                className="hidden"
                id="edit-coverFile"
                disabled={loading}
              />
              <label
                htmlFor="edit-coverFile"
                className={`mt-1 flex items-center gap-2 px-4 py-2 border-2 border-dashed cursor-pointer hover:bg-slate-50 dark:hover:bg-slate-800 ${loading ? "opacity-50 cursor-not-allowed" : ""
                  }`}
              >
                <Upload size={16} />
                <span className="text-sm">
                  {formData.coverFile ? "Thay đổi ảnh" : (document.coverFileId ? "Thay đổi ảnh bìa" : "Chọn ảnh bìa")}
                </span>
              </label>
            </div>
          </div>

          {/* Tác giả (có thể chỉnh sửa) */}
          <div>
            <Label>Tác giả (Tùy chọn)</Label>
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
                  newAuthors: [...(prev.newAuthors || []), author],
                }));
              }}
              newAuthors={formData.newAuthors || []}
              onRemoveNewAuthor={(index) => {
                setFormData((prev) => ({
                  ...prev,
                  newAuthors: (prev.newAuthors || []).filter((_, i) => i !== index),
                }));
              }}
              disabled={loading}
              className="mt-2"
            />
          </div>


          <div>
            <Label htmlFor="edit-tagIds">
              Tags <span className="text-red-500">*</span>
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
              disabled={loading}
              className="mt-2"
            />
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <Label htmlFor="edit-visibility">Quyền truy cập</Label>
              <select
                id="edit-visibility"
                value={formData.visibility ?? 0}
                onChange={(e) =>
                  setFormData((prev) => ({
                    ...prev,
                    visibility: parseInt(e.target.value, 10),
                  }))
                }
                disabled={loading}
                className={selectClassName}
              >
                <option value={0}>Công khai</option>
                <option value={1}>Riêng tư</option>
                <option value={2}>Nội bộ</option>
              </select>
            </div>
          </div>

          <DialogFooter>
            <Button
              type="button"
              variant="outline"
              onClick={() => onOpenChange(false)}
              disabled={loading}
            >
              Hủy
            </Button>
            <Button type="submit" disabled={loading}>
              {loading ? (
                <>
                  <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                  Đang lưu...
                </>
              ) : (
                "Lưu thay đổi"
              )}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}


