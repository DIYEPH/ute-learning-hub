"use client";

import { useState, useCallback, useRef, useEffect } from "react";

let pdfjs: typeof import("react-pdf").pdfjs | null = null;

export function usePdfThumbnail({ width = 400, quality = 0.8, format = "image/jpeg" } = {}) {
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const pdfjsRef = useRef(pdfjs);

    // Initialize pdfjs
    useEffect(() => {
        if (typeof window !== "undefined" && !pdfjsRef.current) {
            import("react-pdf").then(mod => {
                pdfjs = mod.pdfjs;
                pdfjs.GlobalWorkerOptions.workerSrc = new URL(
                    "pdfjs-dist/build/pdf.worker.min.mjs",
                    import.meta.url
                ).toString();
                pdfjsRef.current = pdfjs;
            });
        }
    }, []);

    // Extract thumbnail from PDF
    const extractThumbnail = useCallback(async (pdfFile: File) => {
        if (!pdfFile.type.includes("pdf")) return null;
        if (!pdfjsRef.current) {
            await new Promise(resolve => setTimeout(resolve, 100));
            if (!pdfjsRef.current) return null;
        }
        setLoading(true);
        setError(null);
        try {
            const buffer = await pdfFile.arrayBuffer();
            const pdf = await pdfjsRef.current.getDocument({ data: buffer }).promise;
            const page = await pdf.getPage(1);
            const viewport = page.getViewport({ scale: 1 });
            const scale = width / viewport.width;
            const scaledViewport = page.getViewport({ scale });
            const canvas = document.createElement("canvas");
            canvas.width = Math.floor(scaledViewport.width);
            canvas.height = Math.floor(scaledViewport.height);
            const renderTask = page.render({ canvas, viewport: scaledViewport, intent: "display", annotationMode: 0 });
            await renderTask.promise;
            const blob = await new Promise<Blob | null>(res => canvas.toBlob(res, format, quality));
            if (!blob) return null;
            return new File([blob], pdfFile.name.replace(/\.pdf$/i, "_cover.jpg"), { type: format });
        } catch {
            setError("Không thể tạo ảnh bìa PDF");
            return null;
        } finally { setLoading(false); }
    }, [width, quality, format]);

    return { extractThumbnail, loading, error };
}