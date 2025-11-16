"use client";

export function AppFooter() {
  const year = new Date().getFullYear();

  return (
    <footer className="border-t bg-white py-3 px-4 text-xs md:text-sm text-slate-500 flex items-center justify-between">
      <span>© {year} UTE Learning Hub. All rights reserved.</span>
      <span className="hidden sm:inline">
        Built for University of Technology and Education.
      </span>
    </footer>
  );
}
