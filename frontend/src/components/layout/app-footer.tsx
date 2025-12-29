"use client";

import { cn } from "@/lib/utils";

interface AppFooterProps {
  className?: string;
}

export function AppFooter({ className }: AppFooterProps) {
  const year = new Date().getFullYear();

  return (
    <footer className={cn(
      "border-t bg-card border-border py-3 px-4 text-xs md:text-sm text-muted-foreground flex items-center justify-between",
      className
    )}>
      <span>© {year} UTE Ahihi</span>
      <span className="hidden sm:inline">
        {/* Built for University of Technology and Education. */}
      </span>
    </footer>
  );
}

