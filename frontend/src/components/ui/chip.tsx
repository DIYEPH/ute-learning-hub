"use client";

import * as React from "react";
import { X } from "lucide-react";
import { cn } from "@/lib/utils";
import { cva, type VariantProps } from "class-variance-authority";

const chipVariants = cva(
    "inline-flex items-center gap-1 px-2 py-1 text-sm border transition-all",
    {
        variants: {
            variant: {
                default: "border-input bg-background text-foreground",
                selected: "border-primary bg-primary/10 text-primary",
                new: "bg-emerald-100 text-emerald-700 dark:bg-emerald-900 dark:text-emerald-300 border-emerald-300 dark:border-emerald-700",
                success: "bg-green-100 text-green-700 dark:bg-green-900 dark:text-green-300 border-green-300",
                warning: "bg-amber-100 text-amber-700 dark:bg-amber-900 dark:text-amber-300 border-amber-300",
            },
            size: {
                default: "text-sm",
                sm: "text-xs py-0.5 px-1.5",
            },
        },
        defaultVariants: {
            variant: "default",
            size: "default",
        },
    }
);

export interface ChipProps
    extends React.HTMLAttributes<HTMLSpanElement>,
    VariantProps<typeof chipVariants> {
    onRemove?: () => void;
    removeDisabled?: boolean;
    icon?: React.ReactNode;
    suffix?: React.ReactNode;
}

const Chip = React.forwardRef<HTMLSpanElement, ChipProps>(
    (
        {
            className,
            variant,
            size,
            onRemove,
            removeDisabled = false,
            icon,
            suffix,
            children,
            ...props
        },
        ref
    ) => {
        return (
            <span
                ref={ref}
                className={cn(chipVariants({ variant, size }), className)}
                {...props}
            >
                {icon}
                {children}
                {suffix}
                {onRemove && (
                    <button
                        type="button"
                        onClick={onRemove}
                        disabled={removeDisabled}
                        className="ml-0.5 hover:opacity-70 disabled:opacity-50 disabled:cursor-not-allowed"
                    >
                        <X className="h-3 w-3" />
                    </button>
                )}
            </span>
        );
    }
);

Chip.displayName = "Chip";

export { Chip, chipVariants };
