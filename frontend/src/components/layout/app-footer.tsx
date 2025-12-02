"use client";

import { cn } from "@/lib/utils";

interface AppFooterProps {
  className?: string;
}

export function AppFooter({ className }: AppFooterProps) {
  const year = new Date().getFullYear();

  return (
    <footer className={cn(
      "border-t bg-white dark:bg-slate-900 border-slate-200 dark:border-slate-800 py-3 px-4 text-xs md:text-sm text-slate-500 dark:text-slate-400 flex items-center justify-between",
      className
    )}>
      <span>© {year} UTE Ahihi</span>
      <span className="hidden sm:inline">
        {/* Built for University of Technology and Education. */}
      </span>
    </footer>
  );
}
