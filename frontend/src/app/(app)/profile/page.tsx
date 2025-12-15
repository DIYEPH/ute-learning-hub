"use client";

import React, { useState, useEffect, useRef } from "react";
import { Loader2, Edit, Image as ImageIcon, Lock, User, Mail } from "lucide-react";
import { useUserProfile } from "@/src/hooks/use-user-profile";
import { useMajors } from "@/src/hooks/use-majors";
import { useFileUpload } from "@/src/hooks/use-file-upload";
import { Button } from "@/src/components/ui/button";
import { Badge } from "@/src/components/ui/badge";
import { Avatar, AvatarFallback, AvatarImage } from "@/src/components/ui/avatar";
import { Input } from "@/src/components/ui/input";
import { useNotification } from "@/src/components/providers/notification-provider";
import { getFileUrlById } from "@/src/lib/file-url";
import { putApiAccountProfile, getApiUserByIdTrustHistory, postApiAuthForgotPassword, postApiAuthResetPassword, postApiAuthChangeUsername } from "@/src/api/database/sdk.gen";
import type { UpdateProfileCommand, MajorDto2, UserTrustHistoryDto } from "@/src/api/database/types.gen";

const ProfilePage = () => {
  const { profile, loading, error } = useUserProfile();
  const { fetchMajors, loading: loadingMajors } = useMajors();
  const { uploadFile } = useFileUpload();
  const { success: notifySuccess, error: notifyError } = useNotification();
  const fileInputRef = useRef<HTMLInputElement | null>(null);

  const [majors, setMajors] = useState<MajorDto2[]>([]);
  const [trustHistory, setTrustHistory] = useState<UserTrustHistoryDto[]>([]);
  const [loadingTrust, setLoadingTrust] = useState(false);
  const [form, setForm] = useState<UpdateProfileCommand>({ fullName: "", introduction: "", majorId: null, gender: null, avatarUrl: null });
  const [saving, setSaving] = useState(false);
  const [saveError, setSaveError] = useState<string | null>(null);
  const [saveSuccess, setSaveSuccess] = useState(false);
  const [avatarUploading, setAvatarUploading] = useState(false);
  const [pendingAvatarFile, setPendingAvatarFile] = useState<File | null>(null);
  const [avatarPreview, setAvatarPreview] = useState<string | null>(null);
  const [resetToken, setResetToken] = useState("");
  const [newPassword, setNewPassword] = useState("");
  const [otpSent, setOtpSent] = useState(false);
  const [sendingOtp, setSendingOtp] = useState(false);
  const [resettingPassword, setResettingPassword] = useState(false);
  const [newUsername, setNewUsername] = useState("");
  const [changingUsername, setChangingUsername] = useState(false);

  useEffect(() => {
    if (profile) setForm({ fullName: profile.fullName ?? "", introduction: profile.introduction ?? "", majorId: profile.major?.id ?? null, gender: (profile.gender as any) ?? null, avatarUrl: profile.avatarUrl ?? null });
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

  const handleChange = (field: keyof UpdateProfileCommand, value: any) => { setForm(prev => ({ ...prev, [field]: value })); setSaveSuccess(false); setSaveError(null); };

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
      await putApiAccountProfile<true>({ body: { fullName: form.fullName, introduction: form.introduction, majorId: form.majorId, gender: form.gender, avatarUrl: avatarUrl ?? null }, throwOnError: true });
      handleChange("avatarUrl", avatarUrl); setSaveSuccess(true); notifySuccess("Đã lưu hồ sơ thành công.");
    } catch (err: any) {
      const msg = err?.response?.data?.message || err?.message || "Không thể cập nhật hồ sơ"; setSaveError(msg); notifyError(msg); setAvatarUploading(false);
    } finally { setSaving(false); }
  };

  const handleSendOtp = async () => {
    if (!profile?.email) { notifyError("Không tìm thấy email."); return; }
    setSendingOtp(true);
    try { await postApiAuthForgotPassword<true>({ body: { email: profile.email }, throwOnError: true }); notifySuccess("Đã gửi mã xác thực về email."); setOtpSent(true); }
    catch (err: any) { notifyError(err?.response?.data?.message || err?.message || "Không thể gửi mã xác thực"); }
    finally { setSendingOtp(false); }
  };

  const handleResetPassword = async () => {
    if (!resetToken || !newPassword) { notifyError("Vui lòng nhập mã xác thực và mật khẩu mới."); return; }
    setResettingPassword(true);
    try { await postApiAuthResetPassword<true>({ body: { email: profile?.email || "", token: resetToken, newPassword }, throwOnError: true }); notifySuccess("Đã đặt mật khẩu mới thành công."); setResetToken(""); setNewPassword(""); setOtpSent(false); }
    catch (err: any) { notifyError(err?.response?.data?.message || err?.message || "Không thể đặt mật khẩu mới"); }
    finally { setResettingPassword(false); }
  };

  const handleChangeUsername = async () => {
    if (!newUsername) { notifyError("Vui lòng nhập tên đăng nhập mới."); return; }
    setChangingUsername(true);
    try { await postApiAuthChangeUsername<true>({ body: { newUsername }, throwOnError: true }); notifySuccess("Đã đổi tên đăng nhập thành công."); setNewUsername(""); }
    catch (err: any) { notifyError(err?.response?.data?.message || err?.message || "Không thể đổi tên đăng nhập"); }
    finally { setChangingUsername(false); }
  };

  if (loading && !profile) return <div className="flex h-[50vh] items-center justify-center"><Loader2 className="h-6 w-6 animate-spin text-sky-500" /></div>;
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
            <button type="button" onClick={() => fileInputRef.current?.click()} className="absolute bottom-0 right-0 flex h-7 w-7 items-center justify-center rounded-full border border-slate-200 bg-white text-slate-600 shadow-sm hover:bg-slate-50 dark:border-slate-700 dark:bg-slate-900 dark:text-slate-200" disabled={avatarUploading}>
              {avatarUploading ? <Loader2 className="h-3 w-3 animate-spin" /> : <ImageIcon className="h-3 w-3" />}
            </button>
            <input ref={fileInputRef} type="file" accept="image/*" className="hidden" onChange={handleAvatarFileChange} />
          </div>
          <div>
            <h1 className="text-2xl font-semibold text-foreground">{profile.fullName || profile.username || profile.email}</h1>
            <p className="text-sm text-slate-500 dark:text-slate-400">{profile.email}{profile.username ? ` • @${profile.username}` : ""}</p>
            <p className="mt-1 text-xs text-slate-500 dark:text-slate-400">Giới tính: {genderText}</p>
          </div>
        </div>
        <div className="flex flex-wrap gap-2">
          {profile.trustLevel && <Badge variant="outline" className="rounded-none border-slate-300 text-slate-600 dark:border-slate-600 dark:text-slate-300">{profile.trustLevel}{profile.trustScore != null ? ` (${profile.trustScore})` : ""}</Badge>}
          {profile.roles?.map(role => <Badge key={role} variant="outline" className="rounded-none border-slate-300 text-slate-600 dark:border-slate-600 dark:text-slate-300">{role}</Badge>)}
        </div>
      </div>

      {/* Profile Form */}
      <form onSubmit={handleSubmit} className="space-y-6 border border-slate-200 bg-white p-4 shadow-sm dark:border-slate-700 dark:bg-slate-900">
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
            <label className="text-xs font-medium text-slate-600 dark:text-slate-300">Họ tên</label>
            <Input value={form.fullName ?? ""} onChange={e => handleChange("fullName", e.target.value)} placeholder="Nhập họ tên" className="mt-1 h-9" />
          </div>
          <div>
            <label className="text-xs font-medium text-slate-600 dark:text-slate-300">Giới tính</label>
            <select className="mt-1 h-9 w-full border border-input bg-background px-3 text-sm shadow-sm focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring" value={form.gender ?? ""} onChange={e => handleChange("gender", e.target.value === "" ? null : parseInt(e.target.value, 10))}>
              <option value="">Không xác định</option><option value="1">Nam</option><option value="2">Nữ</option><option value="0">Khác</option>
            </select>
          </div>
        </div>
        <div>
          <label className="text-xs font-medium text-slate-600 dark:text-slate-300">Giới thiệu</label>
          <textarea value={form.introduction ?? ""} onChange={e => handleChange("introduction", e.target.value)} placeholder="Giới thiệu ngắn gọn..." className="mt-1 min-h-[80px] w-full border border-input bg-background px-3 py-2 text-sm shadow-sm focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring" />
        </div>
        <div>
          <label className="text-xs font-medium text-slate-600 dark:text-slate-300">Ngành học</label>
          <select className="mt-1 h-9 w-full border border-input bg-background px-3 text-sm shadow-sm focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-50" value={form.majorId ?? ""} onChange={e => handleChange("majorId", e.target.value === "" ? null : e.target.value)} disabled={loadingMajors}>
            <option value="">{loadingMajors ? "Đang tải..." : "Chọn ngành học"}</option>
            {majors.map(m => <option key={m.id} value={m.id}>{m.majorName}</option>)}
          </select>
        </div>
      </form>

      {/* Account Settings */}
      <div className="space-y-6 border border-slate-200 bg-white p-4 shadow-sm dark:border-slate-700 dark:bg-slate-900">
        <h2 className="text-sm font-semibold text-foreground">Cài đặt tài khoản</h2>
        {/* Username */}
        <div className="space-y-2">
          <label className="text-xs font-medium text-slate-600 dark:text-slate-300">Đổi tên đăng nhập</label>
          <p className="text-xs text-slate-500">Hiện tại: <span className="font-medium">{profile.username}</span></p>
          <div className="flex gap-2">
            <Input type="text" value={newUsername} onChange={e => setNewUsername(e.target.value)} placeholder="Tên đăng nhập mới" className="h-9 flex-1" />
            <Button type="button" size="sm" variant="outline" onClick={handleChangeUsername} disabled={changingUsername || !newUsername} className="h-9">
              {changingUsername && <Loader2 className="mr-1 h-4 w-4 animate-spin" />}<User className="mr-1 h-4 w-4" />Đổi
            </Button>
          </div>
        </div>
        {/* Password */}
        <div className="space-y-2 pt-4 border-t">
          <label className="text-xs font-medium text-slate-600 dark:text-slate-300">Đặt mật khẩu mới</label>
          <p className="text-xs text-slate-500">Mã xác thực gửi về: <span className="font-medium">{profile.email}</span></p>
          {!otpSent ? (
            <Button type="button" size="sm" variant="outline" onClick={handleSendOtp} disabled={sendingOtp} className="h-9">
              {sendingOtp && <Loader2 className="mr-1 h-4 w-4 animate-spin" />}<Mail className="mr-1 h-4 w-4" />Gửi mã xác thực
            </Button>
          ) : (
            <div className="space-y-2">
              <div className="grid grid-cols-1 gap-2 md:grid-cols-2">
                <Input type="text" value={resetToken} onChange={e => setResetToken(e.target.value)} placeholder="Mã xác thực" className="h-9" autoComplete="off" />
                <Input type="password" value={newPassword} onChange={e => setNewPassword(e.target.value)} placeholder="Mật khẩu mới" className="h-9" autoComplete="new-password" />
              </div>
              <div className="flex gap-2">
                <Button type="button" size="sm" variant="outline" onClick={handleResetPassword} disabled={resettingPassword || !resetToken || !newPassword} className="h-9">
                  {resettingPassword && <Loader2 className="mr-1 h-4 w-4 animate-spin" />}<Lock className="mr-1 h-4 w-4" />Đặt mật khẩu
                </Button>
                <Button type="button" size="sm" variant="ghost" onClick={() => { setOtpSent(false); setResetToken(""); setNewPassword(""); }} className="h-9 text-slate-500">Hủy</Button>
              </div>
            </div>
          )}
        </div>
      </div>

      {/* Trust History */}
      <div className="space-y-2 border border-slate-200 bg-slate-50 p-4 text-xs text-slate-600 dark:border-slate-700 dark:bg-slate-900 dark:text-slate-300">
        <h2 className="text-sm font-semibold text-foreground">Lịch sử điểm tin cậy</h2>
        {loadingTrust ? (
          <div className="flex items-center gap-2 text-xs"><Loader2 className="h-3 w-3 animate-spin text-sky-500" /><span>Đang tải...</span></div>
        ) : trustHistory.length === 0 ? (
          <p className="text-xs text-slate-500">Chưa có lịch sử điểm tin cậy.</p>
        ) : (
          <ul className="space-y-1 max-h-40 overflow-y-auto pr-1">
            {trustHistory.slice().sort((a, b) => new Date(b.createdAt || 0).getTime() - new Date(a.createdAt || 0).getTime()).map(item => (
              <li key={item.id} className="flex items-start justify-between gap-2">
                <div><span className="font-semibold text-foreground">{(item.score ?? 0) > 0 ? '+' : ''}{item.score}</span>{item.reason && <span className="ml-1 text-slate-500">– {item.reason}</span>}</div>
                {item.createdAt && <span className="shrink-0 text-[10px] text-slate-400">{new Date(item.createdAt).toLocaleString()}</span>}
              </li>
            ))}
          </ul>
        )}
      </div>
    </div>
  );
};

export default ProfilePage;
