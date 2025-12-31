"use client";

import React, { useState, useEffect, useRef } from "react";
import Link from "next/link";
import { Loader2, Image as ImageIcon, Upload, FileText, ThumbsUp, MessageSquare, Heart, Settings, ChevronRight, GraduationCap, Building2 } from "lucide-react";
import { useUserProfile } from "@/src/hooks/use-user-profile";
import { useMajors } from "@/src/hooks/use-majors";
import { useFileUpload } from "@/src/hooks/use-file-upload";
import { Button } from "@/src/components/ui/button";
import { Avatar, AvatarFallback, AvatarImage } from "@/src/components/ui/avatar";
import { Input } from "@/src/components/ui/input";
import { useNotification } from "@/src/components/providers/notification-provider";
import { getFileUrlById } from "@/src/lib/file-url";
import { putApiAccountProfile, getApiUserByIdTrustHistory, postApiAuthChangeUsername, getApiAccountStats } from "@/src/api";
import type { UpdateProfileCommandRequest, MajorDetailDto, UserTrustHistoryDto, UserStatsDto } from "@/src/api/database/types.gen";

const ProfilePage = () => {
  const { profile, loading, error } = useUserProfile();
  const { fetchMajors, loading: loadingMajors } = useMajors();
  const { uploadFile } = useFileUpload();
  const { success: notifySuccess, error: notifyError } = useNotification();
  const fileInputRef = useRef<HTMLInputElement | null>(null);

  const [majors, setMajors] = useState<MajorDetailDto[]>([]);
  const [trustHistory, setTrustHistory] = useState<UserTrustHistoryDto[]>([]);
  const [loadingTrust, setLoadingTrust] = useState(false);
  const [form, setForm] = useState<UpdateProfileCommandRequest>({ introduction: "", majorId: null, gender: null, avatarUrl: null, isSuggest: false });
  const [saving, setSaving] = useState(false);
  const [avatarUploading, setAvatarUploading] = useState(false);
  const [pendingAvatarFile, setPendingAvatarFile] = useState<File | null>(null);
  const [avatarPreview, setAvatarPreview] = useState<string | null>(null);
  const [newUsername, setNewUsername] = useState("");
  const [changingUsername, setChangingUsername] = useState(false);
  const [showSettings, setShowSettings] = useState(false);

  // Stats
  const [stats, setStats] = useState({ uploads: 0, upvotes: 0, comments: 0 });
  const [loadingStats, setLoadingStats] = useState(true);

  useEffect(() => {
    if (profile) setForm({ introduction: profile.introduction ?? "", majorId: profile.majorId ?? null, gender: (profile.gender as any) ?? null, avatarUrl: profile.avatarUrl ?? null, isSuggest: profile.isSuggest ?? false });
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

  // Fetch user stats
  useEffect(() => {
    const fetchStats = async () => {
      setLoadingStats(true);
      try {
        const res = await getApiAccountStats();
        const data = (res as any)?.data || res;
        if (data) {
          setStats({
            uploads: data.uploads ?? 0,
            upvotes: data.upvotes ?? 0,
            comments: data.comments ?? 0
          });
        }
      } catch (err) {
        console.error("Error fetching stats:", err);
      } finally {
        setLoadingStats(false);
      }
    };
    if (profile?.id) fetchStats();
  }, [profile?.id]);

  const handleChange = (field: keyof UpdateProfileCommandRequest, value: any) => {
    setForm(prev => ({ ...prev, [field]: value }));
  };

  const handleAvatarFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;
    if (!file.type.startsWith("image/")) { notifyError("Vui lòng chọn tệp hình ảnh hợp lệ."); e.target.value = ""; return; }
    setPendingAvatarFile(file); setAvatarPreview(URL.createObjectURL(file)); e.target.value = "";
  };

  const handleSubmit = async () => {
    if (!profile) return;
    setSaving(true);
    try {
      let avatarUrl = form.avatarUrl;
      if (pendingAvatarFile) {
        setAvatarUploading(true);
        const uploaded = await uploadFile(pendingAvatarFile, "AvatarUser");
        if (!uploaded.id) throw new Error("Phản hồi upload avatar không hợp lệ.");
        avatarUrl = getFileUrlById(uploaded.id);
        setPendingAvatarFile(null); setAvatarPreview(null); setAvatarUploading(false);
      }
      await putApiAccountProfile<true>({ body: { introduction: form.introduction, majorId: form.majorId, gender: form.gender, avatarUrl: avatarUrl ?? null, isSuggest: form.isSuggest }, throwOnError: true });
      handleChange("avatarUrl", avatarUrl);
      notifySuccess("Đã lưu hồ sơ thành công.");
    } catch (err: any) {
      const msg = err?.response?.data?.message || err?.message || "Không thể cập nhật hồ sơ";
      notifyError(msg);
      setAvatarUploading(false);
    } finally { setSaving(false); }
  };

  const handleChangeUsername = async () => {
    if (!newUsername) { notifyError("Vui lòng nhập tên đăng nhập mới."); return; }
    setChangingUsername(true);
    try {
      await postApiAuthChangeUsername<true>({ body: { newUsername }, throwOnError: true });
      notifySuccess("Đã đổi tên đăng nhập thành công.");
      setNewUsername("");
    }
    catch (err: any) { notifyError(err?.response?.data?.message || err?.message || "Không thể đổi tên đăng nhập"); }
    finally { setChangingUsername(false); }
  };

  if (loading && !profile) return (
    <div className="flex h-[50vh] items-center justify-center">
      <Loader2 className="h-8 w-8 animate-spin text-primary" />
    </div>
  );

  if (!profile || error) return (
    <div className="max-w-4xl mx-auto p-6">
      <div className="border border-red-200 bg-red-50 p-4 text-sm text-red-600 dark:border-red-900 dark:bg-red-950 dark:text-red-300 rounded-lg">
        {!profile ? "Không thể tải thông tin hồ sơ. Vui lòng đăng nhập lại." : error}
      </div>
    </div>
  );

  const initials = (profile.fullName || profile.username || profile.email || "?").split(" ").map(p => p[0]).join("").slice(0, 2).toUpperCase();
  const majorInfo = majors.find(m => m.id === profile.majorId);

  // Trust Level mapping based on backend TrustLever enum and TrustLevelPolicy
  // None = 0 (< 5), Newbie = 1 (5-8), Contributor = 2 (9-28), TrustedMember = 3 (29-58), Moderator = 4 (>= 59), Master = 5
  const getTrustLevelInfo = (level: number | string | undefined, score: number) => {
    // Map numeric enum values to string names
    const numericToName: Record<number, string> = {
      0: 'None',
      1: 'Newbie',
      2: 'Contributor',
      3: 'TrustedMember',
      4: 'Moderator',
      5: 'Master',
    };

    const levelMap: Record<string, { name: string; nextThreshold: number | null }> = {
      'None': { name: 'Người mới', nextThreshold: 5 },
      'Newbie': { name: 'Thành viên mới', nextThreshold: 9 },
      'Contributor': { name: 'Cộng tác viên', nextThreshold: 29 },
      'TrustedMember': { name: 'Thành viên tin cậy', nextThreshold: 59 },
      'Moderator': { name: 'Kiểm duyệt viên', nextThreshold: null },
      'Master': { name: 'Chuyên gia', nextThreshold: null },
    };

    // Convert numeric to string if needed
    const levelName = typeof level === 'number' ? numericToName[level] : level;
    const info = levelMap[levelName || 'None'] || levelMap['None'];
    const pointsToNext = info.nextThreshold ? Math.max(0, info.nextThreshold - score) : 0;
    return { name: info.name, pointsToNext, isMaxLevel: info.nextThreshold === null };
  };

  const trustInfo = getTrustLevelInfo(profile.trustLevel, profile.trustScore || 0);

  return (
    <div className="space-y-6">
      {/* Header Section */}
      <div className="bg-card border border-border rounded-xl p-6 shadow-sm">
        <div className="flex flex-col md:flex-row md:items-start gap-6">
          {/* Left: Avatar & Info */}
          <div className="flex items-start gap-4 flex-1">
            <div className="relative">
              <Avatar className="h-20 w-20 border-4 border-background shadow-lg">
                <AvatarImage src={avatarPreview || form.avatarUrl || profile.avatarUrl || undefined} alt={profile.fullName || ""} />
                <AvatarFallback className="text-xl font-semibold bg-primary/10 text-primary">{initials}</AvatarFallback>
              </Avatar>
              <button
                type="button"
                onClick={() => fileInputRef.current?.click()}
                className="absolute -bottom-1 -right-1 flex h-8 w-8 items-center justify-center rounded-full border-2 border-background bg-primary text-primary-foreground shadow-md hover:bg-primary/90 transition-colors"
                disabled={avatarUploading}
              >
                {avatarUploading ? <Loader2 className="h-4 w-4 animate-spin" /> : <ImageIcon className="h-4 w-4" />}
              </button>
              <input ref={fileInputRef} type="file" accept="image/*" className="hidden" onChange={handleAvatarFileChange} />
            </div>

            <div className="flex-1 min-w-0">
              <h1 className="text-2xl font-bold text-foreground truncate">
                {profile.fullName || profile.username || profile.email}
              </h1>
              {majorInfo && (
                <div className="flex items-center gap-1.5 mt-1 text-primary">
                  <Building2 className="h-4 w-4" />
                  <span className="text-sm font-medium">{majorInfo.facultyName}</span>
                </div>
              )}
              {majorInfo && (
                <div className="flex items-center gap-1.5 mt-0.5 text-muted-foreground">
                  <GraduationCap className="h-4 w-4" />
                  <span className="text-sm">{majorInfo.majorName}</span>
                </div>
              )}
            </div>
          </div>

          {/* Right: Points & Level */}
          <div className="bg-gradient-to-br from-primary/5 to-primary/10 border border-primary/20 rounded-xl p-4 min-w-[180px]">
            <div className="space-y-1">
              <div className="text-sm text-muted-foreground">
                Điểm tổng: <span className="text-xl font-bold text-primary">{profile.trustScore || 0}</span>
              </div>
              <div className="text-sm text-foreground font-medium">
                {trustInfo.name}
              </div>
              <div className="text-xs text-muted-foreground">
                {trustInfo.isMaxLevel ? 'Đã đạt cấp cao nhất' : `${trustInfo.pointsToNext} điểm để lên cấp`}
              </div>
              {/* <button className="text-xs text-primary hover:underline flex items-center gap-1 mt-1">
                Tìm hiểu thêm <ChevronRight className="h-3 w-3" />
              </button> */}
            </div>
          </div>
        </div>
      </div>

      {/* Statistics Section */}
      <div>
        <h2 className="text-lg font-semibold text-foreground mb-4">Thống kê</h2>
        <div className="">
          {/* My Uploads Card */}
          <div className="bg-card border border-border rounded-xl p-5 shadow-sm">
            <div className="flex items-center gap-2 mb-4">
              <FileText className="h-5 w-5 text-primary" />
              <span className="font-semibold text-foreground">Tài liệu của tôi</span>
            </div>
            <div className="grid grid-cols-3 gap-4 text-center">
              <div>
                <div className="text-2xl font-bold text-foreground">
                  {loadingStats ? <Loader2 className="h-5 w-5 animate-spin mx-auto" /> : stats.uploads}
                </div>
                <div className="text-xs text-muted-foreground mt-1">Đã tải lên</div>
              </div>
              <div>
                <div className="text-2xl font-bold text-foreground">{stats.upvotes}</div>
                <div className="text-xs text-muted-foreground mt-1">Lượt thích</div>
              </div>
              <div>
                <div className="text-2xl font-bold text-foreground">{stats.comments}</div>
                <div className="text-xs text-muted-foreground mt-1">Bình luận</div>
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* CTA Section */}
      <div className="bg-gradient-to-r from-primary/5 via-primary/10 to-primary/5 border border-primary/20 rounded-xl p-5 flex flex-col sm:flex-row items-center justify-between gap-4">
        <div className="flex items-center gap-3">
          <div className="h-10 w-10 rounded-full bg-primary/20 flex items-center justify-center">
            <FileText className="h-5 w-5 text-primary" />
          </div>
          <p className="text-sm text-foreground">
            Giúp đỡ bạn bè và kiếm thêm điểm bằng cách tải lên tài liệu của bạn
          </p>
        </div>
        <Link href="/documents/upload">
          <Button className="gap-2 rounded-full px-6">
            <Upload className="h-4 w-4" />
            Tải tài liệu
          </Button>
        </Link>
      </div>

      {/* Settings Toggle */}
      <button
        onClick={() => setShowSettings(!showSettings)}
        className="flex items-center gap-2 text-sm text-muted-foreground hover:text-foreground transition-colors"
      >
        <Settings className="h-4 w-4" />
        <span>Cài đặt tài khoản</span>
        <ChevronRight className={`h-4 w-4 transition-transform ${showSettings ? 'rotate-90' : ''}`} />
      </button>

      {/* Settings Section */}
      {showSettings && (
        <div className="space-y-4 bg-card border border-border rounded-xl p-5 shadow-sm animate-in slide-in-from-top-2">
          {/* Profile Edit */}
          <div className="space-y-4">
            <h3 className="font-semibold text-foreground">Thông tin cá nhân</h3>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <label className="text-xs font-medium text-muted-foreground">Giới tính</label>
                <select
                  className="mt-1 h-10 w-full rounded-lg border border-input bg-background px-3 text-sm"
                  value={form.gender ?? ""}
                  onChange={e => handleChange("gender", e.target.value === "" ? null : parseInt(e.target.value, 10))}
                >
                  <option value="">Không xác định</option>
                  <option value="1">Nam</option>
                  <option value="2">Nữ</option>
                  <option value="0">Khác</option>
                </select>
              </div>
              <div>
                <label className="text-xs font-medium text-muted-foreground">Ngành học</label>
                <select
                  className="mt-1 h-10 w-full rounded-lg border border-input bg-background px-3 text-sm disabled:opacity-50"
                  value={form.majorId ?? ""}
                  onChange={e => handleChange("majorId", e.target.value === "" ? null : e.target.value)}
                  disabled={loadingMajors}
                >
                  <option value="">{loadingMajors ? "Đang tải..." : "Chọn ngành học"}</option>
                  {majors.map(m => <option key={m.id} value={m.id}>{m.majorName}</option>)}
                </select>
              </div>
            </div>
            <div>
              <label className="text-xs font-medium text-muted-foreground">Giới thiệu</label>
              <textarea
                value={form.introduction ?? ""}
                onChange={e => handleChange("introduction", e.target.value)}
                placeholder="Giới thiệu ngắn gọn về bản thân..."
                className="mt-1 min-h-[80px] w-full rounded-lg border border-input bg-background px-3 py-2 text-sm resize-none"
              />
            </div>
            <Button onClick={handleSubmit} disabled={saving} className="gap-2">
              {saving && <Loader2 className="h-4 w-4 animate-spin" />}
              Lưu thay đổi
            </Button>
          </div>

          <hr className="border-border" />

          {/* AI Suggestion Toggle */}
          <div className="space-y-3">
            <h3 className="font-semibold text-foreground">Gợi ý nhóm học</h3>
            <div className="flex items-center justify-between p-3 bg-muted/50 rounded-lg">
              <div>
                <p className="text-sm font-medium text-foreground">Cho phép AI gợi ý nhóm</p>
                <p className="text-xs text-muted-foreground">Hệ thống sẽ tự động gợi ý bạn tham gia các nhóm học phù hợp</p>
              </div>
              <button
                type="button"
                onClick={() => handleChange("isSuggest", !form.isSuggest)}
                className={`relative inline-flex h-6 w-11 shrink-0 cursor-pointer rounded-full border-2 border-transparent transition-colors duration-200 focus:outline-none focus:ring-2 focus:ring-primary focus:ring-offset-2 ${form.isSuggest ? 'bg-primary' : 'bg-muted-foreground/30'}`}
              >
                <span className={`pointer-events-none inline-block h-5 w-5 transform rounded-full bg-white shadow-lg ring-0 transition duration-200 ${form.isSuggest ? 'translate-x-5' : 'translate-x-0'}`} />
              </button>
            </div>
          </div>

          {/* Username Change */}
          <div className="space-y-3">
            <h3 className="font-semibold text-foreground">Đổi tên đăng nhập</h3>
            <p className="text-xs text-muted-foreground">
              Hiện tại: <span className="font-medium text-foreground">@{profile.username || 'chưa có'}</span>
            </p>
            <div className="flex gap-2">
              <Input
                type="text"
                value={newUsername}
                onChange={e => setNewUsername(e.target.value)}
                placeholder="Tên đăng nhập mới"
                className="flex-1 rounded-lg"
              />
              <Button
                variant="outline"
                onClick={handleChangeUsername}
                disabled={changingUsername || !newUsername}
                className="gap-2"
              >
                {changingUsername && <Loader2 className="h-4 w-4 animate-spin" />}
                Đổi
              </Button>
            </div>
          </div>

          <hr className="border-border" />

          {/* Trust History */}
          <div className="space-y-3">
            <h3 className="font-semibold text-foreground">Lịch sử điểm tin cậy</h3>
            {loadingTrust ? (
              <div className="flex items-center gap-2 text-xs text-muted-foreground">
                <Loader2 className="h-3 w-3 animate-spin" />
                <span>Đang tải...</span>
              </div>
            ) : trustHistory.length === 0 ? (
              <p className="text-xs text-muted-foreground">Chưa có lịch sử điểm tin cậy.</p>
            ) : (
              <ul className="space-y-2 max-h-40 overflow-y-auto">
                {trustHistory.slice().sort((a, b) => new Date(b.createdAt || 0).getTime() - new Date(a.createdAt || 0).getTime()).map(item => (
                  <li key={item.id} className="flex items-start justify-between gap-2 text-sm">
                    <div>
                      <span className={`font-semibold ${(item.score ?? 0) > 0 ? 'text-green-600' : 'text-red-600'}`}>
                        {(item.score ?? 0) > 0 ? '+' : ''}{item.score}
                      </span>
                      {item.reason && <span className="ml-1 text-muted-foreground">– {item.reason}</span>}
                    </div>
                    {item.createdAt && (
                      <span className="shrink-0 text-xs text-muted-foreground">
                        {new Date(item.createdAt).toLocaleDateString('vi-VN')}
                      </span>
                    )}
                  </li>
                ))}
              </ul>
            )}
          </div>
        </div>
      )}
    </div>
  );
};

export default ProfilePage;
