"use client";

import { useState, useEffect } from "react";
import { Label } from "@/src/components/ui/label";
import { Input } from "@/src/components/ui/input";
import { useTranslations } from "next-intl";
import { useFaculties } from "@/src/hooks/use-faculties";
import type { UpdateMajorCommand, CreateMajorCommand, FacultyDto2 } from "@/src/api/database/types.gen";

export interface MajorFormData {
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

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
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
            className="mt-1 flex h-9 w-full rounded-md border border-input bg-background text-foreground px-3 py-1 text-sm shadow-sm transition-colors focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-50"
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
          <Input
            id="majorName"
            value={formData.majorName || ""}
            onChange={(e) =>
              setFormData((prev) => ({ ...prev, majorName: e.target.value }))
            }
            required
            disabled={isDisabled}
            className="mt-1"
          />
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

