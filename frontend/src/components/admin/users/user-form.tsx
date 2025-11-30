"use client";

import { useState, useEffect } from "react";
import { Label } from "@/src/components/ui/label";
import { Input } from "@/src/components/ui/input";
import { Button } from "@/src/components/ui/button";
import { InputWithIcon } from "@/src/components/ui/input-with-icon";
import { Mail, User, FileText } from "lucide-react";
import { getApiMajor } from "@/src/api/database/sdk.gen";
import type { MajorDto, UpdateUserCommand } from "@/src/api/database/types.gen";
import { useTranslations } from "next-intl";

export interface UserFormData {
  fullName?: string | null;
  email?: string | null;
  username?: string | null;
  introduction?: string | null;
  avatarUrl?: string | null;
  majorId?: string | null;
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
  const t = useTranslations("common");
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
  const [majors, setMajors] = useState<MajorDto[]>([]);
  const [loadingMajors, setLoadingMajors] = useState(false);

  useEffect(() => {
    if (initialData) {
      setFormData(initialData);
    }
  }, [initialData]);

  useEffect(() => {
    const fetchMajors = async () => {
      setLoadingMajors(true);
      try {
        const response = await getApiMajor();
        if (response.data?.items) {
          setMajors(response.data.items);
        }
      } catch (err) {
        console.error("Error fetching majors:", err);
      } finally {
        setLoadingMajors(false);
      }
    };

    fetchMajors();
  }, []);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    const command: UpdateUserCommand = {
      fullName: formData.fullName || null,
      email: formData.email || null,
      username: formData.username || null,
      introduction: formData.introduction || null,
      avatarUrl: formData.avatarUrl || null,
      majorId: formData.majorId || null,
      gender: formData.gender as any || null,
      emailConfirmed: formData.emailConfirmed ?? null,
      roles: formData.roles || null,
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

  return (
    <form id="user-form" onSubmit={handleSubmit} className="space-y-4">
      <div className="space-y-3">
        <div>
          <Label htmlFor="fullName">Họ và tên *</Label>
          <Input
            id="fullName"
            value={formData.fullName || ""}
            onChange={(e) =>
              setFormData({ ...formData, fullName: e.target.value })
            }
            required
            disabled={loading}
            className="mt-1"
          />
        </div>

        <div>
          <Label htmlFor="email">Email *</Label>
          <Input
            id="email"
            type="email"
            value={formData.email || ""}
            onChange={(e) =>
              setFormData({ ...formData, email: e.target.value })
            }
            required
            disabled={loading}
            className="mt-1"
          />
        </div>

        <div>
          <Label htmlFor="username">Tên đăng nhập</Label>
          <Input
            id="username"
            value={formData.username || ""}
            onChange={(e) =>
              setFormData({ ...formData, username: e.target.value })
            }
            disabled={loading}
            className="mt-1"
          />
        </div>

        <div>
          <Label htmlFor="majorId">Ngành học</Label>
          <select
            id="majorId"
            value={formData.majorId || ""}
            onChange={(e) =>
              setFormData({ ...formData, majorId: e.target.value || null })
            }
            disabled={loading || loadingMajors}
            className="mt-1 flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm"
          >
            <option value="">Chọn ngành</option>
            {majors
              .filter((major): major is MajorDto & { id: string } => !!major?.id)
              .map((major) => (
                <option key={major.id} value={major.id}>
                  {major.majorName || ""} ({major.majorCode || ""})
                </option>
              ))}
          </select>
        </div>

        <div>
          <Label htmlFor="gender">Giới tính</Label>
          <select
            id="gender"
            value={formData.gender || ""}
            onChange={(e) =>
              setFormData({ ...formData, gender: e.target.value || null })
            }
            disabled={loading}
            className="mt-1 flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm"
          >
            <option value="">Chọn giới tính</option>
            <option value="Male">Nam</option>
            <option value="Female">Nữ</option>
            <option value="Other">Khác</option>
          </select>
        </div>

        <div>
          <Label>Vai trò</Label>
          <div className="mt-1 flex gap-4">
            <label className="flex items-center gap-2">
              <input
                type="checkbox"
                checked={formData.roles?.includes("Admin") || false}
                onChange={() => handleRoleToggle("Admin")}
                disabled={loading}
              />
              <span className="text-sm">Admin</span>
            </label>
            <label className="flex items-center gap-2">
              <input
                type="checkbox"
                checked={formData.roles?.includes("Student") || false}
                onChange={() => handleRoleToggle("Student")}
                disabled={loading}
              />
              <span className="text-sm">Student</span>
            </label>
          </div>
        </div>
      </div>
    </form>
  );
}

