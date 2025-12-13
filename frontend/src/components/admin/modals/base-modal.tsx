"use client";

import { ReactNode } from "react";
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from "@/src/components/ui/dialog";
import { cn } from "@/lib/utils";

export interface BaseModalProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  title: string;
  description?: string;
  children: ReactNode;
  footer?: ReactNode;
  size?: "sm" | "md" | "lg" | "xl";
  showCloseButton?: boolean;
}

export function BaseModal({open, onOpenChange, title, description, children, footer, size = "md", showCloseButton = true}: BaseModalProps) {
  const sizeClasses = {
    sm: "sm:max-w-md",
    md: "sm:max-w-lg",
    lg: "sm:max-w-2xl",
    xl: "sm:max-w-4xl",
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent
        className={cn(sizeClasses[size], "max-h-[90vh] flex flex-col")}
        showCloseButton={showCloseButton}
      >
        <DialogHeader className="flex-shrink-0">
          <DialogTitle>{title}</DialogTitle>
          {description && <DialogDescription>{description}</DialogDescription>}
        </DialogHeader>
        <div className="py-4 overflow-y-auto flex-1 min-h-0">{children}</div>
        {footer && <DialogFooter className="flex-shrink-0">{footer}</DialogFooter>}
      </DialogContent>
    </Dialog>
  );
}
