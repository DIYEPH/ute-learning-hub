"use client";

import React, { useState, useEffect, useRef } from "react";
import { Loader2, Edit, Image as ImageIcon, User } from "lucide-react";
import { useUserProfile } from "@/src/hooks/use-user-profile";
import { useMajors } from "@/src/hooks/use-majors";
import { useFileUpload } from "@/src/hooks/use-file-upload";
import { Button } from "@/src/components/ui/button";
import { Badge } from "@/src/components/ui/badge";
import { Avatar, AvatarFallback, AvatarImage } from "@/src/components/ui/avatar";
import { Input } from "@/src/components/ui/input";
import { useNotification } from "@/src/components/providers/notification-provider";
import { getFileUrlById } from "@/src/lib/file-url";
import { putApiAccountProfile, getApiUserByIdTrustHistory, postApiAuthChangeUsername } from "@/src/api/database/sdk.gen";
import type { UpdateProfileCommandRequest, MajorDetailDto, UserTrustHistoryDto } from "@/src/api/database/types.gen";

const ProfilePage = () => {
  const { profile, loading, error } = useUserProfile();
  const { fetchMajors, loading: loadingMajors } = useMajors();
  const { uploadFile } = useFileUpload();
  const { success: notifySuccess, error: notifyError } = useNotification();
  const fileInputRef = useRef<HTMLInputElement | null>(null);

  const [majors, setMajors] = useState<MajorDetailDto[]>([]);
  const [trustHistory, setTrustHistory] = useState<UserTrustHistoryDto[]>([]);
  const [loadingTrust, setLoadingTrust] = useState(false);
  const [form, setForm] = useState<UpdateProfileCommandRequest>({ introduction: "", majorId: null, gender: null, avatarUrl: null });
  const [saving, setSaving] = useState(false);
  const [saveError, setSaveError] = useState<string | null>(null);
  const [saveSuccess, setSaveSuccess] = useState(false);
  const [avatarUploading, setAvatarUploading] = useState(false);
  const [pendingAvatarFile, setPendingAvatarFile] = useState<File | null>(null);
  const [avatarPreview, setAvatarPreview] = useState<string | null>(null);
  const [newUsername, setNewUsername] = useState("");
  const [changingUsername, setChangingUsername] = useState(false);

  useEffect(() => {
    if (profile) setForm({ introduction: profile.introduction ?? "", majorId: profile.majorId ?? null, gender: (profile.gender as any) ?? null, avatarUrl: profile.avatarUrl ?? null });
  }, [profile]);

  useEffect(() => {
    fetchMajors({ Page: 1, PageSize: 1000 }).then((res: any) => res?.items && setMajors(res.items)).catch(console.error);
  }, [fetchMajors]);

  useEffect(() => {
    if (!profile?.id) return;
    setLoadingTrust(true);
    getApiUserByIdTrustHistory({ path: { id: profile.id } })
      .then((res: any) => { const data = res?.data || res; Array.isArray(data) && setTrustHistory(data); })
      .catch(console.error)
      .finally(() => setLoadingTrust(false));
  }, [profile?.id]);

  const handleChange = (field: keyof UpdateProfileCommandRequest, value: any) => { setForm(prev => ({ ...prev, [field]: value })); setSaveSuccess(false); setSaveError(null); };

  const handleAvatarFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;
    if (!file.type.startsWith("image/")) { setSaveError("Vui lòng chọn tệp hình ảnh hợp lệ."); e.target.value = ""; return; }
    setPendingAvatarFile(file); setAvatarPreview(URL.createObjectURL(file)); e.target.value = "";
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!profile) return;
    setSaving(true); setSaveError(null); setSaveSuccess(false);
    try {
      let avatarUrl = form.avatarUrl;
      if (pendingAvatarFile) {
        setAvatarUploading(true);
        const uploaded = await uploadFile(pendingAvatarFile, "AvatarUser");
        if (!uploaded.id) throw new Error("Phản hồi upload avatar không hợp lệ.");
        avatarUrl = getFileUrlById(uploaded.id);
        setPendingAvatarFile(null); setAvatarPreview(null); setAvatarUploading(false);
      }
      await putApiAccountProfile<true>({ body: { introduction: form.introduction, majorId: form.majorId, gender: form.gender, avatarUrl: avatarUrl ?? null }, throwOnError: true });
      handleChange("avatarUrl", avatarUrl); setSaveSuccess(true); notifySuccess("Đã lưu hồ sơ thành công.");
    } catch (err: any) {
      const msg = err?.response?.data?.message || err?.message || "Không thể cập nhật hồ sơ"; setSaveError(msg); notifyError(msg); setAvatarUploading(false);
    } finally { setSaving(false); }
  };

  const handleChangeUsername = async () => {
    if (!newUsername) { notifyError("Vui lòng nhập tên đăng nhập mới."); return; }
    setChangingUsername(true);
    try { await postApiAuthChangeUsername<true>({ body: { newUsername }, throwOnError: true }); notifySuccess("Đã đổi tên đăng nhập thành công."); setNewUsername(""); }
    catch (err: any) { notifyError(err?.response?.data?.message || err?.message || "Không thể đổi tên đăng nhập"); }
    finally { setChangingUsername(false); }
  };

  if (loading && !profile) return <div className="flex h-[50vh] items-center justify-center"><Loader2 className="h-6 w-6 animate-spin text-primary" /></div>;
  if (!profile || error) return <div className="space-y-4"><h1 className="text-2xl font-semibold">Hồ sơ</h1><div className="border border-red-200 bg-red-50 p-4 text-sm text-red-600 dark:border-red-900 dark:bg-red-950 dark:text-red-300">{!profile ? "Không thể tải thông tin hồ sơ. Vui lòng đăng nhập lại." : error}</div></div>;

  const initials = (profile.fullName || profile.username || profile.email || "?").split(" ").map(p => p[0]).join("").slice(0, 2).toUpperCase();
  const genderText = form.gender === 1 ? "Nam" : form.gender === 2 ? "Nữ" : form.gender === 0 ? "Khác" : "Không xác định";

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col justify-between gap-4 md:flex-row md:items-center">
        <div className="flex items-center gap-4">
          <div className="relative">
            <Avatar className="h-16 w-16">
              <AvatarImage src={avatarPreview || form.avatarUrl || profile.avatarUrl || undefined} alt={profile.fullName || ""} />
              <AvatarFallback>{initials}</AvatarFallback>
            </Avatar>
            <button type="button" onClick={() => fileInputRef.current?.click()} className="absolute bottom-0 right-0 flex h-7 w-7 items-center justify-center rounded-full border border-border bg-card text-muted-foreground shadow-sm hover:bg-muted" disabled={avatarUploading}>
              {avatarUploading ? <Loader2 className="h-3 w-3 animate-spin" /> : <ImageIcon className="h-3 w-3" />}
            </button>
            <input ref={fileInputRef} type="file" accept="image/*" className="hidden" onChange={handleAvatarFileChange} />
          </div>
          <div>
            <h1 className="text-2xl font-semibold text-foreground">{profile.fullName || profile.username || profile.email}</h1>
            <p className="text-sm text-muted-foreground">{profile.email}{profile.username ? ` • @${profile.username}` : ""}</p>
            <p className="mt-1 text-xs text-muted-foreground">Giới tính: {genderText}</p>
          </div>
        </div>
        <div className="flex flex-wrap gap-2">
          {profile.trustLevel && <Badge variant="outline" className="rounded-none border-border text-muted-foreground">{profile.trustLevel}{profile.trustScore != null ? ` (${profile.trustScore})` : ""}</Badge>}
          {profile.roles?.map(role => <Badge key={role} variant="outline" className="rounded-none border-border text-muted-foreground">{role}</Badge>)}
        </div>
      </div>

      {/* Profile Form */}
      <form onSubmit={handleSubmit} className="space-y-6 border border-border bg-card p-4 shadow-sm">
        <div className="flex items-center justify-between gap-2">
          <h2 className="text-sm font-semibold text-foreground">Thông tin cá nhân</h2>
          <Button type="submit" size="sm" className="inline-flex items-center gap-2" disabled={saving}>
            {saving && <Loader2 className="h-4 w-4 animate-spin" />}<Edit className="h-4 w-4" />Lưu thay đổi
          </Button>
        </div>
        {saveError && <div className="border border-red-200 bg-red-50 p-2 text-xs text-red-600 dark:border-red-900 dark:bg-red-950 dark:text-red-300">{saveError}</div>}
        {saveSuccess && <div className="border border-emerald-200 bg-emerald-50 p-2 text-xs text-emerald-700 dark:border-emerald-900 dark:bg-emerald-950 dark:text-emerald-300">Đã lưu hồ sơ thành công.</div>}
        <div className="grid grid-cols-1 gap-3 md:grid-cols-2">
          <div>
            <label className="text-xs font-medium text-muted-foreground">Giới tính</label>
            <select className="mt-1 h-9 w-full border border-input bg-background px-3 text-sm shadow-sm focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring" value={form.gender ?? ""} onChange={e => handleChange("gender", e.target.value === "" ? null : parseInt(e.target.value, 10))}>
              <option value="">Không xác định</option><option value="1">Nam</option><option value="2">Nữ</option><option value="0">Khác</option>
            </select>
          </div>
          <div>
            <label className="text-xs font-medium text-muted-foreground">Ngành học</label>
            <select className="mt-1 h-9 w-full border border-input bg-background px-3 text-sm shadow-sm focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-50" value={form.majorId ?? ""} onChange={e => handleChange("majorId", e.target.value === "" ? null : e.target.value)} disabled={loadingMajors}>
              <option value="">{loadingMajors ? "Đang tải..." : "Chọn ngành học"}</option>
              {majors.map(m => <option key={m.id} value={m.id}>{m.majorName}</option>)}
            </select>
          </div>
        </div>
        <div>
          <label className="text-xs font-medium text-muted-foreground">Giới thiệu</label>
          <textarea value={form.introduction ?? ""} onChange={e => handleChange("introduction", e.target.value)} placeholder="Giới thiệu ngắn gọn..." className="mt-1 min-h-[80px] w-full border border-input bg-background px-3 py-2 text-sm shadow-sm focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring" />
        </div>
      </form>

      {/* Account Settings */}
      <div className="space-y-6 border border-border bg-card p-4 shadow-sm">
        <h2 className="text-sm font-semibold text-foreground">Cài đặt tài khoản</h2>
        {/* Username */}
        <div className="space-y-2">
          <label className="text-xs font-medium text-muted-foreground">Đổi tên đăng nhập</label>
          <p className="text-xs text-muted-foreground">Hiện tại: <span className="font-medium">{profile.username}</span></p>
          <div className="flex gap-2">
            <Input type="text" value={newUsername} onChange={e => setNewUsername(e.target.value)} placeholder="Tên đăng nhập mới" className="h-9 flex-1" />
            <Button type="button" size="sm" variant="outline" onClick={handleChangeUsername} disabled={changingUsername || !newUsername} className="h-9">
              {changingUsername && <Loader2 className="mr-1 h-4 w-4 animate-spin" />}<User className="mr-1 h-4 w-4" />Đổi
            </Button>
          </div>
        </div>
      </div>

      {/* Trust History */}
      <div className="space-y-2 border border-border bg-muted p-4 text-xs text-muted-foreground">
        <h2 className="text-sm font-semibold text-foreground">Lịch sử điểm tin cậy</h2>
        {loadingTrust ? (
          <div className="flex items-center gap-2 text-xs"><Loader2 className="h-3 w-3 animate-spin text-primary" /><span>Đang tải...</span></div>
        ) : trustHistory.length === 0 ? (
          <p className="text-xs text-muted-foreground">Chưa có lịch sử điểm tin cậy.</p>
        ) : (
          <ul className="space-y-1 max-h-40 overflow-y-auto pr-1">
            {trustHistory.slice().sort((a, b) => new Date(b.createdAt || 0).getTime() - new Date(a.createdAt || 0).getTime()).map(item => (
              <li key={item.id} className="flex items-start justify-between gap-2">
                <div><span className="font-semibold text-foreground">{(item.score ?? 0) > 0 ? '+' : ''}{item.score}</span>{item.reason && <span className="ml-1 text-muted-foreground">– {item.reason}</span>}</div>
                {item.createdAt && <span className="shrink-0 text-[10px] text-muted-foreground">{new Date(item.createdAt).toLocaleString()}</span>}
              </li>
            ))}
          </ul>
        )}
      </div>
    </div>
  );
};

export default ProfilePage;
