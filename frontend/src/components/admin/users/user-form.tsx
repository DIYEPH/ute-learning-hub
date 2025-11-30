"use client";

import { useState, useEffect } from "react";
import { Label } from "@/src/components/ui/label";
import { Input } from "@/src/components/ui/input";
import { useTranslations } from "next-intl";
import { useMajors } from "@/src/hooks/use-majors";
import { useFaculties } from "@/src/hooks/use-faculties";
import type { UpdateUserCommand, MajorDto2, FacultyDto2 } from "@/src/api/database/types.gen";

export interface UserFormData {
  fullName?: string | null;
  email?: string | null;
  username?: string | null;
  introduction?: string | null;
  avatarUrl?: string | null;
  majorId?: string | null;
  major?: { id?: string; faculty?: { id?: string } | null } | null;
  gender?: string | null;
  emailConfirmed?: boolean | null;
  roles?: string[] | null;
}

interface UserFormProps {
  initialData?: UserFormData;
  onSubmit: (data: UpdateUserCommand) => void | Promise<void>;
  loading?: boolean;
}

export function UserForm({ initialData, onSubmit, loading }: UserFormProps) {
  const t = useTranslations("admin.users");
  const { fetchMajors, loading: loadingMajors } = useMajors();
  const { fetchFaculties, loading: loadingFaculties } = useFaculties();
  const [formData, setFormData] = useState<UserFormData>({
    fullName: null,
    email: null,
    username: null,
    introduction: null,
    avatarUrl: null,
    majorId: null,
    gender: null,
    emailConfirmed: null,
    roles: null,
  });
  const [majors, setMajors] = useState<MajorDto2[]>([]);
  const [faculties, setFaculties] = useState<FacultyDto2[]>([]);
  const [selectedFacultyId, setSelectedFacultyId] = useState<string | null>(null);

  useEffect(() => {
    if (initialData) {
      // Extract majorId from initialData.majorId or initialData.major.id
      const majorId = initialData.majorId || initialData.major?.id || null;
      // Extract facultyId from initialData.major.faculty.id
      const facultyId = initialData.major?.faculty?.id || null;
      
      setFormData({
        ...initialData,
        majorId: majorId,
      });
      
      // Set selectedFacultyId from major.faculty.id if available
      if (facultyId) {
        setSelectedFacultyId(facultyId);
      } else {
        setSelectedFacultyId(null);
      }
    } else {
      // Reset form when no initialData (create mode)
      setFormData({
        fullName: null,
        email: null,
        username: null,
        introduction: null,
        avatarUrl: null,
        majorId: null,
        gender: null,
        emailConfirmed: null,
        roles: null,
      });
      setSelectedFacultyId(null);
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

  useEffect(() => {
    const loadMajors = async () => {
      try {
        // Load all majors
        const allMajorsResponse = await fetchMajors({
          Page: 1,
          PageSize: 1000,
        });
        
        if (allMajorsResponse?.items) {
          // Filter majors by selected faculty if one is selected
          const filteredMajors = selectedFacultyId
            ? allMajorsResponse.items.filter((m) => m.faculty?.id === selectedFacultyId)
            : allMajorsResponse.items;
          
          setMajors(filteredMajors);
          
          // Reset majorId if selected faculty changed and current major doesn't belong to it
          // Only do this if user manually changed faculty (not during initial load)
          if (selectedFacultyId && formData.majorId) {
            const major = filteredMajors.find((m) => m.id === formData.majorId);
            if (!major || major.faculty?.id !== selectedFacultyId) {
              setFormData((prev) => ({ ...prev, majorId: null }));
      }
          }
        }
      } catch (err) {
        console.error("Error loading majors:", err);
      }
    };
    loadMajors();
  }, [fetchMajors, selectedFacultyId, formData.majorId]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    // Map gender string to enum number: Other = 0, Male = 1, Female = 2
    let genderValue: number | null | undefined = undefined;
    if (formData.gender) {
      const genderMap: Record<string, number> = {
        "Male": 1,
        "Female": 2,
        "Other": 0,
      };
      genderValue = genderMap[formData.gender] ?? undefined;
    } else {
      genderValue = null;
    }
    
    const command: UpdateUserCommand = {
      fullName: formData.fullName || undefined,
      email: formData.email || undefined,
      username: formData.username || undefined,
      introduction: formData.introduction || undefined,
      avatarUrl: formData.avatarUrl || undefined,
      majorId: formData.majorId || undefined,
      gender: genderValue,
      emailConfirmed: formData.emailConfirmed ?? undefined,
      roles: formData.roles || undefined,
    };
    await onSubmit(command);
  };

  const handleRoleToggle = (role: string) => {
    setFormData((prev) => {
      const currentRoles = prev.roles || [];
      const newRoles = currentRoles.includes(role)
        ? currentRoles.filter((r) => r !== role)
        : [...currentRoles, role];
      return { ...prev, roles: newRoles };
    });
  };

  const handleFacultyChange = (facultyId: string) => {
    setSelectedFacultyId(facultyId || null);
    setFormData((prev) => ({ ...prev, majorId: null }));
  };

  const isDisabled = loading || loadingMajors || loadingFaculties;
  const selectClassName = "mt-1 flex h-9 w-full rounded-md border border-input bg-background text-foreground px-3 py-1 text-sm shadow-sm transition-colors focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-50";

  return (
    <form id="user-form" onSubmit={handleSubmit} className="space-y-4">
      <div className="space-y-3 grid grid-cols-1 md:grid-cols-2 gap-4">
        <div>
          <Label htmlFor="fullName">{t("form.fullName")} *</Label>
          <Input
            id="fullName"
            value={formData.fullName || ""}
            onChange={(e) =>
              setFormData((prev) => ({ ...prev, fullName: e.target.value }))
            }
            required
            disabled={isDisabled}
            className="mt-1"
          />
        </div>

        <div>
          <Label htmlFor="email">{t("form.email")} *</Label>
          <Input
            id="email"
            type="email"
            value={formData.email || ""}
            onChange={(e) =>
              setFormData((prev) => ({ ...prev, email: e.target.value }))
            }
            required
            disabled={isDisabled}
            className="mt-1"
          />
        </div>

        <div>
          <Label htmlFor="username">{t("form.username")}</Label>
          <Input
            id="username"
            value={formData.username || ""}
            onChange={(e) =>
              setFormData((prev) => ({ ...prev, username: e.target.value }))
            }
            disabled={isDisabled}
            className="mt-1"
          />
        </div>

        <div>
          <Label htmlFor="facultyId">{t("form.faculty")}</Label>
          <select
            id="facultyId"
            value={selectedFacultyId || ""}
            onChange={(e) => handleFacultyChange(e.target.value)}
            disabled={isDisabled}
            className={selectClassName}
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
          <Label htmlFor="majorId">{t("form.major")}</Label>
          <select
            id="majorId"
            value={formData.majorId || ""}
            onChange={(e) =>
              setFormData((prev) => ({ ...prev, majorId: e.target.value || null }))
            }
            disabled={isDisabled || !selectedFacultyId}
            className={selectClassName}
          >
            <option value="">{t("form.selectMajor")}</option>
            {majors
              .filter((major): major is MajorDto2 & { id: string } => !!major?.id)
              .map((major) => (
                <option key={major.id} value={major.id}>
                  {major.majorName || ""} ({major.majorCode || ""})
                </option>
              ))}
          </select>
        </div>

        <div>
          <Label htmlFor="gender">{t("form.gender")}</Label>
          <select
            id="gender"
            value={formData.gender || ""}
            onChange={(e) =>
              setFormData((prev) => ({ ...prev, gender: e.target.value || null }))
            }
            disabled={isDisabled}
            className={selectClassName}
          >
            <option value="">{t("form.selectGender")}</option>
            <option value="Male">{t("form.genderMale")}</option>
            <option value="Female">{t("form.genderFemale")}</option>
            <option value="Other">{t("form.genderOther")}</option>
          </select>
        </div>

        <div className="md:col-span-2">
          <Label htmlFor="introduction">{t("form.introduction")}</Label>
          <textarea
            id="introduction"
            value={formData.introduction || ""}
            onChange={(e) =>
              setFormData((prev) => ({ ...prev, introduction: e.target.value }))
            }
            disabled={isDisabled}
            rows={4}
            className="mt-1 flex w-full rounded-md border border-input bg-background text-foreground px-3 py-2 text-sm shadow-sm transition-colors placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-50 resize-y"
          />
        </div>

        <div>
          <Label htmlFor="avatarUrl">{t("form.avatarUrl")}</Label>
          <Input
            id="avatarUrl"
            type="url"
            value={formData.avatarUrl || ""}
            onChange={(e) =>
              setFormData((prev) => ({ ...prev, avatarUrl: e.target.value }))
            }
            disabled={isDisabled}
            className="mt-1"
            placeholder="https://..."
          />
        </div>

        <div className="md:col-span-2">
          <Label>{t("form.roles")}</Label>
          <div className="mt-1 flex flex-wrap gap-4">
            <label className="flex items-center gap-2">
              <input
                type="checkbox"
                checked={formData.roles?.includes("Admin") || false}
                onChange={() => handleRoleToggle("Admin")}
                disabled={isDisabled}
                className="cursor-pointer"
              />
              <span className="text-sm">{t("form.roleAdmin")}</span>
            </label>
            <label className="flex items-center gap-2">
              <input
                type="checkbox"
                checked={formData.roles?.includes("Student") || false}
                onChange={() => handleRoleToggle("Student")}
                disabled={isDisabled}
                className="cursor-pointer"
              />
              <span className="text-sm">{t("form.roleStudent")}</span>
            </label>
          </div>
        </div>

        <div className="md:col-span-2">
          <label className="flex items-center gap-2">
            <input
              type="checkbox"
              checked={formData.emailConfirmed ?? false}
              onChange={(e) =>
                setFormData((prev) => ({ ...prev, emailConfirmed: e.target.checked }))
              }
              disabled={isDisabled}
              className="cursor-pointer"
            />
            <span className="text-sm">{t("form.emailConfirmed")}</span>
          </label>
        </div>
      </div>
    </form>
  );
}
