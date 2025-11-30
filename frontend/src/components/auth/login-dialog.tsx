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

interface LoginDialogProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
}

export default function LoginDialog({ open, onOpenChange }: LoginDialogProps) {
    const t = useTranslations('auth');
    const tCommon = useTranslations('common');
    const [showPassword, setShowPassword] = useState(false);
    const { handleMicrosoftLogin, loading, error } = useAuth();
    const router = useRouter();

    const handleMicrosoftLoginClick = async () => {
        try {
            const result = await handleMicrosoftLogin();
            if (result) {
                // Đóng dialog sau khi login thành công
                onOpenChange(false);
                
                // Reload page để update auth state
                router.refresh();
                
                // Redirect nếu cần setup
                if (result.requiresSetup) {
                    router.push('/profile');
                }
            }
        } catch (error) {
            // Error đã được handle trong hook
            console.error('Login failed:', error);
        }
    };

    return (
        <AuthDialog open={open} onOpenChange={onOpenChange} title={t('loginTitle')}>
            <div className="space-y-4">
                {error && (
                    <div className="p-3 text-sm text-red-600 bg-red-50 rounded-md border border-red-200">
                        {error}
                    </div>
                )}
                
                <div className="space-y-1">
                    <Label>{t('emailOrUsername')}</Label>
                    <InputWithIcon
                        prefixIcon={Mail}
                        placeholder={t('emailOrUsernamePlaceholder')}
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
                    />
                    <div className="text-right text-sm text-blue-600 cursor-pointer mt-1">
                        {t('forgotPassword')}
                    </div>
                </div>
                <Button 
                    className="w-full h-11 rounded-full text-base"
                    disabled={loading}
                >
                    {loading ? tCommon('loading') : t('loginTitle')}
                </Button>
                <Button 
                    className="w-full h-11 rounded-full text-base"
                    onClick={handleMicrosoftLoginClick}
                    disabled={loading}
                    variant="outline"
                >
                    {loading ? tCommon('loading') : t('loginWithSchoolEmail')}
                </Button>
            </div>
        </AuthDialog>
    );
}
