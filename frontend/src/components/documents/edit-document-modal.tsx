"use client";

import { useState, useEffect } from "react";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from "@/src/components/ui/dialog";
import { Button } from "@/src/components/ui/button";
import { Label } from "@/src/components/ui/label";
import { Input } from "@/src/components/ui/input";
import { Loader2, X, Upload } from "lucide-react";
import { useSubjects } from "@/src/hooks/use-subjects";
import { useTypes } from "@/src/hooks/use-types";
import { getApiTag } from "@/src/api/database/sdk.gen";
import type { DocumentDetailDto, SubjectDto2, TypeDto, TagDto, PutApiDocumentByIdData } from "@/src/api/database/types.gen";
import { getBearerToken } from "@/src/api/client";
import axios from "axios";

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
  const { fetchSubjects, loading: loadingSubjects } = useSubjects();
  const { fetchTypes, loading: loadingTypes } = useTypes();

  const [formData, setFormData] = useState<{
    documentName?: string;
    description?: string;
    authorNames?: string[];
    subjectId?: string | null;
    typeId?: string | null;
    tagIds?: string[];
    tagNames?: string[];
    isDownload?: boolean;
    visibility?: number;
    coverFile?: File | null;
  }>({
    documentName: document.documentName || "",
    description: document.description || "",
    authorNames: document.authors?.map((a) => a.fullName || "") || [],
    subjectId: document.subject?.id || null,
    typeId: document.type?.id || null,
    tagIds: document.tags?.map((t) => t.id || "").filter(Boolean) || [],
    tagNames: [],
    isDownload: document.isDownload ?? true,
    visibility: document.visibility ?? 2,
    coverFile: null,
  });

  const [subjects, setSubjects] = useState<SubjectDto2[]>([]);
  const [types, setTypes] = useState<TypeDto[]>([]);
  const [tags, setTags] = useState<TagDto[]>([]);
  const [authorNameInput, setAuthorNameInput] = useState("");
  const [newTagInput, setNewTagInput] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (open) {
      setFormData({
        documentName: document.documentName || "",
        description: document.description || "",
        authorNames: document.authors?.map((a) => a.fullName || "") || [],
        subjectId: document.subject?.id || null,
        typeId: document.type?.id || null,
        tagIds: document.tags?.map((t) => t.id || "").filter(Boolean) || [],
        tagNames: [],
        isDownload: document.isDownload ?? true,
        visibility: document.visibility ?? 2,
        coverFile: null,
      });
      setError(null);
    }
  }, [open, document]);

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
      setError("Vui lòng chọn hoặc thêm ít nhất 1 tag");
      return;
    }

    setLoading(true);

    try {
      const apiBaseUrl = process.env.NEXT_PUBLIC_API_URL || "https://localhost:7080";
      const token = getBearerToken();

      const formDataToSend = new FormData();
      formDataToSend.append("Id", document.id || "");
      formDataToSend.append("DocumentName", formData.documentName);
      formDataToSend.append("Description", formData.description);

      if (formData.coverFile) {
        formDataToSend.append("CoverFile", formData.coverFile);
      }

      if (formData.subjectId) {
        formDataToSend.append("SubjectId", formData.subjectId);
      }
      if (formData.typeId) {
        formDataToSend.append("TypeId", formData.typeId);
      }

      if (formData.tagIds && formData.tagIds.length > 0) {
        formData.tagIds.forEach((tagId) => {
          formDataToSend.append("TagIds", tagId);
        });
      }

      formDataToSend.append("IsDownload", (formData.isDownload ?? true).toString());
      formDataToSend.append("Visibility", (formData.visibility ?? 0).toString());

      await axios.put(
        `${apiBaseUrl}/api/Document/${document.id}`,
        formDataToSend,
        {
          headers: {
            ...(token && { Authorization: token }),
          },
        }
      );

      onSuccess?.();
      onOpenChange(false);
    } catch (err: any) {
      const errorMessage =
        err?.response?.data?.message ||
        err?.response?.data ||
        err?.message ||
        "Không thể cập nhật tài liệu";
      setError(errorMessage);
    } finally {
      setLoading(false);
    }
  };

  const handleAddAuthor = () => {
    const authorName = authorNameInput.trim();
    if (authorName && !formData.authorNames?.includes(authorName)) {
      setFormData((prev) => ({
        ...prev,
        authorNames: [...(prev.authorNames || []), authorName],
      }));
      setAuthorNameInput("");
    }
  };

  const handleRemoveAuthor = (authorName: string) => {
    setFormData((prev) => ({
      ...prev,
      authorNames: prev.authorNames?.filter((n) => n !== authorName) || [],
    }));
  };

  const handleAddTag = () => {
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

  const selectClassName =
    "mt-1 flex h-9 w-full rounded-md border border-input bg-background text-foreground px-3 py-1 text-sm shadow-sm transition-colors focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-50";
  const textareaClassName =
    "mt-1 flex w-full rounded-md border border-input bg-background text-foreground px-3 py-2 text-sm shadow-sm transition-colors focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-50";

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
                className={`mt-1 flex items-center gap-2 px-4 py-2 border-2 border-dashed rounded-md cursor-pointer hover:bg-slate-50 dark:hover:bg-slate-800 ${
                  loading ? "opacity-50 cursor-not-allowed" : ""
                }`}
              >
                <Upload size={16} />
                <span className="text-sm">
                  {formData.coverFile ? formData.coverFile.name : "Chọn ảnh bìa"}
                </span>
              </label>
            </div>
          </div>

          <div>
            <Label htmlFor="edit-authorNames">Tác giả (Tùy chọn)</Label>
            <div className="mt-1 flex gap-2">
              <Input
                type="text"
                placeholder="Nhập tên tác giả"
                value={authorNameInput}
                onChange={(e) => setAuthorNameInput(e.target.value)}
                onKeyDown={(e) => {
                  if (e.key === "Enter") {
                    e.preventDefault();
                    handleAddAuthor();
                  }
                }}
                disabled={loading}
                className="flex-1"
              />
              <Button
                type="button"
                onClick={handleAddAuthor}
                disabled={loading || !authorNameInput.trim()}
                size="sm"
              >
                Thêm
              </Button>
            </div>
            {formData.authorNames && formData.authorNames.length > 0 && (
              <div className="mt-2 flex flex-wrap gap-2">
                {formData.authorNames.map((authorName) => (
                  <div
                    key={authorName}
                    className="flex items-center gap-1 px-2 py-1 bg-blue-100 dark:bg-blue-900 rounded-md text-sm"
                  >
                    <span>{authorName}</span>
                    <button
                      type="button"
                      onClick={() => handleRemoveAuthor(authorName)}
                      disabled={loading}
                      className="text-blue-600 dark:text-blue-400 hover:text-blue-800"
                    >
                      <X size={14} />
                    </button>
                  </div>
                ))}
              </div>
            )}
          </div>

          <div>
            <Label htmlFor="edit-tagIds">
              Tags <span className="text-red-500">*</span>
            </Label>
            <select
              id="edit-tagIds"
              multiple
              value={formData.tagIds || []}
              onChange={(e) => {
                const selectedOptions = Array.from(e.target.selectedOptions);
                const selectedIds = selectedOptions.map((option) => option.value);
                setFormData((prev) => ({ ...prev, tagIds: selectedIds }));
              }}
              disabled={loading}
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
            <p className="mt-1 text-xs text-slate-500">
              Giữ Ctrl/Cmd để chọn nhiều thẻ
            </p>

            <div className="mt-2 flex gap-2">
              <Input
                type="text"
                placeholder="Nhập tên tag mới"
                value={newTagInput}
                onChange={(e) => setNewTagInput(e.target.value)}
                onKeyDown={(e) => {
                  if (e.key === "Enter") {
                    e.preventDefault();
                    handleAddTag();
                  }
                }}
                disabled={loading}
                className="flex-1"
              />
              <Button
                type="button"
                onClick={handleAddTag}
                disabled={loading || !newTagInput.trim()}
                size="sm"
              >
                Thêm
              </Button>
            </div>

            {((formData.tagIds?.length || 0) > 0 ||
              (formData.tagNames?.length || 0) > 0) && (
              <div className="mt-2 flex flex-wrap gap-2">
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
                        disabled={loading}
                        className="text-blue-600 dark:text-blue-400"
                      >
                        <X size={14} />
                      </button>
                    </div>
                  );
                })}
                {formData.tagNames?.map((tagName) => (
                  <div
                    key={tagName}
                    className="flex items-center gap-1 px-2 py-1 bg-green-100 dark:bg-green-900 rounded-md text-sm"
                  >
                    <span>{tagName}</span>
                    <span className="text-xs text-green-600">(mới)</span>
                    <button
                      type="button"
                      onClick={() => handleRemoveTag(tagName, true)}
                      disabled={loading}
                      className="text-green-600 dark:text-green-400"
                    >
                      <X size={14} />
                    </button>
                  </div>
                ))}
              </div>
            )}
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
            <div>
              <Label htmlFor="edit-isDownload" className="flex items-center gap-2">
                <input
                  type="checkbox"
                  id="edit-isDownload"
                  checked={formData.isDownload ?? true}
                  onChange={(e) =>
                    setFormData((prev) => ({ ...prev, isDownload: e.target.checked }))
                  }
                  disabled={loading}
                  className="rounded"
                />
                <span>Cho phép tải xuống</span>
              </Label>
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

