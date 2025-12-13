import { getTranslations } from 'next-intl/server';

export default async function ResetPasswordPage() {
  const t = await getTranslations('auth');

  return (
    <div>
      <h1>{t('resetPasswordTitle')}</h1>
      <p>{t('resetPasswordDescription')}</p>
    </div>
  );
}
