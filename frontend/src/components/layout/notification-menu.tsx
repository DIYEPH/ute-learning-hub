"use client";

import { Bell } from "lucide-react";
import { Button } from "../ui/button";
import { DropdownMenuWrapper } from "../ui/dropdown-menu-wrapper";
import {
  DropdownMenuItem,
  DropdownMenuSeparator,
} from "../ui/dropdown-menu";

/**
 * Ví dụ sử dụng DropdownMenuWrapper cho notifications
 */
export function NotificationMenu() {
  // TODO: Fetch notifications từ API
  const notifications = [
    { id: 1, message: "Bạn có 3 tài liệu mới", time: "5 phút trước" },
    { id: 2, message: "Ai đó đã comment vào tài liệu của bạn", time: "1 giờ trước" },
  ];

  const unreadCount = notifications.length;

  return (
    <DropdownMenuWrapper
      trigger={
        <Button
          variant="ghost"
          size="sm"
          className="h-9 w-9 rounded-full p-0 hover:bg-slate-100 dark:hover:bg-slate-800 relative"
          aria-label="Notifications"
        >
          <Bell size={18} className="text-slate-600 dark:text-slate-400" />
          {unreadCount > 0 && (
            <span className="absolute -top-1 -right-1 h-5 w-5 rounded-full bg-red-500 text-white text-xs flex items-center justify-center">
              {unreadCount > 9 ? '9+' : unreadCount}
            </span>
          )}
        </Button>
      }
      align="end"
      contentClassName="w-80"
    >
      <div className="px-2 py-1.5">
        <p className="text-sm font-medium">Thông báo</p>
      </div>
      <DropdownMenuSeparator />
      {notifications.length > 0 ? (
        notifications.map((notification) => (
          <DropdownMenuItem key={notification.id} className="flex flex-col items-start">
            <p className="text-sm">{notification.message}</p>
            <p className="text-xs text-slate-500 dark:text-slate-400">{notification.time}</p>
          </DropdownMenuItem>
        ))
      ) : (
        <DropdownMenuItem disabled>
          <p className="text-sm text-slate-500 dark:text-slate-400">Không có thông báo mới</p>
        </DropdownMenuItem>
      )}
    </DropdownMenuWrapper>
  );
}


