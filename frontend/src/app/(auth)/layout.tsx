// src/app/(auth)/layout.tsx
import { getTranslations } from 'next-intl/server';

export default async function AuthLayout({
    children,
  }: {
    children: React.ReactNode;
  }) {
    const t = await getTranslations('auth');

    return (
      <div className="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100">
        <div className="flex min-h-screen">
          {/* Left side - Branding */}
          <div className="hidden lg:flex lg:w-1/2 bg-gradient-to-br from-blue-600 to-purple-700 items-center justify-center">
            <div className="text-center text-white">
              <h1 className="text-4xl font-bold mb-4">{t('title')}</h1>
              <p className="text-xl opacity-90">{t('subtitle')}</p>
              <div className="mt-8">
                <div className="flex items-center justify-center space-x-8">
                  <div className="text-center">
                    <div className="text-3xl mb-2">📚</div>
                    <p>{t('feature1')}</p>
                  </div>
                  <div className="text-center">
                    <div className="text-3xl mb-2">👥</div>
                    <p>{t('feature2')}</p>
                  </div>
                  <div className="text-center">
                    <div className="text-3xl mb-2">💬</div>
                    <p>{t('feature3')}</p>
                  </div>
                </div>
              </div>
            </div>
          </div>
  
          {/* Right side - Auth forms */}
          <div className="w-full lg:w-1/2 flex items-center justify-center p-8">
            {children}
          </div>
        </div>
      </div>
    );
  }