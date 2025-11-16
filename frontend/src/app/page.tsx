import { AppShell } from "../components/layout/app-shell";

export default function HomePage() {
  return (
    <AppShell>
      {/* Nội dung ở vùng bên phải – giống phần giữa của Studocu */}
      <section className="space-y-4">
        <h1 className="text-2xl font-semibold">
          Welcome to UTE Learning Hub 👋
        </h1>
        <p className="text-slate-600">
          Đây là trang home – chỗ để sinh viên khám phá tài liệu, quiz, câu hỏi, v.v.
        </p>

        {/* Sau này bạn thêm các card “Create a quiz”, “Ask a question”, … ở đây */}
      </section>
    </AppShell>
  );
}
