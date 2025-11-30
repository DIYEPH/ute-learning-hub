"use client";

import { ReactNode } from "react";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuTrigger,
} from "./dropdown-menu";
import { useDropdown } from "@/src/hooks/use-dropdown";

export interface DropdownMenuWrapperProps {
  trigger: ReactNode;
  children: ReactNode;
  align?: "start" | "end" | "center";
  side?: "top" | "right" | "bottom" | "left";
  sideOffset?: number;
  className?: string;
  contentClassName?: string;
  defaultOpen?: boolean;
  onOpenChange?: (open: boolean) => void;
}

/**
 * Wrapper component cho DropdownMenu với useDropdown hook
 * Sử dụng cho các dropdown như notifications, events, search, etc.
 */
export function DropdownMenuWrapper({
  trigger,
  children,
  align = "end",
  side = "bottom",
  sideOffset = 4,
  className,
  contentClassName,
  defaultOpen = false,
  onOpenChange,
}: DropdownMenuWrapperProps) {
  const dropdown = useDropdown(defaultOpen);

  const handleOpenChange = (open: boolean) => {
    dropdown.setOpen(open);
    onOpenChange?.(open);
  };

  return (
    <DropdownMenu open={dropdown.open} onOpenChange={handleOpenChange}>
      <div className={className}>
        <DropdownMenuTrigger asChild>
          {trigger}
        </DropdownMenuTrigger>
      </div>
      <DropdownMenuContent
        align={align}
        side={side}
        sideOffset={sideOffset}
        className={contentClassName}
      >
        {children}
      </DropdownMenuContent>
    </DropdownMenu>
  );
}

