"use client";

import { useState, useEffect } from "react";
import { X, Loader2, Upload } from "lucide-react";
import { useRouter } from "next/navigation";
import { cn } from "@/lib/utils";
import { useSubjects } from "@/src/hooks/use-subjects";
import { useUserProfile } from "@/src/hooks/use-user-profile";
import { deleteApiConversationById, putApiConversationById, getApiTag, postApiConversationByIdLeave } from "@/src/api/database/sdk.gen";
import type { UpdateConversationCommand, SubjectDto2, TagDto, ConversationDetailDto } from "@/src/api/database/types.gen";
import { Button } from "@/src/components/ui/button";
import { Label } from "@/src/components/ui/label";
import { Input } from "@/src/components/ui/input";
import { ScrollArea } from "@/src/components/ui/scroll-area";
import { MemberManagement } from "@/src/components/conversations/member-management";
import { TagPicker } from "@/src/components/ui/tag-picker";

interface EditConversationSidebarProps {
  open: boolean;
  onClose: () => void;
  conversation: ConversationDetailDto | null;
  onSuccess?: () => void;
}

export function EditConversationSidebar({
  open,
  onClose,
  conversation,
  onSuccess,
}: EditConversationSidebarProps) {
  const router = useRouter();
  const { fetchSubjects, loading: loadingSubjects } = useSubjects();
  const { profile } = useUserProfile();

  const currentUserMember = conversation?.members?.find(
    (m) => m.userId === profile?.id
  );

  const isOwnerOrDeputy = currentUserMember?.roleType === 2 || currentUserMember?.roleType === 1;
  const isOwner = currentUserMember?.roleType === 2;

  const [formData, setFormData] = useState<UpdateConversationCommand>({
    id: conversation?.id || undefined,
    conversationName: conversation?.conversationName || null,
    tagIds: null,
    tagNames: null,
    visibility: conversation?.visibility ?? null,
    conversationStatus: null,
    subjectId: conversation?.subject?.id || null,
    isAllowMemberPin: conversation?.isAllowMemberPin ?? null,
  });

  const [subjects, setSubjects] = useState<SubjectDto2[]>([]);
  const [tags, setTags] = useState<TagDto[]>([]);
  const [selectedTagIds, setSelectedTagIds] = useState<string[]>([]);
  const [avatarFile, setAvatarFile] = useState<File | null>(null);
  const [loading, setLoading] = useState(false);
  const [leaving, setLeaving] = useState(false);
  const [deleting, setDeleting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (open && conversation) {
      const currentTagIds = conversation.tags?.map((t) => t.id || "").filter(Boolean) || [];
      setFormData({
        id: conversation.id || undefined,
        conversationName: conversation.conversationName || null,
        tagIds: null,
        tagNames: null,
        visibility: conversation.visibility ?? null,
        conversationStatus: null,
        subjectId: conversation.subject?.id || null,
        isAllowMemberPin: conversation.isAllowMemberPin ?? null,
      });
      setSelectedTagIds(currentTagIds);
      setError(null);
    }
  }, [open, conversation]);

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

    if (!conversation?.id) {
      setError("Không tìm thấy cuộc trò chuyện");
      return;
    }

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

      const submitData: UpdateConversationCommand = {
        id: conversation.id,
        conversationName: formData.conversationName || null,
        tagIds: selectedTagIds.length > 0 ? selectedTagIds : null,
        tagNames: tagNamesToSubmit.length > 0 ? tagNamesToSubmit : null,
        visibility: formData.visibility ?? null,
        subjectId: formData.subjectId || null,
        isAllowMemberPin: formData.isAllowMemberPin ?? null,
      };

      const response = await putApiConversationById({
        path: { id: conversation.id },
        body: submitData,
      });

      if (response.data || response) {
        onSuccess?.();
        onClose();
      }
    } catch (err: any) {
      const errorMessage =
        err?.response?.data?.message ||
        err?.response?.data ||
        err?.message ||
        "Không thể cập nhật cuộc trò chuyện";
      setError(errorMessage);
    } finally {
      setLoading(false);
    }
  };

  const handleLeave = async () => {
    if (!conversation?.id) return;

    if (!confirm("Bạn có chắc chắn muốn rời nhóm này?")) return;

    setLeaving(true);
    setError(null);

    try {
      await postApiConversationByIdLeave({
        path: { id: conversation.id },
      });

      // Quay về danh sách chat
      router.push("/chat");
    } catch (err: any) {
      const errorMessage =
        err?.response?.data?.message ||
        err?.message ||
        "Không thể rời nhóm";
      setError(errorMessage);
    } finally {
      setLeaving(false);
    }
  };

  const handleDeleteConversation = async () => {
    if (!conversation?.id) return;
    if (!isOwner) return;

    if (!confirm("Bạn có chắc chắn muốn giải tán nhóm này? Tất cả tin nhắn và dữ liệu liên quan sẽ bị xóa.")) {
      return;
    }

    setDeleting(true);
    setError(null);

    try {
      await deleteApiConversationById({
        path: { id: conversation.id },
      });

      router.push("/chat");
    } catch (err: any) {
      const errorMessage =
        err?.response?.data?.message ||
        err?.message ||
        "Không thể giải tán nhóm";
      setError(errorMessage);
    } finally {
      setDeleting(false);
    }
  };

  if (!conversation) {
    return null;
  }

  return (
    <>
      {/* Overlay cho mobile */}
      {open && (
        <div
          className="fixed inset-0 bg-black/50 z-40 md:hidden"
          onClick={onClose}
        />
      )}

      {/* Sidebar */}
      <div
        className={cn(
          "fixed top-0 right-0 h-full bg-white dark:bg-slate-900 border-l border-slate-200 dark:border-slate-700 z-50 transition-transform duration-300 ease-in-out",
          "w-full md:w-96",
          open ? "translate-x-0" : "translate-x-full"
        )}
      >
        <div className="flex flex-col h-full overflow-hidden">
          {/* Header */}
          <div className="flex items-center justify-between p-4 border-b border-slate-200 dark:border-slate-700 flex-shrink-0">
            <h3 className="text-lg font-semibold text-foreground">
              Chỉnh sửa cuộc trò chuyện
            </h3>
            <Button
              variant="ghost"
              size="sm"
              onClick={onClose}
              className="h-8 w-8 p-0"
            >
              <X className="h-4 w-4" />
            </Button>
          </div>

          {/* Content */}
          <ScrollArea className="flex-1 min-h-0">
            <div className="p-4">
              <form onSubmit={handleSubmit} className="space-y-4">
                {error && (
                  <div className="p-3 text-sm text-red-600 dark:text-red-400 bg-red-50 dark:bg-red-950 rounded">
                    {error}
                  </div>
                )}

                {/* Avatar nhóm */}
                <div>
                  <Label>Ảnh đại diện nhóm</Label>
                  {/* Hiển thị avatar hiện tại */}
                  {(avatarFile || conversation?.avatarUrl) && (
                    <div className="mt-2 relative inline-block">
                      <img
                        src={avatarFile
                          ? URL.createObjectURL(avatarFile)
                          : conversation?.avatarUrl || ""}
                        alt="Avatar"
                        className="w-20 h-20 object-cover border border-input"
                      />
                      {avatarFile && (
                        <button
                          type="button"
                          onClick={() => setAvatarFile(null)}
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
                      setAvatarFile(file);
                    }}
                    className="hidden"
                    id="edit-avatar"
                    disabled={loading || !isOwnerOrDeputy}
                  />
                  <label
                    htmlFor="edit-avatar"
                    className={`mt-2 flex items-center gap-2 px-3 py-2 text-sm border-2 border-dashed cursor-pointer hover:bg-slate-50 dark:hover:bg-slate-800 w-fit ${(loading || !isOwnerOrDeputy) ? "opacity-50 cursor-not-allowed" : ""}`}
                  >
                    <Upload size={14} />
                    <span>{avatarFile ? "Đổi ảnh" : (conversation?.avatarUrl ? "Đổi avatar" : "Chọn avatar")}</span>
                  </label>
                </div>

                <div>
                  <Label htmlFor="conversationName">
                    Tên cuộc trò chuyện <span className="text-red-500">*</span>
                  </Label>
                  <Input
                    id="conversationName"
                    value={formData.conversationName || ""}
                    onChange={(e) =>
                      setFormData((prev) => ({
                        ...prev,
                        conversationName: e.target.value || null,
                      }))
                    }
                    required
                    disabled={loading}
                    className="mt-1"
                  />
                </div>

                <div>
                  <Label htmlFor="tagIds">
                    Thẻ <span className="text-red-500">*</span>
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

                {isOwnerOrDeputy && (
                  <>
                    <div>
                      <Label htmlFor="conversationType">
                        Loại nhóm <span className="text-red-500">*</span>
                      </Label>
                      <select
                        id="conversationType"
                        value={formData.conversationType ?? ""}
                        onChange={(e) =>
                          setFormData((prev) => ({
                            ...prev,
                            conversationType: e.target.value ? parseInt(e.target.value) : null,
                          }))
                        }
                        required
                        disabled={loading}
                        className="mt-1 flex h-9 w-full  border border-input bg-background text-foreground px-3 py-1 text-sm shadow-sm transition-colors focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-50"
                      >
                        <option value="">Chọn loại nhóm</option>
                        <option value="0">Riêng tư</option>
                        <option value="1">Công khai</option>
                        <option value="2">AI</option>
                      </select>
                      <p className="mt-1 text-xs text-slate-500 dark:text-slate-400">
                        Riêng tư: Cần yêu cầu tham gia. Công khai: Ai cũng có thể tham gia.
                      </p>
                    </div>

                    <div className="flex items-center gap-2">
                      <input
                        type="checkbox"
                        id="isAllowMemberPin"
                        checked={formData.isAllowMemberPin ?? false}
                        onChange={(e) =>
                          setFormData((prev) => ({
                            ...prev,
                            isAllowMemberPin: e.target.checked,
                          }))
                        }
                        disabled={loading}
                        className="h-4 w-4 rounded border-gray-300"
                      />
                      <Label htmlFor="isAllowMemberPin" className="cursor-pointer">
                        Cho phép thành viên ghim tin nhắn
                      </Label>
                    </div>
                  </>
                )}

                <div className="flex gap-2 pt-4 border-t border-slate-200 dark:border-slate-700">
                  <Button
                    type="button"
                    variant="outline"
                    onClick={onClose}
                    disabled={loading || leaving}
                    className="flex-1"
                  >
                    Hủy
                  </Button>
                  <Button type="submit" disabled={loading || leaving} className="flex-1">
                    {loading ? (
                      <>
                        <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                        Đang cập nhật...
                      </>
                    ) : (
                      "Cập nhật"
                    )}
                  </Button>
                </div>
              </form>

              {/* Member Management Section */}
              {conversation && (
                <div className="pt-4 mt-4 border-t border-slate-200 dark:border-slate-700">
                  <MemberManagement conversation={conversation} onSuccess={onSuccess} />
                </div>
              )}

              {/* Leave / Delete Conversation Buttons */}
              <div className="pt-4 mt-4 border-t border-slate-200 dark:border-slate-700 space-y-2">
                <Button
                  type="button"
                  variant="outline"
                  onClick={handleLeave}
                  disabled={leaving || loading || deleting}
                  className="w-full border-red-200 text-red-600 hover:bg-red-50 dark:border-red-800 dark:text-red-300 dark:hover:bg-red-900/30"
                >
                  {leaving ? (
                    <>
                      <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                      Đang rời nhóm...
                    </>
                  ) : (
                    "Rời nhóm"
                  )}
                </Button>

                {isOwner && (
                  <Button
                    type="button"
                    variant="destructive"
                    onClick={handleDeleteConversation}
                    disabled={deleting || loading || leaving}
                    className="w-full"
                  >
                    {deleting ? (
                      <>
                        <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                        Đang giải tán nhóm...
                      </>
                    ) : (
                      "Giải tán nhóm"
                    )}
                  </Button>
                )}
              </div>
            </div>
          </ScrollArea>
        </div>
      </div>
    </>
  );
}



