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
    const [emailOrUsername, setEmailOrUsername] = useState('');
    const [password, setPassword] = useState('');
    const { handleMicrosoftLogin, handleEmailPasswordLogin, loading, error } = useAuth();
    const router = useRouter();

    const handleEmailPasswordSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        
        if (!emailOrUsername.trim() || !password.trim()) {
            return;
        }

        try {
            const result = await handleEmailPasswordLogin(emailOrUsername.trim(), password);
            if (result) {
                // Đóng dialog sau khi login thành công
                onOpenChange(false);
                
                // Reset form
                setEmailOrUsername('');
                setPassword('');
                
                // Reload page để update auth state
                router.refresh();
            }
        } catch (error) {
            // Error đã được handle trong hook
            console.error('Login failed:', error);
        }
    };

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
            <form onSubmit={handleEmailPasswordSubmit} className="space-y-4">
                {error && (
                    <div className="p-3 text-sm text-red-600 dark:text-red-400 bg-red-50 dark:bg-red-950 rounded-md border border-red-200 dark:border-red-800">
                        {error}
                    </div>
                )}
                
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
                    <div className="text-right text-sm text-blue-600 dark:text-blue-400 cursor-pointer mt-1 hover:underline">
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
                        <span className="w-full border-t border-slate-200 dark:border-slate-700" />
                    </div>
                    <div className="relative flex justify-center text-xs uppercase">
                        <span className="bg-background px-2 text-slate-500 dark:text-slate-400">
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
