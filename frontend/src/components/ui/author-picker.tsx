"use client";

import * as React from "react";
import { Check, Plus, Info } from "lucide-react";
import { cn } from "@/lib/utils";
import { useDebounce } from "@/src/hooks/use-debounce";
import { Button } from "@/src/components/ui/button";
import { Input } from "@/src/components/ui/input";
import { Label } from "@/src/components/ui/label";
import { Chip } from "@/src/components/ui/chip";
import { Textarea } from "@/src/components/ui/textarea";

export interface AuthorPickerOption {
    value: string;
    label: string;
    description?: string | null;
}

export interface NewAuthor {
    fullName?: string;
    description?: string | null;
}

interface AuthorPickerProps {
    options: AuthorPickerOption[];
    selected: string[];
    onChange: (selected: string[]) => void;
    onAddNew?: (author: NewAuthor) => void;
    newAuthors?: NewAuthor[];
    onRemoveNewAuthor?: (index: number) => void;
    className?: string;
    disabled?: boolean;
    placeholder?: string;
    debounceMs?: number;
}

export function AuthorPicker({
    options,
    selected,
    onChange,
    onAddNew,
    newAuthors = [],
    onRemoveNewAuthor,
    className,
    disabled = false,
    placeholder = "Tìm tác giả...",
    debounceMs = 300,
}: AuthorPickerProps) {
    const [inputValue, setInputValue] = React.useState("");
    const [isOpen, setIsOpen] = React.useState(false);
    const [showNewForm, setShowNewForm] = React.useState(false);
    const [newName, setNewName] = React.useState("");
    const [newDescription, setNewDescription] = React.useState("");
    const [previewAuthor, setPreviewAuthor] = React.useState<AuthorPickerOption | null>(null);

    const inputRef = React.useRef<HTMLInputElement>(null);
    const dropdownRef = React.useRef<HTMLDivElement>(null);
    const debouncedInput = useDebounce(inputValue, debounceMs);

    // Lọc options theo debounced input
    const filteredOptions = React.useMemo(() => {
        if (!debouncedInput) return [];
        return options.filter((option) =>
            option.label.toLowerCase().includes(debouncedInput.toLowerCase())
        );
    }, [options, debouncedInput]);

    // Close dropdown when clicking outside
    React.useEffect(() => {
        const handleClickOutside = (event: MouseEvent) => {
            if (
                dropdownRef.current &&
                !dropdownRef.current.contains(event.target as Node) &&
                inputRef.current &&
                !inputRef.current.contains(event.target as Node)
            ) {
                setIsOpen(false);
            }
        };
        document.addEventListener("mousedown", handleClickOutside);
        return () => document.removeEventListener("mousedown", handleClickOutside);
    }, []);

    const handleSelect = (value: string) => {
        if (disabled) return;
        if (!selected.includes(value)) {
            onChange([...selected, value]);
        }
        setInputValue("");
        setIsOpen(false);
        setPreviewAuthor(null);
    };

    const handleRemove = (value: string) => {
        if (disabled) return;
        onChange(selected.filter((v) => v !== value));
    };

    const handleAddNewAuthor = () => {
        if (!newName.trim() || !onAddNew) return;
        onAddNew({
            fullName: newName.trim(),
            description: newDescription.trim() || undefined,
        });
        setNewName("");
        setNewDescription("");
        setShowNewForm(false);
    };

    // Lấy labels của selected
    const selectedLabels = React.useMemo(() => {
        return selected.map((v) => {
            const opt = options.find((o) => o.value === v);
            return opt ? opt : null;
        }).filter(Boolean) as AuthorPickerOption[];
    }, [selected, options]);

    return (
        <div className={cn("space-y-3", className)}>
            {/* Selected authors as chips */}
            {(selectedLabels.length > 0 || newAuthors.length > 0) && (
                <div className="flex flex-wrap gap-1.5">
                    {selectedLabels.map((author) => (
                        <Chip
                            key={author.value}
                            variant="selected"
                            icon={<Check className="h-3 w-3" />}
                            onRemove={() => handleRemove(author.value)}
                            removeDisabled={disabled}
                        >
                            {author.label}
                        </Chip>
                    ))}
                    {/* New authors */}
                    {newAuthors.map((author, idx) => (
                        <Chip
                            key={`new-${idx}`}
                            variant="new"
                            suffix={<span className="text-xs text-emerald-500">(mới)</span>}
                            onRemove={onRemoveNewAuthor ? () => onRemoveNewAuthor(idx) : undefined}
                            removeDisabled={disabled}
                        >
                            {author.fullName}
                        </Chip>
                    ))}
                </div>
            )}

            {/* Search input with dropdown */}
            <div className="relative">
                <input
                    ref={inputRef}
                    type="text"
                    value={inputValue}
                    onChange={(e) => {
                        setInputValue(e.target.value);
                        setIsOpen(true);
                        setPreviewAuthor(null);
                    }}
                    onFocus={() => inputValue && setIsOpen(true)}
                    placeholder={placeholder}
                    disabled={disabled}
                    className="w-full px-3 py-2 text-sm border border-input bg-background focus:outline-none focus:ring-1 focus:ring-ring disabled:opacity-50"
                />

                {/* Dropdown results */}
                {isOpen && debouncedInput && (
                    <div
                        ref={dropdownRef}
                        className="absolute z-50 w-full mt-1 bg-popover border border-input shadow-lg max-h-60 overflow-auto"
                    >
                        {filteredOptions.length === 0 ? (
                            <div className="px-3 py-2 text-sm text-muted-foreground">
                                Không tìm thấy "{debouncedInput}"
                            </div>
                        ) : (
                            filteredOptions.map((option) => {
                                const isSelected = selected.includes(option.value);
                                return (
                                    <div
                                        key={option.value}
                                        className={cn(
                                            "flex items-center justify-between px-3 py-2 text-sm cursor-pointer hover:bg-accent",
                                            isSelected && "bg-primary/10"
                                        )}
                                    >
                                        <button
                                            type="button"
                                            onClick={() => handleSelect(option.value)}
                                            disabled={disabled || isSelected}
                                            className={cn(
                                                "flex-1 text-left",
                                                isSelected && "text-muted-foreground"
                                            )}
                                        >
                                            {isSelected && <Check className="inline h-3 w-3 mr-1" />}
                                            {option.label}
                                            {isSelected && <span className="text-xs ml-1">(đã chọn)</span>}
                                        </button>
                                        {option.description && (
                                            <button
                                                type="button"
                                                onClick={(e) => {
                                                    e.stopPropagation();
                                                    setPreviewAuthor(previewAuthor?.value === option.value ? null : option);
                                                }}
                                                className="p-1 hover:bg-muted rounded"
                                                title="Xem mô tả"
                                            >
                                                <Info className="h-4 w-4 text-muted-foreground" />
                                            </button>
                                        )}
                                    </div>
                                );
                            })
                        )}

                        {/* Preview author description */}
                        {previewAuthor && (
                            <div className="border-t px-3 py-2 bg-muted/50">
                                <div className="text-sm font-medium">{previewAuthor.label}</div>
                                <div className="text-xs text-muted-foreground mt-1">
                                    {previewAuthor.description || "Không có mô tả"}
                                </div>
                            </div>
                        )}
                    </div>
                )}
            </div>

            {/* Button thêm tác giả mới */}
            {onAddNew && !showNewForm && (
                <Button
                    type="button"
                    variant="outline"
                    size="sm"
                    onClick={() => setShowNewForm(true)}
                    disabled={disabled}
                >
                    <Plus className="h-4 w-4 mr-1" />
                    Thêm tác giả mới
                </Button>
            )}

            {/* Form thêm tác giả mới */}
            {showNewForm && (
                <div className="p-3 border bg-muted space-y-2">
                    <div>
                        <Label className="text-sm">Tên tác giả *</Label>
                        <Input
                            value={newName}
                            onChange={(e) => setNewName(e.target.value)}
                            placeholder="Nhập tên tác giả"
                            disabled={disabled}
                            className="mt-1"
                        />
                    </div>
                    <div>
                        <Label className="text-sm">Mô tả (tùy chọn)</Label>
                        <Textarea
                            value={newDescription}
                            onChange={(e) => setNewDescription(e.target.value)}
                            placeholder="VD: Giảng viên CNTT, Tiến sĩ..."
                            disabled={disabled}
                            rows={2}
                            className="mt-1"
                        />
                    </div>
                    <div className="flex gap-2">
                        <Button
                            type="button"
                            size="sm"
                            onClick={handleAddNewAuthor}
                            disabled={disabled || !newName.trim()}
                        >
                            Thêm
                        </Button>
                        <Button
                            type="button"
                            variant="ghost"
                            size="sm"
                            onClick={() => {
                                setShowNewForm(false);
                                setNewName("");
                                setNewDescription("");
                            }}
                            disabled={disabled}
                        >
                            Hủy
                        </Button>
                    </div>
                </div>
            )}
        </div>
    );
}
