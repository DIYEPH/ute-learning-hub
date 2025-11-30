import { AdminLayout } from "@/src/components/admin/admin-layout";

export default function Layout({
  children,
}: {
  children: React.ReactNode;
}) {
  return <AdminLayout>{children}</AdminLayout>;
}

