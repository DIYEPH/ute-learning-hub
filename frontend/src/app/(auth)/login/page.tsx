'use client';

import LoginDialog from "@/src/components/auth/login-dialog";
import { useTranslations } from 'next-intl';
import { useState } from "react";

export default function Page() {
  const [open, setOpen] = useState(false);
  const t = useTranslations('common');

  return (
    <>
      <button onClick={() => setOpen(true)}>{t('login')}</button>
      <LoginDialog open={open} onOpenChange={setOpen} />
    </>
  );
}