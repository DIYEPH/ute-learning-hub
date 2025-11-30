"use client";

import { Calendar } from "lucide-react";
import { Button } from "../ui/button";
import { DropdownMenuWrapper } from "../ui/dropdown-menu-wrapper";
import {
  DropdownMenuItem,
  DropdownMenuSeparator,
} from "../ui/dropdown-menu";
import Link from "next/link";

/**
 * Ví dụ sử dụng DropdownMenuWrapper cho events
 */
export function EventMenu() {
  // TODO: Fetch events từ API
  const upcomingEvents = [
    { id: 1, title: "Deadline nộp bài tập", date: "2025-12-01", time: "23:59" },
    { id: 2, title: "Kiểm tra giữa kỳ", date: "2025-12-05", time: "08:00" },
  ];

  return (
    <DropdownMenuWrapper
      trigger={
        <Button
          variant="ghost"
          size="sm"
          className="h-9 w-9 rounded-full p-0 hover:bg-slate-100 dark:hover:bg-slate-800"
          aria-label="Calendar"
        >
          <Calendar size={18} className="text-slate-600 dark:text-slate-400" />
        </Button>
      }
      align="end"
      contentClassName="w-64"
    >
      <div className="px-2 py-1.5">
        <p className="text-sm font-medium">Sự kiện sắp tới</p>
      </div>
      <DropdownMenuSeparator />
      {upcomingEvents.length > 0 ? (
        <>
          {upcomingEvents.map((event) => (
            <DropdownMenuItem key={event.id} asChild>
              <Link href={`/events/${event.id}`} className="flex flex-col items-start cursor-pointer">
                <p className="text-sm font-medium">{event.title}</p>
                <p className="text-xs text-slate-500 dark:text-slate-400">
                  {event.date} lúc {event.time}
                </p>
              </Link>
            </DropdownMenuItem>
          ))}
          <DropdownMenuSeparator />
          <DropdownMenuItem asChild>
            <Link href="/events" className="cursor-pointer text-center w-full">
              Xem tất cả
            </Link>
          </DropdownMenuItem>
        </>
      ) : (
        <DropdownMenuItem disabled>
          <p className="text-sm text-slate-500 dark:text-slate-400">Không có sự kiện sắp tới</p>
        </DropdownMenuItem>
      )}
    </DropdownMenuWrapper>
  );
}

