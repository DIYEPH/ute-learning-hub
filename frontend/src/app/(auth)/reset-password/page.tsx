"use client";

import React from "react";
import { useSearchParams, useRouter } from "next/navigation";
import { Loader2, Lock, CheckCircle, AlertCircle } from "lucide-react";
import { postApiAuthResetPassword } from "@/src/api";
import { Input } from "@/src/components/ui/input";
import { Button } from "@/src/components/ui/button";

export default function ResetPasswordPage() {
  const searchParams = useSearchParams();
  const router = useRouter();
  const token = searchParams.get("token") || "";
  const email = searchParams.get("email") || "";

  const [newPassword, setNewPassword] = React.useState("");
  const [confirmPassword, setConfirmPassword] = React.useState("");
  const [loading, setLoading] = React.useState(false);
  const [error, setError] = React.useState<string | null>(null);
  const [success, setSuccess] = React.useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    if (!token || !email) return setError("Link không hợp lệ.");
    if (newPassword.length < 6) return setError("Mật khẩu phải có ít nhất 6 ký tự.");
    if (newPassword !== confirmPassword) return setError("Mật khẩu xác nhận không khớp.");

    setLoading(true);
    try {
      await postApiAuthResetPassword<true>({ body: { email, token, newPassword }, throwOnError: true });
      setSuccess(true);
    } catch (err: any) {
      setError(err?.response?.data?.message || err?.message || "Không thể đặt lại mật khẩu.");
    } finally {
      setLoading(false);
    }
  };

  if (success) {
    return (
      <div className="flex min-h-screen items-center justify-center bg-muted p-4">
        <div className="w-full max-w-md space-y-6 border border-border bg-card p-8 shadow-sm text-center">
          <CheckCircle className="mx-auto h-16 w-16 text-emerald-500" />
          <h1 className="text-2xl font-semibold">Đặt lại mật khẩu thành công!</h1>
          <p className="text-sm text-muted-foreground">Bạn có thể đăng nhập với mật khẩu mới.</p>
          <Button onClick={() => router.push("/auth/login")}>Đăng nhập</Button>
        </div>
      </div>
    );
  }

  return (
    <div className="flex min-h-screen items-center justify-center bg-muted p-4">
      <div className="w-full max-w-md space-y-6 border border-border bg-card p-8 shadow-sm">
        <div className="text-center">
          <Lock className="mx-auto h-12 w-12 text-primary" />
          <h1 className="mt-4 text-2xl font-semibold">Đặt lại mật khẩu</h1>
          <p className="mt-2 text-sm text-muted-foreground">Nhập mật khẩu mới cho tài khoản của bạn</p>
        </div>

        {(!token || !email) && (
          <div className="flex items-center gap-2 border border-amber-200 bg-amber-50 p-3 text-sm text-amber-700">
            <AlertCircle className="h-4 w-4" />
            <span>Link không hợp lệ. Vui lòng yêu cầu đặt lại mật khẩu mới.</span>
          </div>
        )}

        {error && (
          <div className="flex items-center gap-2 border border-red-200 bg-red-50 p-3 text-sm text-red-700">
            <AlertCircle className="h-4 w-4" />
            <span>{error}</span>
          </div>
        )}

        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label className="text-xs font-medium text-muted-foreground">Email</label>
            <Input type="email" value={email} disabled className="mt-1 h-10 bg-muted" />
          </div>
          <div>
            <label className="text-xs font-medium text-muted-foreground">Mật khẩu mới</label>
            <Input type="password" value={newPassword} onChange={(e) => setNewPassword(e.target.value)} placeholder="Nhập mật khẩu mới" className="mt-1 h-10" autoComplete="new-password" required />
          </div>
          <div>
            <label className="text-xs font-medium text-muted-foreground">Xác nhận mật khẩu</label>
            <Input type="password" value={confirmPassword} onChange={(e) => setConfirmPassword(e.target.value)} placeholder="Nhập lại mật khẩu mới" className="mt-1 h-10" autoComplete="new-password" required />
          </div>
          <Button type="submit" className="w-full h-10" disabled={loading || !token || !email}>
            {loading && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
            Đặt lại mật khẩu
          </Button>
        </form>

        <div className="text-center">
          <button type="button" onClick={() => router.push("/auth/login")} className="text-sm text-primary hover:underline">
            Quay lại đăng nhập
          </button>
        </div>
      </div>
    </div>
  );
}
