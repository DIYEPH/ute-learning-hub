"use client";

import { useState, useEffect } from "react";
import { Label } from "@/src/components/ui/label";
import { Input } from "@/src/components/ui/input";
import { useTranslations } from "next-intl";
import type { UpdateTypeCommand, CreateTypeCommand } from "@/src/api/database/types.gen";

export interface TypeFormData {
  typeName?: string | null;
}

interface TypeFormProps {
  initialData?: TypeFormData;
  onSubmit: (data: CreateTypeCommand | UpdateTypeCommand) => void | Promise<void>;
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

  useEffect(() => {
    if (initialData) {
      setFormData(initialData);
    }
  }, [initialData]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    const command: CreateTypeCommand | UpdateTypeCommand = {
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
          <Input
            id="typeName"
            value={formData.typeName || ""}
            onChange={(e) =>
              setFormData((prev) => ({ ...prev, typeName: e.target.value }))
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

