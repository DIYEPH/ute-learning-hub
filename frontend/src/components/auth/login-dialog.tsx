"use client";

import { Button } from "../ui/button";
import { InputWithIcon } from "../ui/input-with-icon";
import { Label } from "../ui/label";
import { useTranslations } from 'next-intl';
import { Mail, Lock, Eye, EyeOff } from "lucide-react";
import { useState } from "react";
import { useAuth } from "@/src/hooks/use-auth";
import { useRouter } from "next/navigation";
import AuthDialog from "./auth-dialog";
import { useNotification } from "@/src/components/providers/notification-provider";

interface LoginDialogProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
}

export default function LoginDialog({ open, onOpenChange }: LoginDialogProps) {
    const t = useTranslations('auth');
    const tCommon = useTranslations('common');
    const [showPassword, setShowPassword] = useState(false);
    const [emailOrUsername, setEmailOrUsername] = useState('');
    const [password, setPassword] = useState('');
    const { handleMicrosoftLogin, handleLogin, loading } = useAuth();
    const router = useRouter();
    const { success: notifySuccess, error: notifyError } = useNotification();

    const handleEmailPasswordSubmit = async (e: React.FormEvent) => {
        e.preventDefault();

        if (!emailOrUsername.trim() || !password.trim())
            return;

        try {
            const result = await handleLogin(emailOrUsername.trim(), password);
            if (result) {
                onOpenChange(false);
                setEmailOrUsername('');
                setPassword('');
                notifySuccess("Đăng nhập thành công!");
                window.location.reload();
            }
        } catch (error: any) {
            notifyError(error?.message);
        }
    };

    const handleMicrosoftLoginClick = async () => {
        try {
            const result = await handleMicrosoftLogin();
            if (result) {
                onOpenChange(false);
                notifySuccess("Đăng nhập thành công!");
                window.location.reload();
            }
        } catch (error: any) {
            notifyError(error?.message);
        }
    };

    return (
        <AuthDialog open={open} onOpenChange={onOpenChange} title={t('loginTitle')}>
            <form onSubmit={handleEmailPasswordSubmit} className="space-y-4">
                <div className="space-y-1">
                    <Label>{t('emailOrUsername')}</Label>
                    <InputWithIcon
                        prefixIcon={Mail}
                        placeholder={t('emailOrUsernamePlaceholder')}
                        value={emailOrUsername}
                        onChange={(e) => setEmailOrUsername(e.target.value)}
                        required
                        disabled={loading}
                    />
                </div>
                <div className="space-y-1">
                    <Label>{t('password')}</Label>
                    <InputWithIcon
                        type={showPassword ? "text" : "password"}
                        prefixIcon={Lock}
                        suffixIcon={showPassword ? EyeOff : Eye}
                        placeholder={t('passwordPlaceholder')}
                        onSuffixClick={() => setShowPassword(!showPassword)}
                        value={password}
                        onChange={(e) => setPassword(e.target.value)}
                        required
                        disabled={loading}
                    />
                    <div className="text-right text-sm text-primary cursor-pointer mt-1 hover:underline">
                        {t('forgotPassword')}
                    </div>
                </div>
                <Button
                    type="submit"
                    className="w-full h-11 rounded-full text-base"
                    disabled={loading || !emailOrUsername.trim() || !password.trim()}
                >
                    {loading ? tCommon('loading') : t('loginTitle')}
                </Button>
                <div className="relative">
                    <div className="absolute inset-0 flex items-center">
                        <span className="w-full border-t border-border" />
                    </div>
                    <div className="relative flex justify-center text-xs uppercase">
                        <span className="bg-background px-2 text-muted-foreground">
                            {tCommon('or') || 'Hoặc'}
                        </span>
                    </div>
                </div>
                <Button
                    type="button"
                    className="w-full h-11 rounded-full text-base"
                    onClick={handleMicrosoftLoginClick}
                    disabled={loading}
                    variant="outline"
                >
                    {loading ? tCommon('loading') : t('loginWithSchoolEmail')}
                </Button>
            </form>
        </AuthDialog>
    );
}


