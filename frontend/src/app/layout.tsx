import type { Metadata } from "next";
import { Geist, Geist_Mono } from "next/font/google";
import { NextIntlClientProvider } from 'next-intl';
import { getMessages, getLocale } from 'next-intl/server';
import { ThemeProvider } from '@/src/components/providers/theme-provider';
import { NotificationProvider } from '@/src/components/providers/notification-provider';
import { SignalRProvider } from '@/src/components/providers/signalr-provider';
import "./globals.css";

const geistSans = Geist({
  variable: "--font-geist-sans",
  subsets: ["latin"],
});

const geistMono = Geist_Mono({
  variable: "--font-geist-mono",
  subsets: ["latin"],
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
      <body className={`${geistSans.variable} ${geistMono.variable} antialiased`}>
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

