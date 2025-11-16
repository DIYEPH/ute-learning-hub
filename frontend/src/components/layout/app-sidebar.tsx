"use client";

import Link from "next/link";
import { ScrollArea} from "../ui/scroll-area"
import { Button } from "../ui/button";
import { Avatar, AvatarFallback } from "../ui/avatar"
import type { NavItem } from "./nav-config";

type SidebarProps = {
  navItems: NavItem[];
  activePath?: string;
};

export function AppSidebar({ navItems, activePath }: SidebarProps) {
  return (
    <aside className="hidden md:flex w-64 border-r bg-white">
      <ScrollArea className="w-full">
        <div className="p-4 space-y-4">
          {/* User box */}
          <div className="flex items-center gap-3">
            <Avatar>
              <AvatarFallback>GU</AvatarFallback>
            </Avatar>
            <div className="text-sm">
              <div className="font-semibold">Guest user</div>
              <button className="text-xs text-sky-600">
                + Add your university
              </button>
            </div>
          </div>

          <Button className="w-full rounded-full mt-2">+ New</Button>

          {/* Nav */}
          <nav className="mt-4 space-y-1 text-sm">
            {navItems.map((item) => {
              const Icon = item.icon;
              const isActive = activePath === item.href;
              return (
                <Link key={item.href} href={item.href}>
                  <button
                    className={`flex w-full items-center gap-3 rounded-lg px-3 py-2 text-left hover:bg-slate-100 ${
                      isActive
                        ? "bg-slate-100 text-slate-900 font-medium"
                        : "text-slate-600"
                    }`}
                  >
                    <span className="shrink-0">
                      <Icon size={18} />
                    </span>
                    <span>{item.label}</span>
                  </button>
                </Link>
              );
            })}
          </nav>
        </div>
      </ScrollArea>
    </aside>
  );
}
