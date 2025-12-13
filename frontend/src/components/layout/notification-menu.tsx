"use client";

import { useCallback, useEffect, useState } from "react";
import { Bell, Check, CheckCheck, Loader2, FileText, MessageSquare, Award, Calendar, Megaphone, AlertCircle } from "lucide-react";
import { useRouter } from "next/navigation";
import { Button } from "../ui/button";
import { DropdownMenuWrapper } from "../ui/dropdown-menu-wrapper";
import {
  DropdownMenuItem,
  DropdownMenuSeparator,
} from "../ui/dropdown-menu";
import {
  getApiNotification,
  getApiNotificationUnreadCount,
  postApiNotificationByIdMarkAsRead,
  postApiNotificationMarkAllAsRead,
} from "@/src/api/database/sdk.gen";
import type { NotificationDto, GetApiNotificationResponse, GetApiNotificationUnreadCountResponse } from "@/src/api/database/types.gen";
import { cn } from "@/lib/utils";
// NotificationType enum values: System=0, Message=1, Comment=2, Document=3, UserAction=4, Conversation=5, Event=6, AdminNote=7
const getNotificationIcon = (type?: number) => {
  switch (type) {
    case 1: // Message
      return <MessageSquare className="h-4 w-4" />;
    case 2: // Comment
      return <MessageSquare className="h-4 w-4" />;
    case 3: // Document
      return <FileText className="h-4 w-4" />;
    case 4: // UserAction
      return <Award className="h-4 w-4" />;
    case 5: // Conversation
      return <MessageSquare className="h-4 w-4" />;
    case 6: // Event
      return <Calendar className="h-4 w-4" />;
    case 7: // AdminNote
      return <Megaphone className="h-4 w-4" />;
    default: // System=0
      return <Bell className="h-4 w-4" />;
  }
};

// NotificationPriorityType enum values: Low=0, Normal=1, Hight=2
const getPriorityColor = (priority?: number) => {
  switch (priority) {
    case 2: // Hight (High)
      return "text-red-600 dark:text-red-400";
    case 1: // Normal
      return "text-amber-600 dark:text-amber-400";
    default: // Low=0
      return "text-slate-600 dark:text-slate-400";
  }
};

// Format relative time
const formatRelativeTime = (date: string | Date) => {
  const now = new Date();
  const then = new Date(date);
  const diffMs = now.getTime() - then.getTime();
  const diffMins = Math.floor(diffMs / 60000);
  const diffHours = Math.floor(diffMs / 3600000);
  const diffDays = Math.floor(diffMs / 86400000);

  if (diffMins < 1) return "Vừa xong";
  if (diffMins < 60) return `${diffMins} phút trước`;
  if (diffHours < 24) return `${diffHours} giờ trước`;
  if (diffDays < 7) return `${diffDays} ngày trước`;
  return then.toLocaleDateString("vi-VN");
};

