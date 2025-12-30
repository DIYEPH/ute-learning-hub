"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { useTranslations } from 'next-intl';
import { Mail, Lock, Eye, EyeOff, AlertCircle, Loader2 } from "lucide-react";
import { Button } from "../ui/button";
import { InputWithIcon } from "../ui/input-with-icon";
import { Label } from "../ui/label";
import { useAuth } from "@/src/hooks/use-auth";
import { useNotification } from "@/src/components/providers/notification-provider";
import AuthDialog from "./auth-dialog";

interface LoginDialogProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
}

export default function LoginDialog({ open, onOpenChange }: LoginDialogProps) {
    const t = useTranslations('auth');
    const tCommon = useTranslations('common');
    const router = useRouter();
    const { handleMicrosoftLogin, handleLogin } = useAuth();
    const { success: notifySuccess } = useNotification();

    const [showPassword, setShowPassword] = useState(false);
    const [emailOrUsername, setEmailOrUsername] = useState('');
    const [password, setPassword] = useState('');
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [errorMessage, setErrorMessage] = useState<string | null>(null);

    const clearError = () => {
        if (errorMessage) setErrorMessage(null);
    };

    const handleSuccess = () => {
        notifySuccess("Đăng nhập thành công!");
        onOpenChange(false);
        setEmailOrUsername('');
        setPassword('');
        window.location.reload();
    };

    const handleEmailPasswordSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        if (!emailOrUsername.trim() || !password.trim() || isSubmitting) return;

        setErrorMessage(null);
        setIsSubmitting(true);

        try {
            await handleLogin(emailOrUsername.trim(), password);
            handleSuccess();
        } catch (error) {
            const err = error as Error;
            setErrorMessage(err?.message || "Đăng nhập thất bại. Vui lòng thử lại.");
        } finally {
            setIsSubmitting(false);
        }
    };

    const handleMicrosoftLoginClick = async () => {
        if (isSubmitting) return;

        setErrorMessage(null);
        setIsSubmitting(true);

        try {
            await handleMicrosoftLogin();
            handleSuccess();
        } catch (error) {
            const err = error as Error;
            setErrorMessage(err?.message || "Đăng nhập thất bại. Vui lòng thử lại.");
        } finally {
            setIsSubmitting(false);
        }
    };

    const canSubmit = emailOrUsername.trim() && password.trim() && !isSubmitting;

    return (
        <AuthDialog open={open} onOpenChange={onOpenChange} title={t('loginTitle')}>
            <form onSubmit={handleEmailPasswordSubmit} className="space-y-4">
                {errorMessage && (
                    <div className="flex items-center gap-2 p-3 text-sm text-red-600 bg-red-50 dark:bg-red-950/50 dark:text-red-400 border border-red-200 dark:border-red-800 rounded-lg">
                        <AlertCircle className="h-4 w-4 shrink-0" />
                        <span>{errorMessage}</span>
                    </div>
                )}

                <div className="space-y-1">
                    <Label>{t('emailOrUsername')}</Label>
                    <InputWithIcon
                        prefixIcon={Mail}
                        placeholder={t('emailOrUsernamePlaceholder')}
                        value={emailOrUsername}
                        onChange={(e) => { setEmailOrUsername(e.target.value); clearError(); }}
                        required
                        disabled={isSubmitting}
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
                        onChange={(e) => { setPassword(e.target.value); clearError(); }}
                        required
                        disabled={isSubmitting}
                    />
                    <div
                        className="text-right text-sm text-primary cursor-pointer mt-1 hover:underline"
                        onClick={() => { onOpenChange(false); router.push('/forgot-password'); }}
                    >
                        {t('forgotPassword')}
                    </div>
                </div>

                <Button
                    type="submit"
                    className="w-full h-11 rounded-full text-base"
                    disabled={!canSubmit}
                >
                    {isSubmitting ? (
                        <>
                            <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                            {tCommon('loading')}
                        </>
                    ) : (
                        t('loginTitle')
                    )}
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
                    disabled={isSubmitting}
                    variant="outline"
                >
                    {isSubmitting ? (
                        <>
                            <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                            {tCommon('loading')}
                        </>
                    ) : (
                        t('loginWithSchoolEmail')
                    )}
                </Button>
            </form>
        </AuthDialog>
    );
}
