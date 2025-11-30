import { AppShell } from "../components/layout/app-shell";
import { getTranslations } from 'next-intl/server';

export default async function HomePage() {
  const t = await getTranslations('dashboard');

  return (
    <AppShell>
      {/* Nội dung ở vùng bên phải – giống phần giữa của Studocu */}
      <section className="space-y-4">
        <h1 className="text-2xl font-semibold text-foreground">
          {t('welcome')} 👋
        </h1>
        <p className="text-slate-600 dark:text-slate-400">
          {t('description')}
        </p>

        {/* Sau này bạn thêm các card "Create a quiz", "Ask a question", … ở đây */}
      </section>
    </AppShell>
  );
}
