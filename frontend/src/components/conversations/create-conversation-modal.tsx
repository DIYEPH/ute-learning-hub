"use client";

import { useState, useEffect } from "react";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from "@/src/components/ui/dialog";
import { Button } from "@/src/components/ui/button";
import { Label } from "@/src/components/ui/label";
import { Input } from "@/src/components/ui/input";
import { Loader2, X, Image as ImageIcon } from "lucide-react";
import { useSubjects } from "@/src/hooks/use-subjects";
import { postApiConversation, getApiTag } from "@/src/api/database/sdk.gen";
import type { CreateConversationCommand, SubjectDto2, TagDto } from "@/src/api/database/types.gen";
import { useFileUpload } from "@/src/hooks/use-file-upload";
import { Avatar, AvatarFallback, AvatarImage } from "@/src/components/ui/avatar";
import { getFileUrlById } from "@/src/lib/file-url";
import { TagPicker } from "@/src/components/ui/tag-picker";

interface CreateConversationModalProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onSuccess?: () => void;
}

export function CreateConversationModal({
  open,
  onOpenChange,
  onSuccess,
}: CreateConversationModalProps) {
  const { fetchSubjects, loading: loadingSubjects } = useSubjects();
  const { uploadFile, uploading: uploadingAvatar } = useFileUpload();

  const [formData, setFormData] = useState<CreateConversationCommand>({
    conversationName: "",
    tagIds: [],
    tagNames: [],
    conversationType: 1, // 1 = Group (nhóm nhiều người)
    visibility: 1, // 1 = Public (công khai)
    subjectId: null,
    isSuggestedByAI: false,
    isAllowMemberPin: false,
  });

  const [subjects, setSubjects] = useState<SubjectDto2[]>([]);
  const [tags, setTags] = useState<TagDto[]>([]);
  const [selectedTagIds, setSelectedTagIds] = useState<string[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [avatarFileInput, setAvatarFileInput] = useState<HTMLInputElement | null>(null);
  const [avatarFile, setAvatarFile] = useState<File | null>(null);
  const [avatarPreview, setAvatarPreview] = useState<string | null>(null);

  useEffect(() => {
    if (open) {
      setFormData({
        conversationName: "",
        tagIds: [],
        tagNames: [],
        conversationType: 1, // 1 = Group
        visibility: 1, // 1 = Public
        subjectId: null,
        isSuggestedByAI: false,
        isAllowMemberPin: false,
      });
      setSelectedTagIds([]);
      setError(null);
      setAvatarFile(null);
      setAvatarPreview(null);
    }
  }, [open]);

  useEffect(() => {
    const loadData = async () => {
      try {
        const [subjectsRes, tagsRes] = await Promise.all([
          fetchSubjects({ Page: 1, PageSize: 1000 }),
          getApiTag({ query: { Page: 1, PageSize: 1000 } }).then(
            (res: any) => res?.data || res
          ),
        ]);
        if (subjectsRes?.items) {
          setSubjects(subjectsRes.items);
        }
        if (tagsRes?.items) {
          setTags(tagsRes.items);
        }
      } catch (err) {
        console.error("Error loading data:", err);
      }
    };
    if (open) {
      loadData();
    }
  }, [open, fetchSubjects]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    if (!formData.conversationName?.trim()) {
      setError("Tên cuộc trò chuyện không được để trống");
      return;
    }

    if (selectedTagIds.length === 0 && (formData.tagNames?.length ?? 0) === 0) {
      setError("Cuộc trò chuyện phải có ít nhất một thẻ");
      return;
    }

    setLoading(true);

    try {
      const tagNamesToSubmit = [...(formData.tagNames || [])];

      // Upload avatar nếu có
      let avatarUrl: string | undefined;
      if (avatarFile) {
        const uploaded = await uploadFile(avatarFile, "AvatarConversation");
        avatarUrl = uploaded.id ? getFileUrlById(uploaded.id) : undefined;
      }

      const submitData: CreateConversationCommand = {
        ...formData,
        tagIds: selectedTagIds.length > 0 ? selectedTagIds : undefined,
        tagNames: tagNamesToSubmit.length > 0 ? tagNamesToSubmit : undefined,
        avatarUrl: avatarUrl,
      };

      const response = await postApiConversation({
        body: submitData,
      });

      if (response.data || response) {
        onSuccess?.();
        onOpenChange(false);
      }
    } catch (err: any) {
      const errorMessage =
        err?.response?.data?.message ||
        err?.response?.data ||
        err?.message ||
        "Không thể tạo cuộc trò chuyện";
      setError(errorMessage);
    } finally {
      setLoading(false);
    }
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-md">
        <DialogHeader>
          <DialogTitle>Tạo cuộc trò chuyện mới</DialogTitle>
        </DialogHeader>

        <form onSubmit={handleSubmit} className="space-y-4">
          {error && (
            <div className="p-3 text-sm text-red-600 dark:text-red-400 bg-red-50 dark:bg-red-950 rounded">
              {error}
            </div>
          )}

          <div className="flex items-start gap-3">
            {/* Avatar nhóm */}
            <div className="flex flex-col items-center gap-2">
              <Avatar className="h-12 w-12">
                <AvatarImage src={avatarPreview || undefined} alt={formData.conversationName || "Avatar nhóm"} />
                <AvatarFallback>
                  <ImageIcon className="h-5 w-5" />
                </AvatarFallback>
              </Avatar>
              <Button
                type="button"
                variant="outline"
                size="sm"
                disabled={loading || uploadingAvatar}
                onClick={() => avatarFileInput?.click()}
                className="h-6 px-2 text-[10px]"
              >
                {uploadingAvatar ? (
                  <>
                    <Loader2 className="mr-1 h-3 w-3 animate-spin" />
                    Đang tải...
                  </>
                ) : (
                  "Chọn ảnh"
                )}
              </Button>
              <input
                type="file"
                accept="image/*"
                className="hidden"
                ref={(el) => setAvatarFileInput(el)}
                onChange={(e) => {
                  const file = e.target.files?.[0];
                  if (!file) return;
                  setAvatarFile(file);
                  // Tạo preview URL
                  const previewUrl = URL.createObjectURL(file);
                  setAvatarPreview(previewUrl);
                  e.target.value = "";
                }}
              />
            </div>

            {/* Tên cuộc trò chuyện */}
            <div className="flex-1">
              <Label htmlFor="conversationName">
                Tên cuộc trò chuyện <span className="text-red-500">*</span>
              </Label>
              <Input
                id="conversationName"
                value={formData.conversationName || ""}
                onChange={(e) =>
                  setFormData((prev) => ({
                    ...prev,
                    conversationName: e.target.value,
                  }))
                }
                required
                disabled={loading}
                className="mt-1"
              />
            </div>
          </div>

          <div>
            <Label htmlFor="tagIds">
              Chủ đề <span className="text-red-500">*</span>
            </Label>
            <TagPicker
              options={tags
                .filter((tag): tag is TagDto & { id: string } => !!tag?.id)
                .map((tag) => ({
                  value: tag.id,
                  label: tag.tagName || "",
                }))}
              selected={selectedTagIds}
              onChange={(values) => {
                setSelectedTagIds(values);
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

          <div>
            <Label htmlFor="subjectId">Môn học (tùy chọn)</Label>
            <select
              id="subjectId"
              value={formData.subjectId || ""}
              onChange={(e) =>
                setFormData((prev) => ({
                  ...prev,
                  subjectId: e.target.value || null,
                }))
              }
              disabled={loading || loadingSubjects}
              className="mt-1 flex h-9 w-full  border border-input bg-background text-foreground px-3 py-1 text-sm shadow-sm transition-colors focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-50"
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
            <Label htmlFor="visibility">Chế độ hiển thị</Label>
            <select
              id="visibility"
              value={formData.visibility ?? 1}
              onChange={(e) =>
                setFormData((prev) => ({
                  ...prev,
                  visibility: parseInt(e.target.value, 10),
                }))
              }
              disabled={loading}
              className="mt-1 flex h-9 w-full  border border-input bg-background text-foreground px-3 py-1 text-sm shadow-sm transition-colors focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-50"
            >
              <option value={0}>Riêng tư</option>
              <option value={1}>Công khai</option>
            </select>
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
                  Đang tạo...
                </>
              ) : (
                "Tạo"
              )}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}



