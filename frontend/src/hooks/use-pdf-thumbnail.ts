"use client";

import { useState, useCallback } from "react";
import { pdfjs } from "react-pdf";

pdfjs.GlobalWorkerOptions.workerSrc = new URL(
    "pdfjs-dist/build/pdf.worker.min.mjs",
    import.meta.url
).toString();

export function usePdfThumbnail({
    width = 400,
    quality = 0.8,
    format = "image/jpeg",
} = {}) {
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    const extractThumbnail = useCallback(async (pdfFile: File) => {
        if (!pdfFile.type.includes("pdf")) return null;

        setLoading(true);
        setError(null);

        try {
            const buffer = await pdfFile.arrayBuffer();
            const pdf = await pdfjs.getDocument({ data: buffer }).promise;
            const page = await pdf.getPage(1);

            const viewport = page.getViewport({ scale: 1 });
            const scale = width / viewport.width;
            const scaledViewport = page.getViewport({ scale });

            const canvas = document.createElement("canvas");
            canvas.width = Math.floor(scaledViewport.width);
            canvas.height = Math.floor(scaledViewport.height);

            const renderTask = page.render({
                canvas,
                viewport: scaledViewport,
                intent: "display",
                annotationMode: 0,
            });

            await renderTask.promise;

            const blob = await new Promise<Blob | null>((res) =>
                canvas.toBlob(res, format, quality)
            );

            if (!blob) return null;

            return new File(
                [blob],
                pdfFile.name.replace(/\.pdf$/i, "_cover.jpg"),
                { type: format }
            );
        } catch (e) {
            console.error("[react-pdf thumbnail]", e);
            setError("Không thể tạo ảnh bìa PDF");
            return null;
        } finally {
            setLoading(false);
        }
    }, [width, quality, format]);

    return { extractThumbnail, loading, error };
}
