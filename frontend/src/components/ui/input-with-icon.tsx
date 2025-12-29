import * as React from "react";
import { cn } from "@/lib/utils";
import { LucideIcon } from "lucide-react";

export interface InputWithIconProps extends React.ComponentProps<"input"> {
  prefixIcon?: LucideIcon;
  suffixIcon?: LucideIcon;
  onPrefixClick?: () => void;
  onSuffixClick?: () => void;
  containerClassName?: string;
}

const InputWithIcon = React.forwardRef<HTMLInputElement, InputWithIconProps>(
  (
    {
      className,
      type,
      prefixIcon: PrefixIcon,
      suffixIcon: SuffixIcon,
      onPrefixClick,
      onSuffixClick,
      containerClassName,
      ...props
    },
    ref
  ) => {
    return (
      <div
        className={cn(
          "relative flex items-center w-full",
          containerClassName
        )}
      >
        {PrefixIcon && (
          <div
            className={cn(
              "absolute left-3 flex items-center justify-center text-muted-foreground",
              onPrefixClick && "cursor-pointer hover:text-foreground"
            )}
            onClick={onPrefixClick}
          >
            <PrefixIcon size={18} />
          </div>
        )}
        <input
          ref={ref}
          type={type}
          data-slot="input"
          className={cn(
            "file:text-foreground placeholder:text-muted-foreground selection:bg-primary selection:text-primary-foreground dark:bg-input/30 border-input h-9 w-full min-w-0  border bg-transparent px-3 py-1 text-base shadow-xs transition-[color,box-shadow] outline-none file:inline-flex file:h-7 file:border-0 file:bg-transparent file:text-sm file:font-medium disabled:pointer-events-none disabled:cursor-not-allowed disabled:opacity-50 md:text-sm",
            "focus-visible:border-ring focus-visible:ring-ring/50 focus-visible:ring-[3px]",
            "aria-invalid:ring-destructive/20 dark:aria-invalid:ring-destructive/40 aria-invalid:border-destructive",
            PrefixIcon && "pl-9",
            SuffixIcon && "pr-9",
            className
          )}
          {...props}
        />
        {SuffixIcon && (
          <div
            className={cn(
              "absolute right-3 flex items-center justify-center text-muted-foreground",
              onSuffixClick && "cursor-pointer hover:text-foreground"
            )}
            onClick={onSuffixClick}
          >
            <SuffixIcon size={18} />
          </div>
        )}
      </div>
    );
  }
);

InputWithIcon.displayName = "InputWithIcon";

export { InputWithIcon };


