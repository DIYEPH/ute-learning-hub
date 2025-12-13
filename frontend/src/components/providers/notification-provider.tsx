"use client";

import { createContext, useCallback, useContext, useEffect, useState } from "react";
import { X } from "lucide-react";

type NotificationType = "success" | "error" | "info";

export type AppNotification = {
    id: string;
    type: NotificationType;
    message: string;
};

type NotificationContextValue = {
    notify: (type: NotificationType, message: string) => void;
    success: (message: string) => void;
    error: (message: string) => void;
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
        (type: NotificationType, message: string) => {
            const id = `${Date.now()}_${idCounter++}`;
            const item: AppNotification = { id, type, message };
            setItems((prev) => [...prev, item]);

            setTimeout(() => remove(id), 4000);
        },
        [remove]
    );

    const value: NotificationContextValue = {
        notify,
        success: (message) => notify("success", message),
        error: (message) => notify("error", message),
        info: (message) => notify("info", message),
    };

    return (
        <NotificationContext.Provider value={value}>
            {children}
            <div className="fixed inset-0 pointer-events-none z-[60] flex flex-col items-end p-4 gap-2">
                {items.map((item) => (
                    <NotificationItem key={item.id} item={item} onClose={() => remove(item.id)} />
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

function NotificationItem({
    item,
    onClose,
}: {
    item: AppNotification;
    onClose: () => void;
}) {
    const [visible, setVisible] = useState(true);

    useEffect(() => {
        setVisible(true);
    }, [item.id]);

    const base =
        "pointer-events-auto flex items-start gap-2  border px-3 py-2 text-sm shadow-md bg-white dark:bg-slate-900";

    const colorByType: Record<NotificationType, string> = {
        success: "border-emerald-500 text-emerald-800 dark:text-emerald-300",
        error: "border-red-500 text-red-800 dark:text-red-300",
        info: "border-sky-500 text-sky-800 dark:text-sky-300",
    };

    if (!visible) return null;

    return (
        <div className={`${base} ${colorByType[item.type]}`}>
            <div className="flex-1">{item.message}</div>
            <button
                type="button"
                onClick={() => {
                    setVisible(false);
                    onClose();
                }}
                className="ml-2 text-xs opacity-70 hover:opacity-100"
            >
                <X className="h-3 w-3" />
            </button>
        </div>
    );
}


