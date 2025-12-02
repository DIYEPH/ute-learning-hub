import { AppShell } from "@/src/components/layout/app-shell";
import { NotificationProvider } from "@/src/components/ui/notification-center";

export default function Layout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <NotificationProvider>
      <AppShell>{children}</AppShell>
    </NotificationProvider>
  );
}

