"use client";

import Link from "next/link";
import { Button } from "../ui/button";
import { Input } from "../ui/input";
import type { NavItem } from "./nav-config";
import MobileSidebar from "./app-mobile-sidebar";

type HeaderProps = {
  navItems: NavItem[];
  activePath?: string;
};

export function AppHeader({ navItems, activePath }: HeaderProps) {
  return (
    <header className="h-16 border-b bg-white flex items-center px-4 gap-4">
      {/* Mobile: nút menu mở sidebar */}
      <div className="md:hidden">
        <MobileSidebar navItems={navItems} activePath={activePath} />
      </div>

      {/* Logo */}
      <Link href="/" className="flex items-center gap-2 font-semibold text-lg">
        <span className="inline-flex h-8 w-8 items-center justify-center rounded-full bg-slate-900 text-white">
          U
        </span>
        <span className="hidden sm:inline">UTE Learning Hub</span>
      </Link>

      {/* Search */}
      <div className="flex-1 max-w-xl mx-auto w-full">
        <Input
          placeholder="Search for courses, quizzes, or documents"
          className="rounded-full"
        />
      </div>

      {/* Auth buttons */}
      <div className="flex items-center gap-2">
        {/* <Button variant="outline" className="hidden sm:inline-flex rounded-full">
          Log in
        </Button> */}
        <Button className="rounded-full bg-green-500 hover:bg-green-600">
          Login
        </Button>
      </div>
    </header>
  );
}