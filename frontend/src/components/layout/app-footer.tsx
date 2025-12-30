"use client";

import { cn } from "@/lib/utils";

interface AppFooterProps {
  className?: string;
}

export function AppFooter({ className }: AppFooterProps) {
  const year = new Date().getFullYear();

  return (
    <footer className={cn(
      "border-t bg-card border-border py-6 px-4 text-xs md:text-sm text-muted-foreground",
      className
    )}>
      <div className="max-w-6xl mx-auto flex flex-col md:flex-row gap-6 md:gap-12">
        {/* University Info */}
        <div className="flex-1 space-y-1">
          <h4 className="font-semibold text-foreground">TRƯỜNG ĐẠI HỌC SƯ PHẠM KỸ THUẬT</h4>
          <p>Cơ sở 1: 48 Cao Thắng, Thanh Bình, Hải Châu, Đà Nẵng</p>
          <p>Cơ sở 2: Khu Đô thị đại học, Hòa Quý, Ngũ Hành Sơn, Đà Nẵng</p>
        </div>

        {/* Contact Info */}
        <div className="space-y-1">
          <h4 className="font-semibold text-foreground">THÔNG TIN LIÊN LẠC</h4>
          <p>Điện thoại: 0236 3822 571</p>
          <p>Email: pdt@ute.udn.vn</p>
        </div>
      </div>
    </footer>
  );
}

