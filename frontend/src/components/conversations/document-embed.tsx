"use client";

import { useEffect, useState } from "react";
import { Loader2, FileText } from "lucide-react";
import { getApiDocumentById } from "@/src/api";
import type { DocumentDetailDto } from "@/src/api/database/types.gen";
import { DocumentCard } from "@/src/components/documents/document-card";
import { cn } from "@/lib/utils";

interface DocumentEmbedProps {
    documentId: string;
    className?: string;
}

export function DocumentEmbed({ documentId, className }: DocumentEmbedProps) {
    const [document, setDocument] = useState<DocumentDetailDto | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(false);

    useEffect(() => {
        let cancelled = false;
        (async () => {
            try {
                setLoading(true);
                const res = await getApiDocumentById({ path: { id: documentId } });
                const doc = (res as any)?.data || res as DocumentDetailDto;
                if (!cancelled && doc) setDocument(doc);
            } catch {
                if (!cancelled) setError(true);
            } finally {
                if (!cancelled) setLoading(false);
            }
        })();
        return () => { cancelled = true; };
    }, [documentId]);

    if (loading) {
        return (
            <div className={cn("flex items-center gap-2 p-3 bg-muted/50 rounded-lg border border-border/50", className)}>
                <Loader2 className="h-4 w-4 animate-spin text-muted-foreground" />
                <span className="text-sm text-muted-foreground">Đang tải tài liệu...</span>
            </div>
        );
    }

    if (error || !document) {
        return (
            <div className={cn("flex items-center gap-2 p-3 bg-muted/50 rounded-lg border border-border/50", className)}>
                <FileText className="h-4 w-4 text-muted-foreground" />
                <span className="text-sm text-muted-foreground">Không thể tải tài liệu</span>
            </div>
        );
    }

    return (
        <div className={cn("max-w-xs", className)} onClick={e => e.stopPropagation()}>
            <DocumentCard
                id={document.id}
                title={document.documentName || "Tài liệu"}
                subjectName={document.subject?.subjectName}
                thumbnailFileId={document.coverFileId}
                fileCount={document.files?.length}
                usefulCount={document.usefulCount}
                notUsefulCount={document.notUsefulCount}
                totalViewCount={document.totalViewCount}
            />
        </div>
    );
}

const DOC_URL_PATTERN = /(?:https?:\/\/[^\s/]+)?\/documents\/([a-f0-9-]{36})(?:\?[^\s]*)?/gi;

export function parseDocumentEmbeds(content: string): React.ReactNode[] {
    const parts: React.ReactNode[] = [];
    let lastIndex = 0;
    let keyIndex = 0;

    DOC_URL_PATTERN.lastIndex = 0;
    let match: RegExpExecArray | null;

    while ((match = DOC_URL_PATTERN.exec(content)) !== null) {
        if (match.index > lastIndex) parts.push(content.slice(lastIndex, match.index));
        parts.push(<DocumentEmbed key={`doc-${keyIndex++}`} documentId={match[1]} className="my-2" />);
        lastIndex = match.index + match[0].length;
    }

    if (lastIndex < content.length) parts.push(content.slice(lastIndex));
    return parts.length > 0 ? parts : [content];
}