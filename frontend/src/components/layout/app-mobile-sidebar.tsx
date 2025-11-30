import { Menu } from "lucide-react";
import { Button } from "../ui/button";
import { Sheet, SheetContent, SheetHeader, SheetTitle, SheetTrigger } from "../ui/sheet";
import { NavItem } from "./nav-config";
import { ScrollArea } from "../ui/scroll-area";
import { Avatar, AvatarFallback } from "../ui/avatar";
import Link from "next/link";

export default function MobileSidebar({
    navItems,
    activePath,
  }: {
    navItems: NavItem[];
    activePath?: string;
  }) {
    return (
      <Sheet>
        <SheetTrigger asChild>
          <Button variant="ghost" size="icon">
            <Menu />
          </Button>
        </SheetTrigger>
        <SheetContent side="left" className="p-0 w-64">
          <SheetHeader>
          <SheetTitle>UTE Learning Hub</SheetTitle>
          </SheetHeader>
          <ScrollArea className="h-full">
            <div className="p-4 space-y-4">
              {/* <div className="flex items-center gap-3">
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
  
              <Button className="w-full rounded-full mt-2">+ New</Button> */}
  
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
        </SheetContent>
      </Sheet>
    );
  }