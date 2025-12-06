import { AdminLayout } from "@/src/components/admin/admin-layout";
import { NotificationProvider } from "@/src/components/ui/notification-center";

export default function Layout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <AdminLayout>
      <NotificationProvider>
        {children}
      </NotificationProvider>
    </AdminLayout>
  );
}
