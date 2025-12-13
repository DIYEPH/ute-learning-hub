"use client";

import { useState } from "react";
import { Loader2, MessageSquare } from "lucide-react";
import { Button } from "@/src/components/ui/button";
import { Label } from "@/src/components/ui/label";
import {
    Dialog,
    DialogContent,
    DialogDescription,
    DialogFooter,
    DialogHeader,
    DialogTitle,
} from "@/src/components/ui/dialog";

interface JoinRequestModalProps {
    open: boolean;
    onClose: () => void;
    conversationName: string;
    onSubmit: (content: string) => Promise<void>;
}

export function JoinRequestModal({
    open,
    onClose,
    onSubmit,
}: JoinRequestModalProps) {
    const [content, setContent] = useState("");
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setError(null);
        setLoading(true);

        try {
            await onSubmit(content);
            setContent("");
            onClose();
        } catch (err: any) {
            setError(err?.message || "Không thể gửi yêu cầu tham gia");
        } finally {
            setLoading(false);
        }
    };

    const handleClose = () => {
        if (!loading) {
            setContent("");
            setError(null);
            onClose();
        }
    };

    return (
        <Dialog open={open} onOpenChange={handleClose}>
            <DialogContent className="sm:max-w-[425px]">
                <DialogHeader>
                    <DialogTitle className="flex items-center gap-2">
                        Xin tham gia nhóm chat
                    </DialogTitle>
                </DialogHeader>

                <form onSubmit={handleSubmit}>
                    {error && (
                        <div className="mb-4 p-3 text-sm text-red-600 dark:text-red-400 bg-red-50 dark:bg-red-950 rounded">
                            {error}
                        </div>
                    )}

                    <div className="mb-4">
                        <textarea
                            id="content"
                            value={content}
                            onChange={(e) => setContent(e.target.value)}
                            placeholder="Xin chào, mình..."
                            disabled={loading}
                            rows={3}
                            className="mt-1 w-full border border-input bg-background text-foreground px-3 py-2 text-sm shadow-sm transition-colors focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-50 resize-none"
                        />
                    </div>

                    <DialogFooter>
                        <Button
                            type="button"
                            variant="outline"
                            onClick={handleClose}
                            disabled={loading}
                        >
                            Hủy
                        </Button>
                        <Button type="submit" disabled={loading}>
                            {loading ? (
                                <>
                                    <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                                    Đang gửi...
                                </>
                            ) : (
                                "Gửi yêu cầu"
                            )}
                        </Button>
                    </DialogFooter>
                </form>
            </DialogContent>
        </Dialog>
    );
}
