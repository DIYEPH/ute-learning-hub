import { redirect } from "next/navigation";

export default function AdminDashboardPage() {
  // Redirect to statistics page or first available admin page
  redirect("/admin/statistics");
}
