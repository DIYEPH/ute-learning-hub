"use client";

import { useState, useCallback, useEffect, useRef } from "react";
import { putApiDocumentFilesByFileIdProgress, getApiDocumentFilesByFileIdProgress } from "@/src/api";

interface UseDocumentProgressOptions {
    fileId: string;
    documentId: string;
    totalPages?: number;
    autoSaveIntervalMs?: number;
}

interface UseDocumentProgressReturn {
    currentPage: number;
    setCurrentPage: (page: number) => void;
    initialPage: number;
    isLoading: boolean;
    isSaving: boolean;
    saveProgress: () => Promise<void>;
}

function isAuthenticated(): boolean {
    if (typeof window === "undefined") return false;
    return !!localStorage.getItem("access_token");
}

export function useDocumentProgress({
    fileId,
    documentId,
    totalPages,
    autoSaveIntervalMs = 30000,
}: UseDocumentProgressOptions): UseDocumentProgressReturn {
    const [currentPage, setCurrentPageState] = useState(1);
    const [initialPage, setInitialPage] = useState(1);
    const [isLoading, setIsLoading] = useState(true);
    const [isSaving, setIsSaving] = useState(false);
    const lastSavedPageRef = useRef(0);
    const saveTimeoutRef = useRef<NodeJS.Timeout | null>(null);

    // Load progress
    useEffect(() => {
        const loadProgress = async () => {
            if (!isAuthenticated()) { setIsLoading(false); return; }
            try {
                setIsLoading(true);
                const response = await getApiDocumentFilesByFileIdProgress({ path: { fileId } });
                const data = response.data as { lastPage?: number } | undefined;
                if (data?.lastPage && data.lastPage > 0) {
                    setCurrentPageState(data.lastPage);
                    setInitialPage(data.lastPage);
                    lastSavedPageRef.current = data.lastPage;
                }
            } catch { }
            finally { setIsLoading(false); }
        };
        if (fileId) void loadProgress();
    }, [fileId]);

    // Save progress
    const saveProgress = useCallback(async () => {
        if (!isAuthenticated() || currentPage === lastSavedPageRef.current) return;
        try {
            setIsSaving(true);
            await putApiDocumentFilesByFileIdProgress({ path: { fileId }, body: { lastPage: currentPage } });
            lastSavedPageRef.current = currentPage;
        } catch { }
        finally { setIsSaving(false); }
    }, [fileId, currentPage]);

    const setCurrentPage = useCallback((page: number) => {
        setCurrentPageState(page);
        if (saveTimeoutRef.current) clearTimeout(saveTimeoutRef.current);
        saveTimeoutRef.current = setTimeout(() => { void saveProgress(); }, 2000);
    }, [saveProgress]);

    // Auto save interval
    useEffect(() => {
        const interval = setInterval(() => { void saveProgress(); }, autoSaveIntervalMs);
        return () => clearInterval(interval);
    }, [autoSaveIntervalMs, saveProgress]);

    // Save on unload
    useEffect(() => {
        const handleBeforeUnload = () => {
            const url = `${process.env.NEXT_PUBLIC_API_URL}/api/Document/files/${fileId}/progress`;
            const token = typeof window !== "undefined" ? localStorage.getItem("access_token") : null;
            if (currentPage !== lastSavedPageRef.current && token) {
                navigator.sendBeacon(
                    url,
                    new Blob([JSON.stringify({ documentFileId: fileId, lastPage: currentPage })], { type: "application/json" })
                );
            }
        };
        window.addEventListener("beforeunload", handleBeforeUnload);
        return () => {
            window.removeEventListener("beforeunload", handleBeforeUnload);
            void saveProgress();
        };
    }, [fileId, currentPage, saveProgress]);

    // Cleanup timeout
    useEffect(() => {
        return () => { if (saveTimeoutRef.current) clearTimeout(saveTimeoutRef.current); };
    }, []);

    return { currentPage, setCurrentPage, initialPage, isLoading, isSaving, saveProgress };
}