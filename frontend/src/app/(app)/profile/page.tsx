
"use client";

import React from "react";
import { Loader2, Edit, Image as ImageIcon } from "lucide-react";
import { useRouter } from "next/navigation";

import { useUserProfile } from "@/src/hooks/use-user-profile";
import { useMajors } from "@/src/hooks/use-majors";
import { useFileUpload } from "@/src/hooks/use-file-upload";
import { Button } from "@/src/components/ui/button";
import { Badge } from "@/src/components/ui/badge";
import { Avatar, AvatarFallback, AvatarImage } from "@/src/components/ui/avatar";
import { putApiAccountProfile, getApiUserByIdTrustHistory } from "@/src/api/database/sdk.gen";
import type { UpdateProfileCommand, MajorDto2, UserTrustHistoryDto } from "@/src/api/database/types.gen";
import { Input } from "@/src/components/ui/input";
import { useNotification } from "@/src/components/ui/notification-center";

const ProfilePage = () => {
  const router = useRouter();
  const { profile, loading, error } = useUserProfile();
  const { fetchMajors, loading: loadingMajors } = useMajors();
  const { uploadFile } = useFileUpload();
  const { success: notifySuccess, error: notifyError } = useNotification();

  const [majors, setMajors] = React.useState<MajorDto2[]>([]);
  const [trustHistory, setTrustHistory] = React.useState<UserTrustHistoryDto[]>([]);
  const [loadingTrust, setLoadingTrust] = React.useState(false);

  const [form, setForm] = React.useState<UpdateProfileCommand>({
    fullName: profile?.fullName ?? "",
    introduction: profile?.introduction ?? "",
    majorId: profile?.major?.id ?? null,
    // Backend: Gender { Other = 0, Male = 1, Female = 2 }
    gender: (profile?.gender as any) ?? null,
    avatarUrl: profile?.avatarUrl ?? null,
  });
  const [saving, setSaving] = React.useState(false);
  const [saveError, setSaveError] = React.useState<string | null>(null);
  const [saveSuccess, setSaveSuccess] = React.useState(false);
  const [avatarUploading, setAvatarUploading] = React.useState(false);
  const fileInputRef = React.useRef<HTMLInputElement | null>(null);

  React.useEffect(() => {
    if (!profile) return;
    setForm({
      fullName: profile.fullName ?? "",
      introduction: profile.introduction ?? "",
      majorId: profile.major?.id ?? null,
      gender: (profile.gender as any) ?? null,
      avatarUrl: profile.avatarUrl ?? null,
    });
  }, [profile]);

  React.useEffect(() => {
    const loadMajors = async () => {
      try {
        const response = await fetchMajors({ Page: 1, PageSize: 1000 });
        if ((response as any)?.items) {
          setMajors((response as any).items as MajorDto2[]);
        }
      } catch (err) {
        console.error("Error loading majors:", err);
      }
    };

    void loadMajors();
  }, [fetchMajors]);

  React.useEffect(() => {
    const loadTrust = async () => {
      if (!profile?.id) return;
      try {
        setLoadingTrust(true);
        const response = await getApiUserByIdTrustHistory({
          path: { id: profile.id },
        });
        const payload = (response as any)?.data || response;
        if (Array.isArray(payload)) {
          setTrustHistory(payload as UserTrustHistoryDto[]);
        }
      } catch (err) {
        console.error("Error loading trust history:", err);
      } finally {
        setLoadingTrust(false);
      }
    };

    void loadTrust();
  }, [profile?.id]);

  const handleChange = (field: keyof UpdateProfileCommand, value: any) => {
    setForm((prev) => ({
      ...prev,
      [field]: value,
    }));
    setSaveSuccess(false);
    setSaveError(null);
  };

  const handleAvatarClick = () => {
    fileInputRef.current?.click();
  };

  const handleAvatarFileChange = async (
    event: React.ChangeEvent<HTMLInputElement>,
  ) => {
    const file = event.target.files?.[0];
    if (!file) return;

    if (!file.type.startsWith("image/")) {
      setSaveError("Vui lòng chọn tệp hình ảnh hợp lệ.");
      event.target.value = "";
      return;
    }

    setAvatarUploading(true);
    setSaveError(null);
    setSaveSuccess(false);

    try {
      const uploaded = await uploadFile(file, "AvatarUser");
      if (!uploaded.fileUrl) {
        throw new Error("Phản hồi upload avatar không hợp lệ.");
      }
      handleChange("avatarUrl", uploaded.fileUrl);
    } catch (err: any) {
      setSaveError(
        err?.message || "Không thể tải ảnh đại diện lên. Vui lòng thử lại.",
      );
    } finally {
      setAvatarUploading(false);
      event.target.value = "";
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!profile) return;

    setSaving(true);
    setSaveError(null);
    setSaveSuccess(false);

    try {
      await putApiAccountProfile<true>({
        body: {
          fullName: form.fullName,
          introduction: form.introduction,
          majorId: form.majorId,
          gender: form.gender,
          avatarUrl: form.avatarUrl ?? null,
        },
        throwOnError: true,
      });
      setSaveSuccess(true);
      notifySuccess("Đã lưu hồ sơ thành công.");
    } catch (err: any) {
      const message =
        err?.response?.data?.message ||
        err?.message ||
        "Không thể cập nhật hồ sơ";
      setSaveError(message);
      notifyError(message);
    } finally {
      setSaving(false);
    }
  };

  if (loading && !profile) {
    return (
      <div className="flex h-[50vh] items-center justify-center">
        <Loader2 className="h-6 w-6 animate-spin text-sky-500" />
      </div>
    );
  }

  if (!profile || error) {
    return (
      <div className="space-y-4">
        <h1 className="text-2xl font-semibold text-foreground">Hồ sơ</h1>
        <div className="rounded-md border border-red-200 bg-red-50 p-4 text-sm text-red-600 dark:border-red-900 dark:bg-red-950 dark:text-red-300">
          {!profile
            ? "Không thể tải thông tin hồ sơ. Vui lòng đăng nhập lại."
            : error}
        </div>
      </div>
    );
  }

  const initials =
    (profile.fullName || profile.username || profile.email || "?")
      .split(" ")
      .map((p) => p[0])
      .join("")
      .slice(0, 2)
      .toUpperCase();

  return (
    <div className="space-y-6">
      <div className="flex flex-col justify-between gap-4 md:flex-row md:items-center">
        <div className="flex items-center gap-4">
          <div className="relative">
            <Avatar className="h-16 w-16">
              <AvatarImage
                src={form.avatarUrl || profile.avatarUrl || undefined}
                alt={profile.fullName || ""}
              />
              <AvatarFallback>{initials}</AvatarFallback>
            </Avatar>
            <button
              type="button"
              onClick={handleAvatarClick}
              className="absolute bottom-0 right-0 flex h-7 w-7 items-center justify-center rounded-full border border-slate-200 bg-white text-slate-600 shadow-sm hover:bg-slate-50 dark:border-slate-700 dark:bg-slate-900 dark:text-slate-200"
              disabled={avatarUploading}
              aria-label="Chọn ảnh đại diện mới"
            >
              {avatarUploading ? (
                <Loader2 className="h-3 w-3 animate-spin" />
              ) : (
                <ImageIcon className="h-3 w-3" />
              )}
            </button>
            <input
              ref={fileInputRef}
              type="file"
              accept="image/*"
              className="hidden"
              onChange={handleAvatarFileChange}
            />
          </div>
          <div>
            <h1 className="text-2xl font-semibold text-foreground">
              {profile.fullName || profile.username || profile.email}
            </h1>
            <p className="text-sm text-slate-500 dark:text-slate-400">
              {profile.email}
              {profile.username ? ` • @${profile.username}` : ""}
            </p>
            <p className="mt-1 text-xs text-slate-500 dark:text-slate-400">
              Giới tính:{" "}
              {form.gender === 1
                ? "Nam"
                : form.gender === 2
                ? "Nữ"
                : form.gender === 0
                ? "Khác"
                : "Không xác định"}
            </p>
          </div>
        </div>

        <div className="flex flex-wrap gap-2">
          {profile.trustLevel && (
            <Badge variant="outline" className="border-amber-200 text-amber-700">
              Trust: {profile.trustLevel}{" "}
              {profile.trustScore != null ? `(${profile.trustScore})` : ""}
            </Badge>
          )}
          {profile.roles?.map((role) => (
            <Badge
              key={role}
              variant="secondary"
              className="bg-slate-100 text-slate-700 dark:bg-slate-800 dark:text-slate-200"
            >
              {role}
            </Badge>
          ))}
        </div>
      </div>

      {/* Một cột chính: chỉnh sửa + lịch sử điểm + lối tắt */}
      <form
        onSubmit={handleSubmit}
        className="space-y-6 rounded-2xl border border-slate-200 bg-white p-4 shadow-sm dark:border-slate-700 dark:bg-slate-900"
      >
          <div className="flex items-center justify-between gap-2">
            <h2 className="text-sm font-semibold text-foreground">
              Thông tin cá nhân
            </h2>
            <Button
              type="submit"
              size="sm"
              className="inline-flex items-center gap-2"
              disabled={saving}
            >
              {saving && <Loader2 className="h-4 w-4 animate-spin" />}
              <Edit className="h-4 w-4" />
              Lưu thay đổi
            </Button>
          </div>

          {saveError && (
            <div className="rounded-md border border-red-200 bg-red-50 p-2 text-xs text-red-600 dark:border-red-900 dark:bg-red-950 dark:text-red-300">
              {saveError}
            </div>
          )}
          {saveSuccess && (
            <div className="rounded-md border border-emerald-200 bg-emerald-50 p-2 text-xs text-emerald-700 dark:border-emerald-900 dark:bg-emerald-950 dark:text-emerald-300">
              Đã lưu hồ sơ thành công.
            </div>
          )}

          <div className="grid grid-cols-1 gap-3 md:grid-cols-2">
            <div>
              <label className="text-xs font-medium text-slate-600 dark:text-slate-300">
                Họ tên
              </label>
              <Input
                value={form.fullName ?? ""}
                onChange={(event) =>
                  handleChange("fullName", event.target.value)
                }
                placeholder="Nhập họ tên"
                className="mt-1 h-9"
              />
            </div>

            <div>
              <label className="text-xs font-medium text-slate-600 dark:text-slate-300">
                Giới tính
              </label>
              <select
                className="mt-1 h-9 w-full rounded-md border border-input bg-background px-3 text-sm shadow-sm focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring"
                value={form.gender ?? ""}
                onChange={(event: React.ChangeEvent<HTMLSelectElement>) => {
                  const raw = event.target.value;
                  handleChange(
                    "gender",
                    raw === "" ? null : Number.parseInt(raw, 10)
                  );
                }}
              >
                <option value="">Không xác định</option>
                {/* Backend: Gender { Other = 0, Male = 1, Female = 2 } */}
                <option value="1">Nam</option>
                <option value="2">Nữ</option>
                <option value="0">Khác</option>
              </select>
            </div>
          </div>

          <div>
            <label className="text-xs font-medium text-slate-600 dark:text-slate-300">
              Giới thiệu
            </label>
            <textarea
              value={form.introduction ?? ""}
              onChange={(event: React.ChangeEvent<HTMLTextAreaElement>) =>
                handleChange("introduction", event.target.value)
              }
              placeholder="Giới thiệu ngắn gọn về bản thân bạn..."
              className="mt-1 min-h-[80px] w-full rounded-md border border-input bg-background px-3 py-2 text-sm shadow-sm focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-50"
            />
          </div>

          <div>
            <label className="text-xs font-medium text-slate-600 dark:text-slate-300">
              Ngành học
            </label>
            <select
              className="mt-1 h-9 w-full rounded-md border border-input bg-background px-3 text-sm shadow-sm focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-50"
              value={form.majorId ?? ""}
              onChange={(event: React.ChangeEvent<HTMLSelectElement>) =>
                handleChange(
                  "majorId",
                  event.target.value === "" ? null : event.target.value
                )
              }
              disabled={loadingMajors}
            >
              <option value="">
                {loadingMajors ? "Đang tải danh sách ngành..." : "Chọn ngành học"}
              </option>
              {majors.map((major) => (
                <option key={major.id} value={major.id}>
                  {major.majorName}
                </option>
              ))}
            </select>
          </div>
          {/* Lịch sử điểm tin cậy */}
          <div className="space-y-2 rounded-2xl border border-slate-200 bg-slate-50 p-4 text-xs text-slate-600 dark:border-slate-700 dark:bg-slate-900 dark:text-slate-300">
            <h2 className="text-sm font-semibold text-foreground">
              Lịch sử điểm tin cậy
            </h2>
            {loadingTrust ? (
              <div className="flex items-center gap-2 text-xs">
                <Loader2 className="h-3 w-3 animate-spin text-sky-500" />
                <span>Đang tải lịch sử...</span>
              </div>
            ) : trustHistory.length === 0 ? (
              <p className="text-xs text-slate-500 dark:text-slate-400">
                Chưa có lịch sử điểm tin cậy.
              </p>
            ) : (
              <ul className="space-y-1 max-h-40 overflow-y-auto pr-1">
                {trustHistory
                  .slice()
                  .sort(
                    (a, b) =>
                      new Date(b.createdAt || 0).getTime() -
                      new Date(a.createdAt || 0).getTime()
                  )
                  .map((item) => (
                    <li key={item.id} className="flex items-start justify-between gap-2">
                      <div>
                        <span className="font-semibold text-foreground">
                          {item.score}
                        </span>
                        {item.reason && (
                          <span className="ml-1 text-slate-500 dark:text-slate-400">
                            – {item.reason}
                          </span>
                        )}
                      </div>
                      {item.createdAt && (
                        <span className="shrink-0 text-[10px] text-slate-400 dark:text-slate-500">
                          {new Date(item.createdAt).toLocaleString()}
                        </span>
                      )}
                    </li>
                  ))}
              </ul>
            )}
          </div>
      </form>
    </div>
  );
};

export default ProfilePage;