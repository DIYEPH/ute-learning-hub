"use client";

import { useState, useEffect } from "react";
import { Label } from "@/src/components/ui/label";
import { Input } from "@/src/components/ui/input";
import { Button } from "@/src/components/ui/button";
import { Upload, Link as LinkIcon, X } from "lucide-react";
import { useTranslations } from "next-intl";
import type { UpdateFacultyCommand, CreateFacultyCommand } from "@/src/api/database/types.gen";

export interface FacultyFormData {
  facultyName?: string | null;
  facultyCode?: string | null;
  logo?: string | null;
}

interface FacultyFormProps {
  initialData?: FacultyFormData;
  onSubmit: (data: CreateFacultyCommand | UpdateFacultyCommand) => void | Promise<void>;
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

  useEffect(() => {
    if (initialData) {
      setFormData(initialData);
    }
  }, [initialData]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    const command: CreateFacultyCommand | UpdateFacultyCommand = {
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
          <Input
            id="facultyName"
            value={formData.facultyName || ""}
            onChange={(e) =>
              setFormData((prev) => ({ ...prev, facultyName: e.target.value }))
            }
            required
            disabled={isDisabled}
            className="mt-1"
          />
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
                  <p className="text-xs text-slate-500 dark:text-slate-400">
                    {t("form.uploading")}
                  </p>
                )}
                {uploadError && (
                  <p className="text-xs text-red-600 dark:text-red-400">{uploadError}</p>
                )}
              </div>
            )}
            {formData.logo && (
              <div className="flex items-center gap-2 p-2 bg-slate-50 dark:bg-slate-800 rounded">
                <img
                  src={formData.logo}
                  alt={t("form.logoPreview")}
                  className="h-12 w-12 object-contain rounded"
                  onError={(e) => {
                    (e.target as HTMLImageElement).style.display = "none";
                  }}
                />
                <span className="text-xs text-slate-600 dark:text-slate-400 flex-1 truncate">
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
