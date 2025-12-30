"use client";

import { useCallback, useEffect, useState } from "react";
import {
  Bell,
  Check,
  CheckCheck,
  Loader2,
  FileText,
  MessageSquare,
  Award,
  Calendar,
  Megaphone,
  AlertCircle,
  Users,
  X,
} from "lucide-react";
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
  getApiConversationMyInvitations,
  postApiConversationInvitationsByInvitationIdRespond,
} from "@/src/api";

import type {
  NotificationDto,
  GetApiNotificationResponse,
  GetApiNotificationUnreadCountResponse,
  InvitationDto,
} from "@/src/api/database/types.gen";

import { cn } from "@/lib/utils";
import { useNotification } from "../providers/notification-provider";

/* =========================
   Helpers
========================= */

function unwrapApiResponse<T>(response: unknown): T {
  return (response as { data?: T })?.data ?? (response as T);
}

const getNotificationIcon = (type?: number) => {
  switch (type) {
    case 1:
    case 2:
      return <MessageSquare className="h-4 w-4" />;
    case 3:
      return <FileText className="h-4 w-4" />;
    case 4:
      return <Award className="h-4 w-4" />;
    case 5:
      return <Users className="h-4 w-4" />;
    case 6:
      return <Calendar className="h-4 w-4" />;
    case 7:
      return <Megaphone className="h-4 w-4" />;
    default:
      return <Bell className="h-4 w-4" />;
  }
};

const getPriorityColor = (priority?: number) => {
  switch (priority) {
    case 2:
      return "text-red-600 dark:text-red-400";
    case 1:
      return "text-amber-600 dark:text-amber-400";
    default:
      return "text-muted-foreground";
  }
};

const formatRelativeTime = (date: string | Date) => {
  const now = new Date();
  const then = new Date(date);
  const diffMs = now.getTime() - then.getTime();
  const mins = Math.floor(diffMs / 60000);
  const hours = Math.floor(diffMs / 3600000);
  const days = Math.floor(diffMs / 86400000);

  if (mins < 1) return "Vừa xong";
  if (mins < 60) return `${mins} phút trước`;
  if (hours < 24) return `${hours} giờ trước`;
  if (days < 7) return `${days} ngày trước`;
  return then.toLocaleDateString("vi-VN");
};

/* =========================
   Component
========================= */

