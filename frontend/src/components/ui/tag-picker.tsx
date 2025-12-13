"use client";

import * as React from "react";
import { Check, Plus } from "lucide-react";
import { cn } from "@/lib/utils";
import { useDebounce } from "@/src/hooks/use-debounce";
import { Button } from "@/src/components/ui/button";
import { Input } from "@/src/components/ui/input";
import { Chip } from "@/src/components/ui/chip";

export interface TagPickerOption {
    value: string;
    label: string;
}

interface TagPickerProps {
    options: TagPickerOption[];
    selected: string[];
    onChange: (selected: string[]) => void;
    onAddNew?: (tagName: string) => void; // callback khi thêm tag mới
    newTags?: string[]; // danh sách tag mới đã thêm
    onRemoveNewTag?: (tagName: string) => void;
    className?: string;
    disabled?: boolean;
    maxVisibleTags?: number;
    placeholder?: string;
    addNewPlaceholder?: string;
    debounceMs?: number;
    showAddNew?: boolean;
}

export function TagPicker({
    options,
    selected,
    onChange,
    onAddNew,
    newTags = [],
    onRemoveNewTag,
    className,
    disabled = false,
    maxVisibleTags = 8,
    placeholder = "Tìm thẻ...",
    addNewPlaceholder = "Nhập tên tag mới",
    debounceMs = 300,
    showAddNew = true,
}: TagPickerProps) {
    const [inputValue, setInputValue] = React.useState("");
    const debouncedInput = useDebounce(inputValue, debounceMs);

    // Lọc options theo debounced input
    const filteredOptions = React.useMemo(() => {
        if (!debouncedInput) {
            // Không search: hiển thị tag đã chọn + một số tag phổ biến
            const selectedOptions = options.filter((o) => selected.includes(o.value));
            const unselectedOptions = options.filter((o) => !selected.includes(o.value));
            return [...selectedOptions, ...unselectedOptions.slice(0, maxVisibleTags - selectedOptions.length)];
        }
        // Có search: lọc theo tên
        return options.filter((option) =>
            option.label.toLowerCase().includes(debouncedInput.toLowerCase())
        );
    }, [options, selected, debouncedInput, maxVisibleTags]);

    // Kiểm tra tag đã tồn tại chưa (trong options hoặc newTags)
    const isDuplicate = React.useMemo(() => {
        const trimmed = inputValue.trim().toLowerCase();
        if (!trimmed) return false;

        const existsInOptions = options.some(
            (o) => o.label.toLowerCase() === trimmed
        );
        const existsInNewTags = newTags.some(
            (t) => t.toLowerCase() === trimmed
        );
        return existsInOptions || existsInNewTags;
    }, [inputValue, options, newTags]);

    const handleToggle = (value: string) => {
        if (disabled) return;

        if (selected.includes(value)) {
            onChange(selected.filter((v) => v !== value));
        } else {
            onChange([...selected, value]);
        }
    };

    const handleRemove = (value: string) => {
        if (disabled) return;
        onChange(selected.filter((v) => v !== value));
    };

    const handleAddNew = () => {
        const trimmed = inputValue.trim();
        if (!trimmed || isDuplicate || !onAddNew) return;
        onAddNew(trimmed);
        setInputValue("");
    };

    const handleKeyDown = (e: React.KeyboardEvent) => {
        if (e.key === "Enter") {
            e.preventDefault();
            handleAddNew();
        }
    };

    // Lấy labels của selected items
    const selectedLabels = React.useMemo(() => {
        return selected.map((v) => {
            const opt = options.find((o) => o.value === v);
            return opt ? { value: v, label: opt.label } : null;
        }).filter(Boolean) as TagPickerOption[];
    }, [selected, options]);

    return (
        <div className={cn("space-y-3", className)}>
            {/* Available tags to pick */}
            <div className="flex flex-wrap gap-1.5">
                {filteredOptions.length === 0 && !debouncedInput ? (
                    <span className="text-sm text-muted-foreground py-2">
                        Không có thẻ nào
                    </span>
                ) : (
                    filteredOptions.map((option) => {
                        const isSelected = selected.includes(option.value);
                        return (
                            <button
                                key={option.value}
                                type="button"
                                onClick={() => handleToggle(option.value)}
                                disabled={disabled}
                                className={cn(
                                    "inline-flex items-center gap-1 px-2.5 py-1 text-sm border transition-all",
                                    "hover:border-primary hover:bg-accent",
                                    "disabled:opacity-50 disabled:cursor-not-allowed",
                                    isSelected
                                        ? "border-primary bg-primary/10 text-primary"
                                        : "border-input bg-background text-foreground"
                                )}
                            >
                                {isSelected && <Check className="h-3 w-3" />}
                                {option.label}
                            </button>
                        );
                    })
                )}
            </div>

            {/* Input thêm tag mới */}
            {showAddNew && onAddNew && (
                <div className="flex gap-2">
                    <div className="flex-1 relative">
                        <input
                            type="text"
                            value={inputValue}
                            onChange={(e) => setInputValue(e.target.value)}
                            onKeyDown={handleKeyDown}
                            placeholder={addNewPlaceholder}
                            disabled={disabled}
                            className={cn(
                                "w-full px-3 py-2 text-sm border border-input bg-background focus:outline-none focus:ring-1 focus:ring-ring disabled:opacity-50",
                                isDuplicate && inputValue.trim() && "border-amber-500 focus:ring-amber-500"
                            )}
                        />
                        {isDuplicate && inputValue.trim() && (
                            <span className="absolute right-2 top-1/2 -translate-y-1/2 text-xs text-amber-600">
                                Đã tồn tại
                            </span>
                        )}
                    </div>
                    <Button
                        type="button"
                        variant="outline"
                        size="sm"
                        disabled={disabled || !inputValue.trim() || isDuplicate}
                        onClick={handleAddNew}
                    >
                        <Plus className="h-4 w-4 mr-1" />
                        Thêm chủ đề
                    </Button>
                </div>
            )}

            {/* Hiển thị tag mới đã thêm */}
            {newTags.length > 0 && (
                <div className="flex flex-wrap gap-1.5">
                    {newTags.map((tagName, idx) => (
                        <Chip
                            key={`new-${idx}`}
                            variant="new"
                            size="sm"
                            onRemove={onRemoveNewTag ? () => onRemoveNewTag(tagName) : undefined}
                            removeDisabled={disabled}
                        >
                            {tagName}
                        </Chip>
                    ))}
                </div>
            )}

            {/* Hint */}
            {!debouncedInput && options.length > maxVisibleTags && (
                <p className="text-xs text-muted-foreground">
                    Gõ vào ô input để tìm thêm trong {options.length} thẻ
                </p>
            )}

            {debouncedInput && filteredOptions.length === 0 && !isDuplicate && (
                <p className="text-xs text-muted-foreground">
                    Không tìm thấy "{debouncedInput}" - có thể thêm như tag mới
                </p>
            )}
        </div>
    );
}
