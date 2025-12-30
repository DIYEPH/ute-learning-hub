import type { Metadata } from "next";
import { Be_Vietnam_Pro } from "next/font/google";
import { NextIntlClientProvider } from 'next-intl';
import { getMessages, getLocale } from 'next-intl/server';
import { ThemeProvider } from '@/src/components/providers/theme-provider';
import { NotificationProvider } from '@/src/components/providers/notification-provider';
import { SignalRProvider } from '@/src/components/providers/signalr-provider';
import "./globals.css";

const beVietnamPro = Be_Vietnam_Pro({
  variable: "--font-be-vietnam-pro",
  subsets: ["latin", "vietnamese"],
  weight: ["300", "400", "500", "600", "700"],
});

export const metadata: Metadata = {
  title: "UteLearningHub",
  description: "Chia sẻ tài liệu",
  icons: {
    icon: "/images/ute_logo.png",
    shortcut: "/images/ute_logo.png",
    apple: "/images/ute_logo.png",
  },
};

export default async function RootLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  const locale = await getLocale();
  const messages = await getMessages();

  return (
    <html lang={locale} suppressHydrationWarning>
      <body className={`${beVietnamPro.variable} font-sans antialiased`}>
        <ThemeProvider>
          <NextIntlClientProvider messages={messages}>
            <SignalRProvider>
              <NotificationProvider>
                {children}
              </NotificationProvider>
            </SignalRProvider>
          </NextIntlClientProvider>
        </ThemeProvider>
      </body>
    </html>
  );
}