export function NotificationMenu() {
  const router = useRouter();
  const { success, error: showError } = useNotification();

  const [notifications, setNotifications] = useState<NotificationDto[]>([]);
  const [invitations, setInvitations] = useState<InvitationDto[]>([]);
  const [unreadCount, setUnreadCount] = useState(0);

  const [loading, setLoading] = useState(false);
  const [markingAllRead, setMarkingAllRead] = useState(false);
  const [respondingInvitation, setRespondingInvitation] = useState<string | null>(null);
  const [isOpen, setIsOpen] = useState(false);

  /* =========================
     Fetch data
  ========================= */

  const fetchUnreadCount = useCallback(async () => {
    try {
      const res = await getApiNotificationUnreadCount();
      const data = unwrapApiResponse<GetApiNotificationUnreadCountResponse>(res);
      setUnreadCount(data?.unreadCount ?? 0);
    } catch (err) {
      console.error("Fetch unread count failed", err);
    }
  }, []);

  const fetchNotifications = useCallback(async () => {
    setLoading(true);
    try {
      const res = await getApiNotification({
        query: { Page: 1, PageSize: 10 },
      });
      const data = unwrapApiResponse<GetApiNotificationResponse>(res);
      setNotifications(data?.items ?? []);
    } catch (err) {
      console.error("Fetch notifications failed", err);
    } finally {
      setLoading(false);
    }
  }, []);

  const fetchInvitations = useCallback(async () => {
    try {
      const res = await getApiConversationMyInvitations({
        query: { PageNumber: 1, PageSize: 5, PendingOnly: true },
      });
      const data = unwrapApiResponse<{ items?: InvitationDto[] }>(res);
      setInvitations(data?.items ?? []);
    } catch (err) {
      console.error("Fetch invitations failed", err);
    }
  }, []);

  /* =========================
     Actions
  ========================= */

  const markAsRead = useCallback(async (notification: NotificationDto) => {
    if (notification.isRead || !notification.id) return;

    try {
      await postApiNotificationByIdMarkAsRead({ path: { id: notification.id } });
      setNotifications((prev) =>
        prev.map((n) =>
          n.id === notification.id ? { ...n, isRead: true } : n
        )
      );
      setUnreadCount((prev) => Math.max(0, prev - 1));
    } catch (err) {
      console.error("Mark as read failed", err);
    }
  }, []);

  const markAllAsRead = useCallback(async () => {
    if (unreadCount === 0) return;

    setMarkingAllRead(true);
    try {
      await postApiNotificationMarkAllAsRead();
      setNotifications((prev) => prev.map((n) => ({ ...n, isRead: true })));
      setUnreadCount(0);
    } catch (err) {
      console.error("Mark all as read failed", err);
    } finally {
      setMarkingAllRead(false);
    }
  }, [unreadCount]);

  const handleNotificationClick = useCallback(
    async (notification: NotificationDto) => {
      await markAsRead(notification);

      if (!notification.link) return;

      if (
        notification.link.startsWith("http://") ||
        notification.link.startsWith("https://")
      ) {
        window.open(notification.link, "_blank", "noopener,noreferrer");
      } else {
        router.push(notification.link);
      }
    },
    [markAsRead, router]
  );

  const respondToInvitation = useCallback(
    async (invitation: InvitationDto, accept: boolean) => {
      if (!invitation.id) return;

      setRespondingInvitation(invitation.id);
      try {
        await postApiConversationInvitationsByInvitationIdRespond({
          path: { invitationId: invitation.id },
          body: { accept, note: null },
        });

        setInvitations((prev) => prev.filter((i) => i.id !== invitation.id));

        if (accept) {
          success("Đã tham gia nhóm thành công!");
          router.push(`/conversations/${invitation.conversationId}`);
        } else {
          success("Đã từ chối lời mời");
        }
      } catch (err) {
        console.error("Respond invitation failed", err);
        showError("Không thể xử lý lời mời");
      } finally {
        setRespondingInvitation(null);
      }
    },
    [router, success, showError]
  );

  /* =========================
     Effects
  ========================= */

  useEffect(() => {
    fetchUnreadCount();
  }, [fetchUnreadCount]);

  useEffect(() => {
    if (isOpen) {
      fetchNotifications();
      fetchInvitations();
    }
  }, [isOpen, fetchNotifications, fetchInvitations]);

  const totalBadge = unreadCount + invitations.length;

  /* =========================
     Render
  ========================= */

  return (
    <DropdownMenuWrapper
      align="end"
      contentClassName="w-96"
      onOpenChange={setIsOpen}
      trigger={
        <Button
          variant="ghost"
          size="sm"
          className="group relative h-9 w-9 rounded-full p-0 hover:bg-muted"
          aria-label="Notifications"
        >
          <Bell size={18} className="text-muted-foreground transition-colors group-hover:text-foreground" />
          {totalBadge > 0 && (
            <span className="absolute -top-1 -right-1 h-5 w-5 rounded-full bg-red-500 text-white text-xs flex items-center justify-center animate-pulse">
              {totalBadge > 9 ? "9+" : totalBadge}
            </span>
          )}
        </Button>
      }
    >
      {/* Header */}
      <div className="flex items-center justify-between px-3 py-2 border-b">
        <p className="text-sm font-semibold">Thông báo</p>
        {unreadCount > 0 && (
          <Button
            variant="ghost"
            size="sm"
            className="h-7 text-xs"
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
          <div className="flex justify-center py-8">
            <Loader2 className="h-6 w-6 animate-spin text-muted-foreground" />
          </div>
        ) : (
          <>
            {/* Invitations */}
            {invitations.length > 0 && (
              <>
                <div className="px-3 py-2 bg-primary/10 border-b">
                  <p className="text-xs font-semibold text-primary">
                    Lời mời tham gia nhóm ({invitations.length})
                  </p>
                </div>

                {invitations.map((inv) => (
                  <div
                    key={inv.id}
                    className="flex gap-3 px-3 py-3 border-b bg-primary/5"
                  >
                    <Users className="h-5 w-5 text-primary mt-1" />

                    <div className="flex-1">
                      <p className="text-sm font-semibold">
                        {inv.conversationName}
                      </p>
                      <p className="text-xs text-muted-foreground">
                        {inv.invitedByName} đã mời bạn
                      </p>

                      <div className="flex gap-2 mt-2">
                        <Button
                          size="sm"
                          onClick={() => respondToInvitation(inv, true)}
                          disabled={respondingInvitation === inv.id}
                        >
                          {respondingInvitation === inv.id ? (
                            <Loader2 className="h-3 w-3 animate-spin" />
                          ) : (
                            <Check className="h-3 w-3 mr-1" />
                          )}
                          Chấp nhận
                        </Button>
                        <Button
                          size="sm"
                          variant="outline"
                          onClick={() => respondToInvitation(inv, false)}
                          disabled={respondingInvitation === inv.id}
                        >
                          <X className="h-3 w-3 mr-1" />
                          Từ chối
                        </Button>
                      </div>
                    </div>

                    <p className="text-xs text-muted-foreground">
                      {inv.createdAt && formatRelativeTime(inv.createdAt)}
                    </p>
                  </div>
                ))}
                <DropdownMenuSeparator />
              </>
            )}

            {/* Notifications */}
            {notifications.length > 0 ? (
              notifications.map((n) => (
                <DropdownMenuItem
                  key={n.id}
                  className={cn(
                    "flex gap-3 px-3 py-3 cursor-pointer",
                    !n.isRead && "bg-primary/5"
                  )}
                  onClick={() => handleNotificationClick(n)}
                >
                  <div className={getPriorityColor(n.notificationPriorityType)}>
                    {getNotificationIcon(n.notificationType)}
                  </div>

                  <div className="flex-1">
                    <p className={cn("text-sm", !n.isRead && "font-semibold")}>
                      {n.title}
                    </p>
                    <p className="text-xs text-muted-foreground line-clamp-2">
                      {n.content}
                    </p>
                    <p className="text-xs text-muted-foreground mt-1">
                      {n.createdAt && formatRelativeTime(n.createdAt)}
                    </p>
                  </div>

                  {n.isRead ? (
                    <Check className="h-4 w-4 text-green-500" />
                  ) : (
                    <span className="h-2 w-2 rounded-full bg-primary mt-2" />
                  )}
                </DropdownMenuItem>
              ))
            ) : invitations.length === 0 ? (
              <div className="flex flex-col items-center py-8 text-muted-foreground">
                <AlertCircle className="h-8 w-8 mb-2" />
                <p className="text-sm">Không có thông báo nào</p>
              </div>
            ) : null}
          </>
        )}
      </div>

      {/* Footer */}
      {(notifications.length > 0 || invitations.length > 0) && (
        <>
          <DropdownMenuSeparator />
          <DropdownMenuItem
            className="justify-center text-primary"
            onClick={() => router.push("/notifications")}
          >
            Xem tất cả thông báo
          </DropdownMenuItem>
        </>
      )}
    </DropdownMenuWrapper>
  );
}
