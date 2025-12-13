"use client";

import * as React from "react";
import { X, Check, ChevronsUpDown } from "lucide-react";
import { cn } from "@/lib/utils";
import { Badge } from "@/src/components/ui/badge";
import { Button } from "@/src/components/ui/button";
import {
    Popover,
    PopoverContent,
    PopoverTrigger,
} from "@/src/components/ui/popover";

export interface MultiSelectOption {
    value: string;
    label: string;
}

interface MultiSelectComboboxProps {
    options: MultiSelectOption[];
    selected: string[];
    onChange: (selected: string[]) => void;
    placeholder?: string;
    searchPlaceholder?: string;
    emptyMessage?: string;
    className?: string;
    disabled?: boolean;
    maxDisplayedItems?: number;
}

export function MultiSelectCombobox({
    options,
    selected,
    onChange,
    placeholder = "Chọn...",
    searchPlaceholder = "Tìm kiếm...",
    emptyMessage = "Không tìm thấy",
    className,
    disabled = false,
    maxDisplayedItems = 3,
}: MultiSelectComboboxProps) {
    const [open, setOpen] = React.useState(false);
    const [search, setSearch] = React.useState("");

    const filteredOptions = React.useMemo(() => {
        if (!search) return options;
        return options.filter((option) =>
            option.label.toLowerCase().includes(search.toLowerCase())
        );
    }, [options, search]);

    const selectedOptions = React.useMemo(() => {
        return options.filter((option) => selected.includes(option.value));
    }, [options, selected]);

    const handleSelect = (value: string) => {
        if (selected.includes(value)) {
            onChange(selected.filter((v) => v !== value));
        } else {
            onChange([...selected, value]);
        }
    };

    const handleRemove = (value: string, e: React.MouseEvent) => {
        e.stopPropagation();
        onChange(selected.filter((v) => v !== value));
    };

    const handleClearAll = (e: React.MouseEvent) => {
        e.stopPropagation();
        onChange([]);
    };

    const displayedItems = selectedOptions.slice(0, maxDisplayedItems);
    const remainingCount = selectedOptions.length - maxDisplayedItems;

    return (
        <Popover open={open} onOpenChange={setOpen}>
            <PopoverTrigger asChild>
                <Button
                    variant="outline"
                    role="combobox"
                    aria-expanded={open}
                    disabled={disabled}
                    className={cn(
                        "w-full justify-between min-h-[40px] h-auto py-2",
                        selected.length === 0 && "text-muted-foreground",
                        className
                    )}
                >
                    <div className="flex flex-wrap gap-1 flex-1">
                        {selected.length === 0 ? (
                            <span>{placeholder}</span>
                        ) : (
                            <>
                                {displayedItems.map((option) => (
                                    <Badge
                                        key={option.value}
                                        variant="secondary"
                                        className="px-2 py-0.5 text-xs font-normal"
                                    >
                                        {option.label}
                                        <button
                                            type="button"
                                            className="ml-1 hover:text-destructive"
                                            onClick={(e) => handleRemove(option.value, e)}
                                        >
                                            <X className="h-3 w-3" />
                                        </button>
                                    </Badge>
                                ))}
                                {remainingCount > 0 && (
                                    <Badge variant="secondary" className="px-2 py-0.5 text-xs font-normal">
                                        +{remainingCount}
                                    </Badge>
                                )}
                            </>
                        )}
                    </div>
                    <div className="flex items-center gap-1 ml-2 shrink-0">
                        {selected.length > 0 && (
                            <button
                                type="button"
                                className="p-0.5 hover:text-destructive"
                                onClick={handleClearAll}
                            >
                                <X className="h-4 w-4" />
                            </button>
                        )}
                        <ChevronsUpDown className="h-4 w-4 opacity-50" />
                    </div>
                </Button>
            </PopoverTrigger>
            <PopoverContent className="w-full min-w-[200px] p-0" align="start">
                <div className="p-2 border-b">
                    <input
                        type="text"
                        value={search}
                        onChange={(e) => setSearch(e.target.value)}
                        placeholder={searchPlaceholder}
                        className="w-full px-2 py-1.5 text-sm bg-transparent border outline-none focus:ring-1 focus:ring-ring"
                    />
                </div>
                <div className="max-h-[200px] overflow-y-auto p-1">
                    {filteredOptions.length === 0 ? (
                        <div className="py-4 text-center text-sm text-muted-foreground">
                            {emptyMessage}
                        </div>
                    ) : (
                        filteredOptions.map((option) => {
                            const isSelected = selected.includes(option.value);
                            return (
                                <button
                                    key={option.value}
                                    type="button"
                                    onClick={() => handleSelect(option.value)}
                                    className={cn(
                                        "w-full flex items-center gap-2 px-2 py-1.5 text-sm text-left hover:bg-accent transition-colors",
                                        isSelected && "bg-accent"
                                    )}
                                >
                                    <div
                                        className={cn(
                                            "flex h-4 w-4 items-center justify-center border",
                                            isSelected
                                                ? "bg-primary border-primary text-primary-foreground"
                                                : "border-input"
                                        )}
                                    >
                                        {isSelected && <Check className="h-3 w-3" />}
                                    </div>
                                    <span className="flex-1">{option.label}</span>
                                </button>
                            );
                        })
                    )}
                </div>
                {selected.length > 0 && (
                    <div className="p-2 border-t text-xs text-muted-foreground">
                        Đã chọn {selected.length} mục
                    </div>
                )}
            </PopoverContent>
        </Popover>
    );
}
