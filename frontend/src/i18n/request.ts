import {getRequestConfig} from 'next-intl/server';
import {routing} from './routing';
import {cookies} from 'next/headers';

export default getRequestConfig(async ({requestLocale}) => {
  let locale = await requestLocale;

  // Try to get locale from cookie if not in request
  if (!locale) {
    const cookieStore = await cookies();
    const cookieLocale = cookieStore.get('NEXT_LOCALE')?.value;
    if (cookieLocale && routing.locales.includes(cookieLocale as any)) {
      locale = cookieLocale;
    }
  }

  if (!locale || !routing.locales.includes(locale as any)) {
    locale = routing.defaultLocale;
  }

  return {
    locale,
    messages: (await import(`../messages/${locale}.json`)).default
  };
});
