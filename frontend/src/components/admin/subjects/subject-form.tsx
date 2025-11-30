"use client";

import { useState, useEffect } from "react";
import { Label } from "@/src/components/ui/label";
import { Input } from "@/src/components/ui/input";
import { useTranslations } from "next-intl";
import { useMajors } from "@/src/hooks/use-majors";
import type { UpdateSubjectCommand, CreateSubjectCommand, MajorDto2 } from "@/src/api/database/types.gen";

export interface SubjectFormData {
  subjectName?: string | null;
  subjectCode?: string | null;
  majorIds?: string[];
}

interface SubjectFormProps {
  initialData?: SubjectFormData;
  onSubmit: (data: CreateSubjectCommand | UpdateSubjectCommand) => void | Promise<void>;
  loading?: boolean;
}

export function SubjectForm({
  initialData,
  onSubmit,
  loading,
}: SubjectFormProps) {
  const t = useTranslations("admin.subjects");
  const { fetchMajors, loading: loadingMajors } = useMajors();
  const [formData, setFormData] = useState<SubjectFormData>({
    subjectName: null,
    subjectCode: null,
    majorIds: [],
  });
  const [majors, setMajors] = useState<MajorDto2[]>([]);

  useEffect(() => {
    if (initialData) {
      setFormData({
        subjectName: initialData.subjectName || null,
        subjectCode: initialData.subjectCode || null,
        majorIds: initialData.majorIds || [],
      });
    }
  }, [initialData]);

  useEffect(() => {
    const loadMajors = async () => {
      try {
        const response = await fetchMajors({ Page: 1, PageSize: 1000 });
        if (response?.items) {
          setMajors(response.items);
        }
      } catch (err) {
        console.error("Error loading majors:", err);
      }
    };
    loadMajors();
  }, [fetchMajors]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    const command: CreateSubjectCommand | UpdateSubjectCommand = {
      subjectName: formData.subjectName || undefined,
      subjectCode: formData.subjectCode || undefined,
      majorIds: formData.majorIds && formData.majorIds.length > 0 ? formData.majorIds : undefined,
    };
    await onSubmit(command);
  };

  const handleMajorChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
    const selectedOptions = Array.from(e.target.selectedOptions);
    const selectedIds = selectedOptions.map((option) => option.value);
    setFormData((prev) => ({ ...prev, majorIds: selectedIds }));
  };

  const isDisabled = loading || loadingMajors;

  return (
    <form id="subject-form" onSubmit={handleSubmit} className="space-y-4">
      <div className="space-y-3 grid grid-cols-1 md:grid-cols-2 gap-4">
        <div>
          <Label htmlFor="subjectName">{t("form.subjectName")} *</Label>
          <Input
            id="subjectName"
            value={formData.subjectName || ""}
            onChange={(e) =>
              setFormData((prev) => ({ ...prev, subjectName: e.target.value }))
            }
            required
            disabled={isDisabled}
            className="mt-1"
          />
        </div>
        <div>
          <Label htmlFor="subjectCode">{t("form.subjectCode")} *</Label>
          <Input
            id="subjectCode"
            value={formData.subjectCode || ""}
            onChange={(e) =>
              setFormData((prev) => ({ ...prev, subjectCode: e.target.value }))
            }
            required
            disabled={isDisabled}
            className="mt-1"
          />
        </div>
        <div className="md:col-span-2">
          <Label htmlFor="majorIds">{t("form.majors")}</Label>
          <select
            id="majorIds"
            multiple
            value={formData.majorIds || []}
            onChange={handleMajorChange}
            disabled={isDisabled}
            size={5}
            className="mt-1 flex w-full rounded-md border border-input bg-background text-foreground px-3 py-2 text-sm shadow-sm transition-colors focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-50"
          >
            {majors
              .filter((major): major is MajorDto2 & { id: string } => !!major?.id)
              .map((major) => (
                <option key={major.id} value={major.id}>
                  {major.majorName || ""} ({major.majorCode || ""})
                  {major.faculty ? ` - ${major.faculty.facultyName}` : ""}
                </option>
              ))}
          </select>
          <p className="mt-1 text-xs text-slate-500 dark:text-slate-400">
            {t("form.selectMultipleHint")}
          </p>
        </div>
      </div>
    </form>
  );
}

