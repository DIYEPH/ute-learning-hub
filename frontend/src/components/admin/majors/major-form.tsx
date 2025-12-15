"use client";

import { useState, useEffect, useCallback } from "react";
import { Label } from "@/src/components/ui/label";
import { Input } from "@/src/components/ui/input";
import { useTranslations } from "next-intl";
import { useFaculties } from "@/src/hooks/use-faculties";
import { useDebounce } from "@/src/hooks/use-debounce";
import { getApiMajor } from "@/src/api/database/sdk.gen";
import type { UpdateMajorCommand, CreateMajorCommand, FacultyDto2, MajorDto2 } from "@/src/api/database/types.gen";
import { AlertCircle, Loader2 } from "lucide-react";

export interface MajorFormData {
  id?: string;
  majorName?: string | null;
  majorCode?: string | null;
  facultyId?: string | null;
}

interface MajorFormProps {
  initialData?: MajorFormData;
  onSubmit: (data: CreateMajorCommand | UpdateMajorCommand) => void | Promise<void>;
  loading?: boolean;
}

export function MajorForm({
  initialData,
  onSubmit,
  loading,
}: MajorFormProps) {
  const t = useTranslations("admin.majors");
  const { fetchFaculties, loading: loadingFaculties } = useFaculties();
  const [formData, setFormData] = useState<MajorFormData>({
    majorName: null,
    majorCode: null,
    facultyId: null,
  });
  const [faculties, setFaculties] = useState<FacultyDto2[]>([]);

  // Debounce search state
  const [searching, setSearching] = useState(false);
  const [matchingMajors, setMatchingMajors] = useState<MajorDto2[]>([]);
  const [isDuplicate, setIsDuplicate] = useState(false);

  const debouncedName = useDebounce(formData.majorName || "", 400);

  useEffect(() => {
    if (initialData) {
      setFormData(initialData);
    }
  }, [initialData]);

  useEffect(() => {
    const loadFaculties = async () => {
      try {
        const response = await fetchFaculties({ Page: 1, PageSize: 1000 });
        if (response?.items) {
          setFaculties(response.items);
        }
      } catch (err) {
        console.error("Error loading faculties:", err);
      }
    };
    loadFaculties();
  }, [fetchFaculties]);

  // Search for matching majors when name changes
  const searchMajors = useCallback(async (searchTerm: string) => {
    if (!searchTerm.trim()) {
      setMatchingMajors([]);
      setIsDuplicate(false);
      return;
    }

    setSearching(true);
    try {
      const response = await getApiMajor({ query: { SearchTerm: searchTerm, Page: 1, PageSize: 5 } });
      const data = (response as unknown as { data: { items?: MajorDto2[] } })?.data || response as { items?: MajorDto2[] };
      const items = data?.items || [];

      // Filter out current item if editing
      const filtered = initialData?.id
        ? items.filter(item => item.id !== initialData.id)
        : items;

      setMatchingMajors(filtered);

      // Check for exact duplicate
      const exactMatch = filtered.some(
        item => item.majorName?.toLowerCase() === searchTerm.toLowerCase()
      );
      setIsDuplicate(exactMatch);
    } catch (error) {
      console.error("Error searching majors:", error);
      setMatchingMajors([]);
      setIsDuplicate(false);
    } finally {
      setSearching(false);
    }
  }, [initialData?.id]);

  useEffect(() => {
    // Skip search if the debounced value is the same as initial data (edit mode)
    if (initialData?.majorName && debouncedName === initialData.majorName) {
      setMatchingMajors([]);
      setIsDuplicate(false);
      return;
    }
    searchMajors(debouncedName);
  }, [debouncedName, searchMajors, initialData?.majorName]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (isDuplicate) return;
    const command: CreateMajorCommand | UpdateMajorCommand = {
      majorName: formData.majorName || undefined,
      majorCode: formData.majorCode || undefined,
      facultyId: formData.facultyId || undefined,
    };
    await onSubmit(command);
  };

  const isDisabled = loading || loadingFaculties;

  return (
    <form id="major-form" onSubmit={handleSubmit} className="space-y-4">
      <div className="space-y-3 grid grid-cols-1 md:grid-cols-2 gap-4">
        <div>
          <Label htmlFor="facultyId">{t("form.faculty")} *</Label>
          <select
            id="facultyId"
            value={formData.facultyId || ""}
            onChange={(e) =>
              setFormData((prev) => ({ ...prev, facultyId: e.target.value || null }))
            }
            required
            disabled={isDisabled}
            className="mt-1 flex h-9 w-full  border border-input bg-background text-foreground px-3 py-1 text-sm shadow-sm transition-colors focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-50"
          >
            <option value="">{t("form.selectFaculty")}</option>
            {faculties
              .filter((faculty): faculty is FacultyDto2 & { id: string } => !!faculty?.id)
              .map((faculty) => (
                <option key={faculty.id} value={faculty.id}>
                  {faculty.facultyName || ""} ({faculty.facultyCode || ""})
                </option>
              ))}
          </select>
        </div>
        <div>
          <Label htmlFor="majorName">{t("form.majorName")} *</Label>
          <div className="relative">
            <Input
              id="majorName"
              value={formData.majorName || ""}
              onChange={(e) =>
                setFormData((prev) => ({ ...prev, majorName: e.target.value }))
              }
              required
              disabled={isDisabled}
              className={`mt-1 ${isDuplicate ? "border-red-500 focus-visible:ring-red-500" : ""}`}
              placeholder={t("form.majorNamePlaceholder")}
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

          {/* Matching majors list */}
          {matchingMajors.length > 0 && !isDuplicate && (
            <div className="mt-2 p-2 bg-slate-50 dark:bg-slate-800  border border-slate-200 dark:border-slate-700">
              <p className="text-xs text-slate-500 dark:text-slate-400 mb-1">
                {t("form.similarMajors")}:
              </p>
              <ul className="space-y-0.5">
                {matchingMajors.map((major) => (
                  <li key={major.id} className="text-sm text-slate-700 dark:text-slate-300">
                    • {major.majorName} ({major.majorCode})
                  </li>
                ))}
              </ul>
            </div>
          )}
        </div>
        <div>
          <Label htmlFor="majorCode">{t("form.majorCode")} *</Label>
          <Input
            id="majorCode"
            value={formData.majorCode || ""}
            onChange={(e) =>
              setFormData((prev) => ({ ...prev, majorCode: e.target.value }))
            }
            required
            disabled={isDisabled}
            className="mt-1"
          />
        </div>
      </div>
    </form>
  );
}

