"use client";

import { useState, useEffect, useCallback } from "react";
import { Label } from "@/src/components/ui/label";
import { Input } from "@/src/components/ui/input";
import { useTranslations } from "next-intl";
import { useMajors } from "@/src/hooks/use-majors";
import { useDebounce } from "@/src/hooks/use-debounce";
import { getApiSubject } from "@/src/api/database/sdk.gen";
import type { UpdateSubjectCommand, CreateSubjectCommand, MajorDto2, SubjectDto2 } from "@/src/api/database/types.gen";
import { AlertCircle, Loader2 } from "lucide-react";

export interface SubjectFormData {
  id?: string;
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

  // Debounce search state
  const [searching, setSearching] = useState(false);
  const [matchingSubjects, setMatchingSubjects] = useState<SubjectDto2[]>([]);
  const [isDuplicate, setIsDuplicate] = useState(false);

  const debouncedName = useDebounce(formData.subjectName || "", 400);

  useEffect(() => {
    if (initialData) {
      setFormData({
        id: initialData.id,
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

  // Search for matching subjects when name changes
  const searchSubjects = useCallback(async (searchTerm: string) => {
    if (!searchTerm.trim()) {
      setMatchingSubjects([]);
      setIsDuplicate(false);
      return;
    }

    setSearching(true);
    try {
      const response = await getApiSubject({ query: { SearchTerm: searchTerm, Page: 1, PageSize: 5 } });
      const data = (response as unknown as { data: { items?: SubjectDto2[] } })?.data || response as { items?: SubjectDto2[] };
      const items = data?.items || [];

      // Filter out current item if editing
      const filtered = initialData?.id
        ? items.filter(item => item.id !== initialData.id)
        : items;

      setMatchingSubjects(filtered);

      // Check for exact duplicate
      const exactMatch = filtered.some(
        item => item.subjectName?.toLowerCase() === searchTerm.toLowerCase()
      );
      setIsDuplicate(exactMatch);
    } catch (error) {
      console.error("Error searching subjects:", error);
      setMatchingSubjects([]);
      setIsDuplicate(false);
    } finally {
      setSearching(false);
    }
  }, [initialData?.id]);

  useEffect(() => {
    // Skip search if the debounced value is the same as initial data (edit mode)
    if (initialData?.subjectName && debouncedName === initialData.subjectName) {
      setMatchingSubjects([]);
      setIsDuplicate(false);
      return;
    }
    searchSubjects(debouncedName);
  }, [debouncedName, searchSubjects, initialData?.subjectName]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (isDuplicate) return;
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
          <div className="relative">
            <Input
              id="subjectName"
              value={formData.subjectName || ""}
              onChange={(e) =>
                setFormData((prev) => ({ ...prev, subjectName: e.target.value }))
              }
              required
              disabled={isDisabled}
              className={`mt-1 ${isDuplicate ? "border-red-500 focus-visible:ring-red-500" : ""}`}
              placeholder={t("form.subjectNamePlaceholder")}
            />
            {searching && (
              <div className="absolute right-3 top-1/2 -translate-y-1/2 mt-0.5">
                <Loader2 className="h-4 w-4 animate-spin text-slate-400" />
              </div>
            )}
          </div>

          {/* Duplicate warning */}
          {isDuplicate && (
            <div className="mt-2 flex items-center gap-1.5 text-sm text-red-600 dark:text-red-400">
              <AlertCircle className="h-4 w-4" />
              <span>{t("form.duplicateWarning")}</span>
            </div>
          )}

          {/* Matching subjects list */}
          {matchingSubjects.length > 0 && !isDuplicate && (
            <div className="mt-2 p-2 bg-slate-50 dark:bg-slate-800  border border-slate-200 dark:border-slate-700">
              <p className="text-xs text-slate-500 dark:text-slate-400 mb-1">
                {t("form.similarSubjects")}:
              </p>
              <ul className="space-y-0.5">
                {matchingSubjects.map((subject) => (
                  <li key={subject.id} className="text-sm text-slate-700 dark:text-slate-300">
                    • {subject.subjectName} ({subject.subjectCode})
                  </li>
                ))}
              </ul>
            </div>
          )}
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
            className="mt-1 flex w-full  border border-input bg-background text-foreground px-3 py-2 text-sm shadow-sm transition-colors focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-50"
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

