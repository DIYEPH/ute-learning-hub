"use client";

import { useState, useCallback, useEffect, useRef } from "react";
import dynamic from "next/dynamic";
import { ChevronLeft, ChevronRight, ZoomIn, ZoomOut, Loader2, RotateCw, Save, Download } from "lucide-react";
import { Button } from "@/src/components/ui/button";
import { cn } from "@/lib/utils";
import { useDocumentProgress } from "@/src/hooks/use-document-progress";

const Document = dynamic(() => import("react-pdf").then(mod => mod.Document), { ssr: false });
const Page = dynamic(() => import("react-pdf").then(mod => mod.Page), { ssr: false });

if (typeof window !== "undefined") {
    import("react-pdf").then(mod => {
        mod.pdfjs.GlobalWorkerOptions.workerSrc = new URL("pdfjs-dist/build/pdf.worker.min.mjs", import.meta.url).toString();
    });
}

import "react-pdf/dist/Page/AnnotationLayer.css";
import "react-pdf/dist/Page/TextLayer.css";

interface PdfViewerProps {
    fileUrl: string;
    fileId: string;
    documentId: string;
    title?: string;
    className?: string;
}

export function PdfViewer({ fileUrl, fileId, documentId, title, className }: PdfViewerProps) {
    const [numPages, setNumPages] = useState<number>(0);
    const [scale, setScale] = useState(1.0);
    const [rotation, setRotation] = useState(0);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [containerWidth, setContainerWidth] = useState<number | null>(null);
    const containerRef = useRef<HTMLDivElement>(null);

    useEffect(() => {
        const container = containerRef.current;
        if (!container) return;
        const updateWidth = () => setContainerWidth(container.clientWidth);
        updateWidth();
        const resizeObserver = new ResizeObserver(updateWidth);
        resizeObserver.observe(container);
        return () => resizeObserver.disconnect();
    }, []);

    const { currentPage, setCurrentPage, isLoading: progressLoading, isSaving, saveProgress } = useDocumentProgress({
        fileId, documentId, totalPages: numPages
    });

    const onDocumentLoadSuccess = useCallback(({ numPages }: { numPages: number }) => {
        setNumPages(numPages);
        setLoading(false);
        setError(null);
    }, []);

    const onDocumentLoadError = useCallback((err: Error) => {
        setError("Không thể tải file PDF. Vui lòng thử lại.");
        setLoading(false);
    }, []);

    const goToPrevPage = useCallback(() => setCurrentPage(Math.max(1, currentPage - 1)), [currentPage, setCurrentPage]);
    const goToNextPage = useCallback(() => setCurrentPage(Math.min(numPages, currentPage + 1)), [currentPage, numPages, setCurrentPage]);
    const goToPage = useCallback((page: number) => setCurrentPage(Math.max(1, Math.min(numPages, page))), [numPages, setCurrentPage]);
    const zoomIn = useCallback(() => setScale(prev => Math.min(prev + 0.25, 3)), []);
    const zoomOut = useCallback(() => setScale(prev => Math.max(prev - 0.25, 0.5)), []);
    const rotate = useCallback(() => setRotation(prev => (prev + 90) % 360), []);

    useEffect(() => {
        const handleKeyDown = (e: KeyboardEvent) => {
            if (e.target instanceof HTMLInputElement || e.target instanceof HTMLTextAreaElement) return;
            switch (e.key) {
                case "ArrowLeft": case "PageUp": e.preventDefault(); goToPrevPage(); break;
                case "ArrowRight": case "PageDown": case " ": e.preventDefault(); goToNextPage(); break;
                case "Home": e.preventDefault(); goToPage(1); break;
                case "End": e.preventDefault(); goToPage(numPages); break;
                case "+": case "=": e.preventDefault(); zoomIn(); break;
                case "-": e.preventDefault(); zoomOut(); break;
            }
        };
        window.addEventListener("keydown", handleKeyDown);
        return () => window.removeEventListener("keydown", handleKeyDown);
    }, [goToPrevPage, goToNextPage, goToPage, numPages, zoomIn, zoomOut]);

    const progressPercent = numPages > 0 ? Math.round((currentPage / numPages) * 100) : 0;

    if (error) {
        return (
            <div className={cn("flex items-center justify-center h-full bg-muted", className)}>
                <div className="text-center text-destructive">
                    <p>{error}</p>
                    <Button variant="outline" size="sm" className="mt-4" onClick={() => window.location.reload()}>Thử lại</Button>
                </div>
            </div>
        );
    }

    return (
        <div className={cn("flex flex-col h-full bg-card dark:bg-background", className)}>
            <div className="flex flex-wrap items-center justify-between gap-1 md:gap-2 px-2 md:px-3 py-2 bg-muted border-b border-border shrink-0">
                <div className="flex items-center gap-0.5 md:gap-1">
                    <Button
                        variant="ghost"
                        size="icon"
                        onClick={goToPrevPage}
                        disabled={currentPage <= 1 || loading}
                        className="h-7 w-7 md:h-8 md:w-8 text-muted-foreground hover:text-foreground hover:bg-muted"
                    >
                        <ChevronLeft className="h-4 w-4" />
                    </Button>
                    <div className="flex items-center gap-1 px-1 md:px-2">
                        <input
                            type="number"
                            value={currentPage}
                            onChange={e => goToPage(parseInt(e.target.value, 10) || 1)}
                            className="w-10 md:w-12 h-6 md:h-7 text-center text-xs md:text-sm bg-secondary border border-border rounded text-foreground focus:outline-none focus:ring-1 focus:ring-ring"
                            min={1}
                            max={numPages}
                        />
                        <span className="text-xs md:text-sm text-muted-foreground">/ {numPages || "..."}</span>
                    </div>
                    <Button
                        variant="ghost"
                        size="icon"
                        onClick={goToNextPage}
                        disabled={currentPage >= numPages || loading}
                        className="h-7 w-7 md:h-8 md:w-8 text-muted-foreground hover:text-foreground hover:bg-muted"
                    >
                        <ChevronRight className="h-4 w-4" />
                    </Button>
                </div>
                <div className="hidden md:flex items-center gap-2 flex-1 max-w-xs mx-4">
                    <div className="flex-1 h-1.5 bg-secondary rounded-full overflow-hidden">
                        <div className="h-full bg-primary transition-all duration-300" style={{ width: `${progressPercent}%` }} />
                    </div>
                    <span className="text-xs text-muted-foreground w-10 text-right">{progressPercent}%</span>
                </div>
                <div className="flex items-center gap-0.5 md:gap-1">
                    {isSaving && (
                        <div className="flex items-center gap-1 text-xs text-muted-foreground mr-1 md:mr-2">
                            <Loader2 className="h-3 w-3 animate-spin" />
                            <span className="hidden md:inline">Đang lưu...</span>
                        </div>
                    )}
                    <Button variant="ghost" size="icon" onClick={zoomOut} disabled={scale <= 0.5} className="h-7 w-7 md:h-8 md:w-8 text-muted-foreground hover:text-foreground hover:bg-muted" title="Thu nhỏ (-)">
                        <ZoomOut className="h-4 w-4" />
                    </Button>
                    <span className="hidden sm:inline text-xs text-muted-foreground w-10 text-center">{Math.round(scale * 100)}%</span>
                    <Button variant="ghost" size="icon" onClick={zoomIn} disabled={scale >= 3} className="h-7 w-7 md:h-8 md:w-8 text-muted-foreground hover:text-foreground hover:bg-muted" title="Phóng to (+)">
                        <ZoomIn className="h-4 w-4" />
                    </Button>
                    <div className="hidden sm:block w-px h-4 bg-border mx-1" />
                    <Button variant="ghost" size="icon" onClick={rotate} className="h-7 w-7 md:h-8 md:w-8 text-muted-foreground hover:text-foreground hover:bg-muted" title="Xoay trang">
                        <RotateCw className="h-4 w-4" />
                    </Button>
                    <Button variant="ghost" size="icon" onClick={saveProgress} disabled={isSaving} className="h-7 w-7 md:h-8 md:w-8 text-muted-foreground hover:text-foreground hover:bg-muted" title="Lưu tiến trình">
                        <Save className="h-4 w-4" />
                    </Button>
                    <a href={fileUrl} download={title || "document"} target="_blank" rel="noopener noreferrer">
                        <Button variant="ghost" size="icon" className="h-7 w-7 md:h-8 md:w-8 text-muted-foreground hover:text-foreground hover:bg-muted" title="Tải xuống">
                            <Download className="h-4 w-4" />
                        </Button>
                    </a>
                </div>
            </div>
            <div ref={containerRef} className="flex-1 overflow-auto flex justify-center bg-card dark:bg-background">
                {(loading || progressLoading) && (
                    <div className="flex items-center justify-center h-full">
                        <Loader2 className="h-8 w-8 animate-spin text-primary" />
                    </div>
                )}
                <Document file={fileUrl} onLoadSuccess={onDocumentLoadSuccess} onLoadError={onDocumentLoadError} loading={null} className="py-4">
                    <Page
                        pageNumber={currentPage}
                        scale={scale}
                        rotate={rotation}
                        width={containerWidth ? Math.min(containerWidth - 16, 900) * scale : undefined}
                        className="shadow-lg mx-auto"
                        loading={
                            <div className="flex items-center justify-center h-96">
                                <Loader2 className="h-6 w-6 animate-spin text-primary" />
                            </div>
                        }
                    />
                </Document>
            </div>
        </div>
    );
}