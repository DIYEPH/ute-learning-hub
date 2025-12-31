import { AppShell } from "../components/layout/app-shell";
import { getTranslations } from 'next-intl/server';
import { HomePageSections } from "../components/home/home-page-sections";
import { AppFooter } from "../components/layout/app-footer";

export default async function HomePage() {
  const t = await getTranslations('home');

  return (
    <AppShell>
      <div className="min-h-full flex flex-col">
        <section className="flex-1 space-y-6">
          <div>
            <h1 className="text-2xl font-semibold text-foreground">
              {t('welcome')}
            </h1>
            <p className="text-sm text-muted-foreground mt-1">
              {t('description')}
            </p>
          </div>

          <HomePageSections />
        </section>

        <AppFooter className="shrink-0 mt-8 -mx-4 md:-mx-6" />
      </div>
    </AppShell>
  );
}

