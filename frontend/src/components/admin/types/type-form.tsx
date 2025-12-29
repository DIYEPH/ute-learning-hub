"use client";

import { useState, useEffect, useCallback } from "react";
import { Label } from "@/src/components/ui/label";
import { Input } from "@/src/components/ui/input";
import { useTranslations } from "next-intl";
import { useDebounce } from "@/src/hooks/use-debounce";
import { getApiType } from "@/src/api/database/sdk.gen";
import type { UpdateTypeCommandRequest, CreateTypeCommand, TypeDto } from "@/src/api/database/types.gen";
import { AlertCircle, Loader2 } from "lucide-react";

export interface TypeFormData {
  id?: string;
  typeName?: string | null;
}

interface TypeFormProps {
  initialData?: TypeFormData;
  onSubmit: (data: CreateTypeCommand | UpdateTypeCommandRequest) => void | Promise<void>;
  loading?: boolean;
}

export function TypeForm({
  initialData,
  onSubmit,
  loading,
}: TypeFormProps) {
  const t = useTranslations("admin.types");
  const [formData, setFormData] = useState<TypeFormData>({
    typeName: null,
  });

  // Debounce search state
  const [searching, setSearching] = useState(false);
  const [matchingTypes, setMatchingTypes] = useState<TypeDto[]>([]);
  const [isDuplicate, setIsDuplicate] = useState(false);

  const debouncedName = useDebounce(formData.typeName || "", 400);

  useEffect(() => {
    if (initialData) {
      setFormData(initialData);
    }
  }, [initialData]);

  // Search for matching types when name changes
  const searchTypes = useCallback(async (searchTerm: string) => {
    if (!searchTerm.trim()) {
      setMatchingTypes([]);
      setIsDuplicate(false);
      return;
    }

    setSearching(true);
    try {
      const response = await getApiType({ query: { SearchTerm: searchTerm, Page: 1, PageSize: 5 } });
      const data = (response as unknown as { data: { items?: TypeDto[] } })?.data || response as { items?: TypeDto[] };
      const items = data?.items || [];

      // Filter out current item if editing
      const filtered = initialData?.id
        ? items.filter(item => item.id !== initialData.id)
        : items;

      setMatchingTypes(filtered);

      // Check for exact duplicate
      const exactMatch = filtered.some(
        item => item.typeName?.toLowerCase() === searchTerm.toLowerCase()
      );
      setIsDuplicate(exactMatch);
    } catch (error) {
      console.error("Error searching types:", error);
      setMatchingTypes([]);
      setIsDuplicate(false);
    } finally {
      setSearching(false);
    }
  }, [initialData?.id]);

  useEffect(() => {
    // Skip search if the debounced value is the same as initial data (edit mode)
    if (initialData?.typeName && debouncedName === initialData.typeName) {
      setMatchingTypes([]);
      setIsDuplicate(false);
      return;
    }
    searchTypes(debouncedName);
  }, [debouncedName, searchTypes, initialData?.typeName]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (isDuplicate) {
      return; // Don't submit if duplicate
    }
    const command: CreateTypeCommand | UpdateTypeCommandRequest = {
      typeName: formData.typeName || undefined,
    };
    await onSubmit(command);
  };

  const isDisabled = loading;

  return (
    <form id="type-form" onSubmit={handleSubmit} className="space-y-4">
      <div className="space-y-3">
        <div>
          <Label htmlFor="typeName">{t("form.typeName")} *</Label>
          <div className="relative">
            <Input
              id="typeName"
              value={formData.typeName || ""}
              onChange={(e) =>
                setFormData((prev) => ({ ...prev, typeName: e.target.value }))
              }
              required
              disabled={isDisabled}
              className={`mt-1 ${isDuplicate ? "border-red-500 focus-visible:ring-red-500" : ""}`}
              placeholder={t("form.typeNamePlaceholder")}
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

          {/* Matching types list */}
          {matchingTypes.length > 0 && !isDuplicate && (
            <div className="mt-2 p-2 bg-muted border border-border">
              <p className="text-xs text-muted-foreground mb-1">
                {t("form.similarTypes")}:
              </p>
              <ul className="space-y-0.5">
                {matchingTypes.map((type) => (
                  <li key={type.id} className="text-sm text-foreground">
                    • {type.typeName}
                  </li>
                ))}
              </ul>
            </div>
          )}
        </div>
      </div>
    </form>
  );
}

