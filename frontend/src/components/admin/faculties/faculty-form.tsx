"use client";

import { useState, useEffect } from "react";
import { Label } from "@/src/components/ui/label";
import { Input } from "@/src/components/ui/input";
import type { UpdateFacultyCommand, CreateFacultyCommand} from "@/src/api/database/types.gen";

export interface FacultyFormData {
  facultyName?: string | null;
  facultyCode?: string | null;
  logo?: string | null;
}

interface FacultyFormProps {
  initialData?: FacultyFormData;
  onSubmit: (data: CreateFacultyCommand | UpdateFacultyCommand) => void | Promise<void>;
  loading?: boolean;
}

export function FacultyForm({ initialData, onSubmit, loading }: FacultyFormProps) {
  const [formData, setFormData] = useState<FacultyFormData>({
    facultyName: null,
    facultyCode: null,
    logo: null,
  });

  useEffect(() => {
    if (initialData) {
      setFormData(initialData);
    }
  }, [initialData]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    const command = {
      facultyName: formData.facultyName || undefined,
      facultyCode: formData.facultyCode || undefined,
      logo: formData.logo || undefined,
    };
    await onSubmit(command);
  };

  return (
    <form id="faculty-form" onSubmit={handleSubmit} className="space-y-4">
      <div className="space-y-3">
        <div>
          <Label htmlFor="facultyName">Tên khoa *</Label>
          <Input
            id="facultyName"
            value={formData.facultyName || ""}
            onChange={(e) =>
              setFormData({ ...formData, facultyName: e.target.value })
            }
            required
            disabled={loading}
            className="mt-1"
          />
        </div>

        <div>
          <Label htmlFor="facultyCode">Mã khoa *</Label>
          <Input
            id="facultyCode"
            value={formData.facultyCode || ""}
            onChange={(e) =>
              setFormData({ ...formData, facultyCode: e.target.value })
            }
            required
            disabled={loading}
            className="mt-1"
          />
        </div>

        <div>
          <Label htmlFor="logo">Logo URL</Label>
          <Input
            id="logo"
            type="url"
            value={formData.logo || ""}
            onChange={(e) =>
              setFormData({ ...formData, logo: e.target.value })
            }
            disabled={loading}
            placeholder="https://..."
            className="mt-1"
          />
        </div>
      </div>
    </form>
  );
}

