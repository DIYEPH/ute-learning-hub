"use client";

import { Button } from "../ui/button";
import { InputWithIcon } from "../ui/input-with-icon";
import { Label } from "../ui/label";
import { useTranslations } from 'next-intl';
import { Mail, Lock, Eye, EyeOff } from "lucide-react";
import { useState } from "react";
import AuthDialog from "./auth-dialog";

interface LoginDialogProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
}
export default function LoginDialog({ open, onOpenChange }: LoginDialogProps) {
    const t = useTranslations('auth');
    const [showPassword, setShowPassword] = useState(false);

    return (
        <AuthDialog open={open} onOpenChange={onOpenChange} title={t('loginTitle')}>
            <div className="space-y-4">
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
                <Button className="w-full h-11 rounded-full text-base">{t('loginTitle')}</Button>
                <Button className="w-full h-11 rounded-full text-base">{t('loginWithSchoolEmail')}</Button>
            </div>
        </AuthDialog>
    );
}
