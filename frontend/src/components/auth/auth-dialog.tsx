"use client";

import { Dialog, DialogContent, DialogHeader, DialogTitle } from "@/src/components/ui/dialog";

interface AuthDialogProps {
    open: boolean
    onOpenChange: (open: boolean) => void
    title: string
    children: React.ReactNode
}
export default function AuthDialog({open, onOpenChange, title, children}: AuthDialogProps) {
    return (
        <Dialog open={open} onOpenChange={onOpenChange}>
            <DialogContent className="max-w-md rounded-xl p-6">
                <DialogHeader>
                    <DialogTitle>{title}</DialogTitle>
                </DialogHeader>
                <div className="mt-4">{children}</div>
            </DialogContent>
        </Dialog>
    );
}