export function NotificationMenu() {
  const router = useRouter();
  const [notifications, setNotifications] = useState<NotificationDto[]>([]);
  const [unreadCount, setUnreadCount] = useState(0);
  const [loading, setLoading] = useState(false);
  const [markingAllRead, setMarkingAllRead] = useState(false);
  const [isOpen, setIsOpen] = useState(false);

  // Fetch unread count
  const fetchUnreadCount = useCallback(async () => {
    try {
      const response = await getApiNotificationUnreadCount();
      const data = (response as unknown as { data: GetApiNotificationUnreadCountResponse })?.data || response as GetApiNotificationUnreadCountResponse;
      setUnreadCount(data?.unreadCount ?? 0);
    } catch (error) {
      console.error("Failed to fetch unread count:", error);
    }
  }, []);

  // Fetch notifications
  const fetchNotifications = useCallback(async () => {
    setLoading(true);
    try {
      const response = await getApiNotification({
        query: { Page: 1, PageSize: 10 },
      });
      const data = (response as unknown as { data: GetApiNotificationResponse })?.data || response as GetApiNotificationResponse;
      setNotifications(data?.items ?? []);
    } catch (error) {
      console.error("Failed to fetch notifications:", error);
    } finally {
      setLoading(false);
    }
  }, []);

  // Mark single notification as read
  const markAsRead = useCallback(async (notification: NotificationDto) => {
    if (notification.isRead || !notification.id) return;

    try {
      await postApiNotificationByIdMarkAsRead({ path: { id: notification.id } });
      // Update local state
      setNotifications((prev) =>
        prev.map((n) =>
          n.id === notification.id ? { ...n, isRead: true } : n
        )
      );
      setUnreadCount((prev) => Math.max(0, prev - 1));
    } catch (error) {
      console.error("Failed to mark as read:", error);
    }
  }, []);

  // Mark all as read
  const markAllAsRead = useCallback(async () => {
    if (unreadCount === 0) return;

    setMarkingAllRead(true);
    try {
      await postApiNotificationMarkAllAsRead();
      // Update local state
      setNotifications((prev) => prev.map((n) => ({ ...n, isRead: true })));
      setUnreadCount(0);
    } catch (error) {
      console.error("Failed to mark all as read:", error);
    } finally {
      setMarkingAllRead(false);
    }
  }, [unreadCount]);

  // Handle notification click
  const handleNotificationClick = useCallback(
    async (notification: NotificationDto) => {
      // Mark as read
      await markAsRead(notification);

      // Navigate to link if provided
      if (notification.link) {
        // Check if it's an external URL
        if (notification.link.startsWith("http://") || notification.link.startsWith("https://")) {
          window.open(notification.link, "_blank", "noopener,noreferrer");
        } else {
          // Internal route - use Next.js router
          router.push(notification.link);
        }
      }
    },
    [markAsRead, router]
  );

  // Fetch data when dropdown opens
  useEffect(() => {
    if (isOpen) {
      fetchNotifications();
    }
  }, [isOpen, fetchNotifications]);

  // Fetch unread count on mount
  useEffect(() => {
    fetchUnreadCount();
  }, [fetchUnreadCount]);

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
            <span className="absolute -top-1 -right-1 h-5 w-5 rounded-full bg-red-500 text-white text-xs flex items-center justify-center animate-pulse">
              {unreadCount > 9 ? "9+" : unreadCount}
            </span>
          )}
        </Button>
      }
      align="end"
      contentClassName="w-96"
      onOpenChange={setIsOpen}
    >
      {/* Header */}
      <div className="flex items-center justify-between px-3 py-2 border-b dark:border-slate-700">
        <p className="text-sm font-semibold">Thông báo</p>
        {unreadCount > 0 && (
          <Button
            variant="ghost"
            size="sm"
            className="h-7 text-xs text-primary hover:text-primary/80"
            onClick={(e) => {
              e.preventDefault();
              e.stopPropagation();
              markAllAsRead();
            }}
            disabled={markingAllRead}
          >
            {markingAllRead ? (
              <Loader2 className="h-3 w-3 animate-spin mr-1" />
            ) : (
              <CheckCheck className="h-3 w-3 mr-1" />
            )}
            Đánh dấu tất cả đã đọc
          </Button>
        )}
      </div>

      {/* Content */}
      <div className="max-h-[400px] overflow-y-auto">
        {loading ? (
          <div className="flex items-center justify-center py-8">
            <Loader2 className="h-6 w-6 animate-spin text-slate-400" />
          </div>
        ) : notifications.length > 0 ? (
          notifications.map((notification) => (
            <DropdownMenuItem
              key={notification.id}
              className={cn(
                "flex items-start gap-3 px-3 py-3 cursor-pointer",
                !notification.isRead && "bg-primary/5 dark:bg-primary/10"
              )}
              onClick={() => handleNotificationClick(notification)}
            >
              {/* Icon */}
              <div
                className={cn(
                  "flex-shrink-0 mt-0.5",
                  getPriorityColor(notification.notificationPriorityType)
                )}
              >
                {getNotificationIcon(notification.notificationType)}
              </div>

              {/* Content */}
              <div className="flex-1 min-w-0">
                <p
                  className={cn(
                    "text-sm line-clamp-1",
                    !notification.isRead
                      ? "font-semibold text-slate-900 dark:text-white"
                      : "font-medium text-slate-700 dark:text-slate-300"
                  )}
                >
                  {notification.title}
                </p>
                <p className="text-xs text-slate-500 dark:text-slate-400 line-clamp-2 mt-0.5">
                  {notification.content}
                </p>
                <p className="text-xs text-slate-400 dark:text-slate-500 mt-1">
                  {notification.createdAt && formatRelativeTime(notification.createdAt)}
                </p>
              </div>

              {/* Read indicator */}
              {notification.isRead ? (
                <Check className="h-4 w-4 text-green-500 flex-shrink-0" />
              ) : (
                <div className="h-2 w-2 rounded-full bg-primary flex-shrink-0 mt-1.5" />
              )}
            </DropdownMenuItem>
          ))
        ) : (
          <div className="flex flex-col items-center justify-center py-8 text-slate-400">
            <AlertCircle className="h-8 w-8 mb-2" />
            <p className="text-sm">Không có thông báo nào</p>
          </div>
        )}
      </div>

      {/* Footer */}
      {notifications.length > 0 && (
        <>
          <DropdownMenuSeparator />
          <DropdownMenuItem
            className="flex items-center justify-center py-2 text-sm text-primary cursor-pointer"
            onClick={() => router.push("/notifications")}
          >
            Xem tất cả thông báo
          </DropdownMenuItem>
        </>
      )}
    </DropdownMenuWrapper>
  );
}
