import { AppShell } from "@/src/components/layout/app-shell";
import { NotificationProvider } from "@/src/components/ui/notification-center";
import { SignalRProvider } from "@/src/components/providers/signalr-provider";

export default function Layout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <NotificationProvider>
      <SignalRProvider>
        <AppShell>{children}</AppShell>
      </SignalRProvider>
    </NotificationProvider>
  );
}
