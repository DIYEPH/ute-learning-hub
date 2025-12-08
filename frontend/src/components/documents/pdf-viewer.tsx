"use client";

import { useState, useCallback, useEffect, useRef } from "react";
import { Document, Page, pdfjs } from "react-pdf";
import {
    ChevronLeft,
    ChevronRight,
    ZoomIn,
    ZoomOut,
    Loader2,
    Maximize2,
    RotateCw,
    Save
} from "lucide-react";
import { Button } from "@/src/components/ui/button";
import { cn } from "@/lib/utils";
import { useDocumentProgress } from "@/src/hooks/use-document-progress";

import "react-pdf/dist/Page/AnnotationLayer.css";
import "react-pdf/dist/Page/TextLayer.css";

// Configure PDF.js worker
pdfjs.GlobalWorkerOptions.workerSrc = `//unpkg.com/pdfjs-dist@${pdfjs.version}/build/pdf.worker.min.mjs`;

interface PdfViewerProps {
    fileUrl: string;
    fileId: string;
    documentId: string;
    title?: string;
    className?: string;
}

export function PdfViewer({
    fileUrl,
    fileId,
    documentId,
    title,
    className
}: PdfViewerProps) {
    const [numPages, setNumPages] = useState<number>(0);
    const [scale, setScale] = useState(1.0);
    const [rotation, setRotation] = useState(0);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [containerWidth, setContainerWidth] = useState<number | null>(null);
    const containerRef = useRef<HTMLDivElement>(null);

    // Track container width for responsive PDF
    useEffect(() => {
        const container = containerRef.current;
        if (!container) return;

        const updateWidth = () => {
            setContainerWidth(container.clientWidth);
        };

        updateWidth();

        const resizeObserver = new ResizeObserver(updateWidth);
        resizeObserver.observe(container);

        return () => resizeObserver.disconnect();
    }, []);

    const {
        currentPage,
        setCurrentPage,
        initialPage,
        isLoading: progressLoading,
        isSaving,
        saveProgress
    } = useDocumentProgress({
        fileId,
        documentId,
        totalPages: numPages,
    });

    // Handle document load
    const onDocumentLoadSuccess = useCallback(({ numPages }: { numPages: number }) => {
        setNumPages(numPages);
        setLoading(false);
        setError(null);
    }, []);

    const onDocumentLoadError = useCallback((err: Error) => {
        console.error("PDF load error:", err);
        setError("Không thể tải file PDF. Vui lòng thử lại.");
        setLoading(false);
    }, []);

    // Navigation
    const goToPrevPage = useCallback(() => {
        setCurrentPage(Math.max(1, currentPage - 1));
    }, [currentPage, setCurrentPage]);

    const goToNextPage = useCallback(() => {
        setCurrentPage(Math.min(numPages, currentPage + 1));
    }, [currentPage, numPages, setCurrentPage]);

    const goToPage = useCallback((page: number) => {
        const validPage = Math.max(1, Math.min(numPages, page));
        setCurrentPage(validPage);
    }, [numPages, setCurrentPage]);

    // Zoom
    const zoomIn = useCallback(() => {
        setScale((prev) => Math.min(prev + 0.25, 3));
    }, []);

    const zoomOut = useCallback(() => {
        setScale((prev) => Math.max(prev - 0.25, 0.5));
    }, []);

    // Rotation
    const rotate = useCallback(() => {
        setRotation((prev) => (prev + 90) % 360);
    }, []);

    // Keyboard navigation
    useEffect(() => {
        const handleKeyDown = (e: KeyboardEvent) => {
            if (e.target instanceof HTMLInputElement || e.target instanceof HTMLTextAreaElement) {
                return;
            }

            switch (e.key) {
                case "ArrowLeft":
                case "PageUp":
                    e.preventDefault();
                    goToPrevPage();
                    break;
                case "ArrowRight":
                case "PageDown":
                case " ":
                    e.preventDefault();
                    goToNextPage();
                    break;
                case "Home":
                    e.preventDefault();
                    goToPage(1);
                    break;
                case "End":
                    e.preventDefault();
                    goToPage(numPages);
                    break;
                case "+":
                case "=":
                    e.preventDefault();
                    zoomIn();
                    break;
                case "-":
                    e.preventDefault();
                    zoomOut();
                    break;
            }
        };

        window.addEventListener("keydown", handleKeyDown);
        return () => window.removeEventListener("keydown", handleKeyDown);
    }, [goToPrevPage, goToNextPage, goToPage, numPages, zoomIn, zoomOut]);

    // Progress percentage
    const progressPercent = numPages > 0 ? Math.round((currentPage / numPages) * 100) : 0;

    if (error) {
        return (
            <div className={cn("flex items-center justify-center h-full bg-slate-100 dark:bg-slate-900", className)}>
                <div className="text-center text-red-500 dark:text-red-400">
                    <p>{error}</p>
                    <Button variant="outline" size="sm" className="mt-4" onClick={() => window.location.reload()}>
                        Thử lại
                    </Button>
                </div>
            </div>
        );
    }

    return (
        <div className={cn("flex flex-col h-full bg-slate-800", className)}>
            {/* Toolbar */}
            <div className="flex items-center justify-between gap-2 px-3 py-2 bg-slate-900 border-b border-slate-700 shrink-0">
                {/* Left: Page navigation */}
                <div className="flex items-center gap-1">
                    <Button
                        variant="ghost"
                        size="icon"
                        onClick={goToPrevPage}
                        disabled={currentPage <= 1 || loading}
                        className="h-8 w-8 text-slate-300 hover:text-white hover:bg-slate-700"
                    >
                        <ChevronLeft className="h-4 w-4" />
                    </Button>

                    <div className="flex items-center gap-1 px-2">
                        <input
                            type="number"
                            value={currentPage}
                            onChange={(e) => goToPage(parseInt(e.target.value, 10) || 1)}
                            className="w-12 h-7 text-center text-sm bg-slate-700 border border-slate-600 rounded text-white focus:outline-none focus:ring-1 focus:ring-sky-500"
                            min={1}
                            max={numPages}
                        />
                        <span className="text-sm text-slate-400">/ {numPages || "..."}</span>
                    </div>

                    <Button
                        variant="ghost"
                        size="icon"
                        onClick={goToNextPage}
                        disabled={currentPage >= numPages || loading}
                        className="h-8 w-8 text-slate-300 hover:text-white hover:bg-slate-700"
                    >
                        <ChevronRight className="h-4 w-4" />
                    </Button>
                </div>

                {/* Center: Progress bar */}
                <div className="hidden sm:flex items-center gap-2 flex-1 max-w-xs mx-4">
                    <div className="flex-1 h-1.5 bg-slate-700 rounded-full overflow-hidden">
                        <div
                            className="h-full bg-sky-500 transition-all duration-300"
                            style={{ width: `${progressPercent}%` }}
                        />
                    </div>
                    <span className="text-xs text-slate-400 w-10 text-right">{progressPercent}%</span>
                </div>

                {/* Right: Zoom, rotate, save */}
                <div className="flex items-center gap-1">
                    {isSaving && (
                        <div className="flex items-center gap-1 text-xs text-slate-400 mr-2">
                            <Loader2 className="h-3 w-3 animate-spin" />
                            <span className="hidden sm:inline">Đang lưu...</span>
                        </div>
                    )}

                    <Button
                        variant="ghost"
                        size="icon"
                        onClick={zoomOut}
                        disabled={scale <= 0.5}
                        className="h-8 w-8 text-slate-300 hover:text-white hover:bg-slate-700"
                        title="Thu nhỏ (-)"
                    >
                        <ZoomOut className="h-4 w-4" />
                    </Button>

                    <span className="text-xs text-slate-400 w-10 text-center">{Math.round(scale * 100)}%</span>

                    <Button
                        variant="ghost"
                        size="icon"
                        onClick={zoomIn}
                        disabled={scale >= 3}
                        className="h-8 w-8 text-slate-300 hover:text-white hover:bg-slate-700"
                        title="Phóng to (+)"
                    >
                        <ZoomIn className="h-4 w-4" />
                    </Button>

                    <div className="w-px h-4 bg-slate-700 mx-1" />

                    <Button
                        variant="ghost"
                        size="icon"
                        onClick={rotate}
                        className="h-8 w-8 text-slate-300 hover:text-white hover:bg-slate-700"
                        title="Xoay trang"
                    >
                        <RotateCw className="h-4 w-4" />
                    </Button>

                    <Button
                        variant="ghost"
                        size="icon"
                        onClick={saveProgress}
                        disabled={isSaving}
                        className="h-8 w-8 text-slate-300 hover:text-white hover:bg-slate-700"
                        title="Lưu tiến trình"
                    >
                        <Save className="h-4 w-4" />
                    </Button>
                </div>
            </div>

            {/* PDF Content */}
            <div
                ref={containerRef}
                className="flex-1 overflow-auto flex justify-center bg-slate-800"
            >
                {(loading || progressLoading) && (
                    <div className="flex items-center justify-center h-full">
                        <Loader2 className="h-8 w-8 animate-spin text-sky-500" />
                    </div>
                )}

                <Document
                    file={fileUrl}
                    onLoadSuccess={onDocumentLoadSuccess}
                    onLoadError={onDocumentLoadError}
                    loading={null}
                    className="py-4"
                >
                    <Page
                        pageNumber={currentPage}
                        scale={scale}
                        rotate={rotation}
                        width={containerWidth ? Math.min(containerWidth - 32, 900) * scale : undefined}
                        className="shadow-lg mx-auto"
                        loading={
                            <div className="flex items-center justify-center h-96">
                                <Loader2 className="h-6 w-6 animate-spin text-sky-500" />
                            </div>
                        }
                    />
                </Document>
            </div>
        </div>
    );
}
