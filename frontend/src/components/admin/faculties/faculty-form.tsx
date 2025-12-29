"use client";

import { useState, useEffect, useCallback } from "react";
import { Label } from "@/src/components/ui/label";
import { Input } from "@/src/components/ui/input";
import { Button } from "@/src/components/ui/button";
import { Upload, Link as LinkIcon, X, AlertCircle, Loader2 } from "lucide-react";
import { useTranslations } from "next-intl";
import { useDebounce } from "@/src/hooks/use-debounce";
import { getApiFaculty } from "@/src/api/database/sdk.gen";
import type { UpdateFacultyCommandRequest, CreateFacultyCommand, FacultyDetailDto } from "@/src/api/database/types.gen";

export interface FacultyFormData {
  id?: string;
  facultyName?: string | null;
  facultyCode?: string | null;
  logo?: string | null;
}

interface FacultyFormProps {
  initialData?: FacultyFormData;
  onSubmit: (data: CreateFacultyCommand | UpdateFacultyCommandRequest) => void | Promise<void>;
  loading?: boolean;
  onUploadLogo?: (file: File) => Promise<string | null>;
  uploadingLogo?: boolean;
  uploadError?: string | null;
}

export function FacultyForm({
  initialData,
  onSubmit,
  loading,
  onUploadLogo,
  uploadingLogo = false,
  uploadError: externalUploadError,
}: FacultyFormProps) {
  const t = useTranslations("admin.faculties");
  const [formData, setFormData] = useState<FacultyFormData>({
    facultyName: null,
    facultyCode: null,
    logo: null,
  });
  const [uploadMode, setUploadMode] = useState<"url" | "file">("url");
  const [localUploadError, setLocalUploadError] = useState<string | null>(null);

  // Debounce search state
  const [searching, setSearching] = useState(false);
  const [matchingFaculties, setMatchingFaculties] = useState<FacultyDetailDto[]>([]);
  const [isDuplicate, setIsDuplicate] = useState(false);

  const debouncedName = useDebounce(formData.facultyName || "", 400);

  useEffect(() => {
    if (initialData) {
      setFormData(initialData);
    }
  }, [initialData]);

  // Search for matching faculties when name changes
  const searchFaculties = useCallback(async (searchTerm: string) => {
    if (!searchTerm.trim()) {
      setMatchingFaculties([]);
      setIsDuplicate(false);
      return;
    }

    setSearching(true);
    try {
      const response = await getApiFaculty({ query: { SearchTerm: searchTerm, Page: 1, PageSize: 5 } });
      const data = (response as unknown as { data: { items?: FacultyDetailDto[] } })?.data || response as { items?: FacultyDetailDto[] };
      const items = data?.items || [];

      // Filter out current item if editing
      const filtered = initialData?.id
        ? items.filter(item => item.id !== initialData.id)
        : items;

      setMatchingFaculties(filtered);

      // Check for exact duplicate
      const exactMatch = filtered.some(
        item => item.facultyName?.toLowerCase() === searchTerm.toLowerCase()
      );
      setIsDuplicate(exactMatch);
    } catch (error) {
      console.error("Error searching faculties:", error);
      setMatchingFaculties([]);
      setIsDuplicate(false);
    } finally {
      setSearching(false);
    }
  }, [initialData?.id]);

  useEffect(() => {
    // Skip search if the debounced value is the same as initial data (edit mode)
    if (initialData?.facultyName && debouncedName === initialData.facultyName) {
      setMatchingFaculties([]);
      setIsDuplicate(false);
      return;
    }
    searchFaculties(debouncedName);
  }, [debouncedName, searchFaculties, initialData?.facultyName]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (isDuplicate) return;
    const command: CreateFacultyCommand | UpdateFacultyCommandRequest = {
      facultyName: formData.facultyName || undefined,
      facultyCode: formData.facultyCode || undefined,
      logo: formData.logo || undefined,
    };
    await onSubmit(command);
  };

  const handleFileUpload = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file || !onUploadLogo) return;

    setLocalUploadError(null);

    try {
      const logoUrl = await onUploadLogo(file);
      if (logoUrl) {
        setFormData((prev) => ({ ...prev, logo: logoUrl }));
      }
    } catch (err) {
      setLocalUploadError(externalUploadError || "Không thể upload file");
    }
  };

  const clearLogo = () => {
    setFormData((prev) => ({ ...prev, logo: null }));
  };

  const uploadError = externalUploadError || localUploadError;
  const isUploading = uploadingLogo;
  const isDisabled = loading || isUploading;

  return (
    <form id="faculty-form" onSubmit={handleSubmit} className="space-y-4">
      <div className="space-y-3 grid grid-cols-1 md:grid-cols-2 gap-4">
        <div>
          <Label htmlFor="facultyName">{t("form.facultyName")} *</Label>
          <div className="relative">
            <Input
              id="facultyName"
              value={formData.facultyName || ""}
              onChange={(e) =>
                setFormData((prev) => ({ ...prev, facultyName: e.target.value }))
              }
              required
              disabled={isDisabled}
              className={`mt-1 ${isDuplicate ? "border-red-500 focus-visible:ring-red-500" : ""}`}
              placeholder={t("form.facultyNamePlaceholder")}
            />
            {searching && (
              <div className="absolute right-3 top-1/2 -translate-y-1/2 mt-0.5">
                <Loader2 className="h-4 w-4 animate-spin text-muted-foreground" />
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

          {/* Matching faculties list */}
          {matchingFaculties.length > 0 && !isDuplicate && (
            <div className="mt-2 p-2 bg-muted border border-border">
              <p className="text-xs text-muted-foreground mb-1">
                {t("form.similarFaculties")}:
              </p>
              <ul className="space-y-0.5">
                {matchingFaculties.map((faculty) => (
                  <li key={faculty.id} className="text-sm text-foreground">
                    • {faculty.facultyName} ({faculty.facultyCode})
                  </li>
                ))}
              </ul>
            </div>
          )}
        </div>
        <div>
          <Label htmlFor="facultyCode">{t("form.facultyCode")} *</Label>
          <Input
            id="facultyCode"
            value={formData.facultyCode || ""}
            onChange={(e) =>
              setFormData((prev) => ({ ...prev, facultyCode: e.target.value }))
            }
            required
            disabled={isDisabled}
            className="mt-1"
          />
        </div>
        <div className="md:col-span-2">
          <Label>{t("form.logo")}</Label>
          <div className="mt-2 space-y-2">
            <div className="flex flex-wrap gap-2">
              <Button
                type="button"
                variant={uploadMode === "url" ? "default" : "outline"}
                size="sm"
                onClick={() => setUploadMode("url")}
                disabled={isDisabled}
              >
                <LinkIcon size={16} className="mr-1" />
                {t("form.logoUrl")}
              </Button>
              <Button
                type="button"
                variant={uploadMode === "file" ? "default" : "outline"}
                size="sm"
                onClick={() => setUploadMode("file")}
                disabled={isDisabled}
              >
                <Upload size={16} className="mr-1" />
                {t("form.upload")}
              </Button>
            </div>
            {uploadMode === "url" && (
              <Input
                id="logo"
                type="url"
                value={formData.logo || ""}
                onChange={(e) =>
                  setFormData((prev) => ({ ...prev, logo: e.target.value }))
                }
                disabled={isDisabled}
                placeholder={t("form.logoUrlPlaceholder")}
              />
            )}
            {uploadMode === "file" && (
              <div className="space-y-2">
                <Input
                  type="file"
                  accept="image/*"
                  onChange={handleFileUpload}
                  disabled={isDisabled}
                />
                {isUploading && (
                  <p className="text-xs text-muted-foreground">
                    {t("form.uploading")}
                  </p>
                )}
                {uploadError && (
                  <p className="text-xs text-red-600 dark:text-red-400">{uploadError}</p>
                )}
              </div>
            )}
            {formData.logo && (
              <div className="flex items-center gap-2 p-2 bg-muted rounded">
                <img
                  src={formData.logo}
                  alt={t("form.logoPreview")}
                  className="h-12 w-12 object-contain rounded"
                  onError={(e) => {
                    (e.target as HTMLImageElement).style.display = "none";
                  }}
                />
                <span className="text-xs text-muted-foreground flex-1 truncate">
                  {formData.logo}
                </span>
                <Button
                  type="button"
                  variant="ghost"
                  size="sm"
                  onClick={clearLogo}
                  disabled={isDisabled}
                  aria-label={t("form.removeLogo")}
                >
                  <X size={16} />
                </Button>
              </div>
            )}
          </div>
        </div>
      </div>
    </form>
  );
}

