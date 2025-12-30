"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import Link from "next/link";
import { Loader2, Mail, CheckCircle, AlertCircle, ArrowLeft } from "lucide-react";
import { postApiAuthForgotPassword } from "@/src/api";
import { Input } from "@/src/components/ui/input";
import { Button } from "@/src/components/ui/button";

export default function ForgotPasswordPage() {
    const router = useRouter();
    const [email, setEmail] = useState("");
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [success, setSuccess] = useState(false);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setError(null);

        if (!email) return setError("Vui lòng nhập email.");
        if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) return setError("Email không hợp lệ.");

        setLoading(true);
        try {
            await postApiAuthForgotPassword<true>({ body: { email }, throwOnError: true });
        } catch {
            // Show success even if user doesn't exist (security)
        } finally {
            setLoading(false);
            setSuccess(true);
        }
    };

    if (success) {
        return (
            <div className="flex min-h-screen items-center justify-center bg-muted p-4">
                <div className="w-full max-w-md space-y-6 rounded-lg border bg-card p-8 text-center">
                    <CheckCircle className="mx-auto h-16 w-16 text-emerald-500" />
                    <h1 className="text-2xl font-semibold">Kiểm tra email của bạn</h1>
                    <p className="text-sm text-muted-foreground">
                        Nếu tài khoản với email <strong>{email}</strong> tồn tại, chúng tôi đã gửi link đặt lại mật khẩu.
                    </p>
                    <p className="text-xs text-muted-foreground">
                        Vui lòng kiểm tra hộp thư (bao gồm cả thư rác) và click vào link trong email.
                    </p>
                    <div className="space-y-2 pt-4">
                        <Button onClick={() => router.push("/")} className="w-full">
                            Về trang chủ
                        </Button>
                        <button
                            type="button"
                            onClick={() => { setSuccess(false); setEmail(""); }}
                            className="text-sm text-primary hover:underline"
                        >
                            Gửi lại email
                        </button>
                    </div>
                </div>
            </div>
        );
    }

    return (
        <div className="flex min-h-screen items-center justify-center bg-muted p-4">
            <div className="w-full max-w-md space-y-6 rounded-lg border bg-card p-8">
                <div className="text-center">
                    <Mail className="mx-auto h-12 w-12 text-primary" />
                    <h1 className="mt-4 text-2xl font-semibold">Quên mật khẩu?</h1>
                    <p className="mt-2 text-sm text-muted-foreground">
                        Nhập email của bạn để nhận link đặt lại mật khẩu
                    </p>
                </div>

                {error && (
                    <div className="flex items-center gap-2 rounded-lg border border-red-200 bg-red-50 p-3 text-sm text-red-700 dark:border-red-800 dark:bg-red-950 dark:text-red-300">
                        <AlertCircle className="h-4 w-4 shrink-0" />
                        <span>{error}</span>
                    </div>
                )}

                <form onSubmit={handleSubmit} className="space-y-4">
                    <div>
                        <label className="text-xs font-medium text-muted-foreground">Email</label>
                        <Input
                            type="email"
                            value={email}
                            onChange={(e) => setEmail(e.target.value)}
                            placeholder="your@email.com"
                            className="mt-1"
                            autoComplete="email"
                            required
                        />
                    </div>
                    <Button type="submit" className="w-full" disabled={loading}>
                        {loading && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
                        Gửi link đặt lại mật khẩu
                    </Button>
                </form>

                <div className="text-center">
                    <Link href="/" className="inline-flex items-center gap-1 text-sm text-primary hover:underline">
                        <ArrowLeft className="h-4 w-4" />
                        Quay lại trang chủ
                    </Link>
                </div>
            </div>
        </div>
    );
}
