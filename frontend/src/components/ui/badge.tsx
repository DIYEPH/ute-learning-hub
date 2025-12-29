import * as React from "react"
import { Slot } from "@radix-ui/react-slot"
import { cva, type VariantProps } from "class-variance-authority"

import { cn } from "@/lib/utils"

const badgeVariants = cva(
  "inline-flex items-center justify-center gap-1.5 rounded-md px-2.5 py-1 text-xs font-medium",
  {
    variants: {
      variant: {
        default:
          "bg-primary text-primary-foreground",
        secondary:
          "bg-muted text-muted-foreground",
        destructive:
          "bg-destructive/15 text-destructive",
        outline:
          "bg-transparent text-foreground",
        success:
          "bg-emerald-500/15 text-emerald-600 dark:text-emerald-400",
        warning:
          "bg-amber-500/15 text-amber-600 dark:text-amber-400",
        info:
          "bg-blue-500/15 text-blue-600 dark:text-blue-400",
      },
    },
    defaultVariants: {
      variant: "default",
    },
  }
)

function Badge({
  className,
  variant,
  asChild = false,
  ...props
}: React.ComponentProps<"span"> &
  VariantProps<typeof badgeVariants> & { asChild?: boolean }) {
  const Comp = asChild ? Slot : "span"

  return (
    <Comp
      data-slot="badge"
      className={cn(badgeVariants({ variant }), className)}
      {...props}
    />
  )
}

export { Badge, badgeVariants }

