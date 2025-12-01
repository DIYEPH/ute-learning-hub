// src/app/(auth)/layout.tsx
import { getTranslations } from 'next-intl/server';

export default async function AuthLayout({
    children,
  }: {
    children: React.ReactNode;
  }) {
    const t = await getTranslations('auth');

    return (
      <>{children}</>
    );
  }