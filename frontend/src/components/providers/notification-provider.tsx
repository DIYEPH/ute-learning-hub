"use client";

import { createContext, useCallback, useContext, useState } from "react";
import { AlertTriangle, CheckCircle, Info, XCircle } from "lucide-react";
import { cn } from "@/lib/utils";

type NotificationType = "success" | "error" | "warning" | "info";

export type AppNotification = {
    id: string;
    type: NotificationType;
    message: string;
    duration?: number;
};

type NotificationContextValue = {
    notify: (type: NotificationType, message: string, duration?: number) => void;
    success: (message: string) => void;
    error: (message: string) => void;
    warning: (message: string) => void;
    info: (message: string) => void;
};

const NotificationContext = createContext<NotificationContextValue | undefined>(undefined);

let idCounter = 0;

export function NotificationProvider({ children }: { children: React.ReactNode }) {
    const [items, setItems] = useState<AppNotification[]>([]);

    const remove = useCallback((id: string) => {
        setItems((prev) => prev.filter((n) => n.id !== id));
    }, []);

    const notify = useCallback(
        (type: NotificationType, message: string, duration = 3000) => {
            const id = `${Date.now()}_${idCounter++}`;

            const item: AppNotification = {
                id,
                type,
                message,
                duration,
            };
            setItems((prev) => [...prev, item]);

            if (duration > 0) {
                setTimeout(() => remove(id), duration);
            }
        },
        [remove]
    );

    const value: NotificationContextValue = {
        notify,
        success: (message) => notify("success", message),
        error: (message) => notify("error", message),
        warning: (message) => notify("warning", message),
        info: (message) => notify("info", message),
    };

    return (
        <NotificationContext.Provider value={value}>
            {children}
            {/* Toast Container - góc trên bên phải */}
            <div className="fixed top-4 right-4 z-[100] flex flex-col gap-2 pointer-events-none">
                {items.map((item) => (
                    <ToastItem key={item.id} notification={item} onRemove={() => remove(item.id)} />
                ))}
            </div>
        </NotificationContext.Provider>
    );
}

export function useNotification() {
    const ctx = useContext(NotificationContext);
    if (!ctx) {
        throw new Error("useNotification must be used within NotificationProvider");
    }
    return ctx;
}

const toastConfig: Record<
    NotificationType,
    {
        icon: typeof CheckCircle;
        bgColor: string;
        iconColor: string;
        textColor: string;
    }
> = {
    success: {
        icon: CheckCircle,
        bgColor: "bg-emerald-500",
        iconColor: "text-white",
        textColor: "text-white",
    },
    error: {
        icon: XCircle,
        bgColor: "bg-red-500",
        iconColor: "text-white",
        textColor: "text-white",
    },
    warning: {
        icon: AlertTriangle,
        bgColor: "bg-amber-500",
        iconColor: "text-white",
        textColor: "text-white",
    },
    info: {
        icon: Info,
        bgColor: "bg-blue-500",
        iconColor: "text-white",
        textColor: "text-white",
    },
};

function ToastItem({
    notification,
    onRemove,
}: {
    notification: AppNotification;
    onRemove: () => void;
}) {
    const config = toastConfig[notification.type];
    const Icon = config.icon;

    return (
        <div
            className={cn(
                "pointer-events-auto flex items-center gap-3 px-4 py-3 rounded-lg shadow-lg",
                "animate-in slide-in-from-right-full fade-in duration-300",
                "cursor-pointer select-none",
                config.bgColor
            )}
            onClick={onRemove}
            style={{
                animation: `slideInRight 0.3s ease-out, fadeOut 0.3s ease-in ${(notification.duration || 3000) - 300}ms forwards`
            }}
        >
            <Icon className={cn("h-5 w-5 flex-shrink-0", config.iconColor)} />
            <span className={cn("text-sm font-medium", config.textColor)}>
                {notification.message}
            </span>
        </div>
    );
}

// CSS for animations - add to global styles or use style tag
const styles = `
@keyframes slideInRight {
    from {
        transform: translateX(100%);
        opacity: 0;
    }
    to {
        transform: translateX(0);
        opacity: 1;
    }
}

@keyframes fadeOut {
    from {
        opacity: 1;
        transform: translateX(0) scale(1);
    }
    to {
        opacity: 0;
        transform: translateX(50%) scale(0.8);
    }
}
`;

// Inject styles
if (typeof document !== 'undefined') {
    const styleEl = document.createElement('style');
    styleEl.textContent = styles;
    document.head.appendChild(styleEl);
}


