"use client";

import { useState, useEffect } from "react";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from "@/src/components/ui/dialog";
import { Button } from "@/src/components/ui/button";
import { Label } from "@/src/components/ui/label";
import { Input } from "@/src/components/ui/input";
import { Loader2 } from "lucide-react";
import { useSubjects } from "@/src/hooks/use-subjects";
import { postApiConversation } from "@/src/api/database/sdk.gen";
import type { CreateConversationCommand, SubjectDto2 } from "@/src/api/database/types.gen";

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

  const [formData, setFormData] = useState<CreateConversationCommand>({
    conversationName: "",
    topic: "",
    conversationType: 0, // 0 = Public, 1 = Private, etc.
    subjectId: null,
    isSuggestedByAI: false,
    isAllowMemberPin: false,
  });

  const [subjects, setSubjects] = useState<SubjectDto2[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (open) {
      setFormData({
        conversationName: "",
        topic: "",
        conversationType: 0,
        subjectId: null,
        isSuggestedByAI: false,
        isAllowMemberPin: false,
      });
      setError(null);
    }
  }, [open]);

  useEffect(() => {
    const loadSubjects = async () => {
      try {
        const subjectsRes = await fetchSubjects({ Page: 1, PageSize: 1000 });
        if (subjectsRes?.items) {
          setSubjects(subjectsRes.items);
        }
      } catch (err) {
        console.error("Error loading subjects:", err);
      }
    };
    if (open) {
      loadSubjects();
    }
  }, [open, fetchSubjects]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    if (!formData.conversationName?.trim()) {
      setError("Tên cuộc trò chuyện không được để trống");
      return;
    }

    setLoading(true);

    try {
      const response = await postApiConversation({
        body: formData,
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
                  conversationName: e.target.value,
                }))
              }
              required
              disabled={loading}
              className="mt-1"
            />
          </div>

          <div>
            <Label htmlFor="topic">Chủ đề (tùy chọn)</Label>
            <Input
              id="topic"
              value={formData.topic || ""}
              onChange={(e) =>
                setFormData((prev) => ({ ...prev, topic: e.target.value }))
              }
              disabled={loading}
              className="mt-1"
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
              className="mt-1 flex h-9 w-full rounded-md border border-input bg-background text-foreground px-3 py-1 text-sm shadow-sm transition-colors focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-50"
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
            <Label htmlFor="conversationType">Loại cuộc trò chuyện</Label>
            <select
              id="conversationType"
              value={formData.conversationType ?? 0}
              onChange={(e) =>
                setFormData((prev) => ({
                  ...prev,
                  conversationType: parseInt(e.target.value, 10),
                }))
              }
              disabled={loading}
              className="mt-1 flex h-9 w-full rounded-md border border-input bg-background text-foreground px-3 py-1 text-sm shadow-sm transition-colors focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-50"
            >
              <option value={0}>Công khai</option>
              <option value={1}>Riêng tư</option>
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

