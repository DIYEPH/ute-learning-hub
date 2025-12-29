"use client";

import { useState, useRef, useEffect } from "react";
import { Search, ChevronDown, X, Check } from "lucide-react";

interface FilterOption {
    value: string;
    label: string;
}

interface SearchableMultiFilterProps {
    label: string;
    placeholder?: string;
    searchPlaceholder?: string;
    options: FilterOption[];
    value: string[];
    onChange: (value: string[]) => void;
    className?: string;
}

export function SearchableMultiFilter({
    label,
    placeholder = "Chọn...",
    searchPlaceholder = "Tìm kiếm...",
    options,
    value,
    onChange,
    className = "",
}: SearchableMultiFilterProps) {
    const [isOpen, setIsOpen] = useState(false);
    const [searchTerm, setSearchTerm] = useState("");
    const containerRef = useRef<HTMLDivElement>(null);
    const inputRef = useRef<HTMLInputElement>(null);

    const selectedOptions = options.filter((opt) => value.includes(opt.value));

    const filteredOptions = options.filter((opt) =>
        opt.label.toLowerCase().includes(searchTerm.toLowerCase())
    );

    useEffect(() => {
        const handleClickOutside = (event: MouseEvent) => {
            if (containerRef.current && !containerRef.current.contains(event.target as Node)) {
                setIsOpen(false);
                setSearchTerm("");
            }
        };
        document.addEventListener("mousedown", handleClickOutside);
        return () => document.removeEventListener("mousedown", handleClickOutside);
    }, []);

    useEffect(() => {
        if (isOpen && inputRef.current) {
            inputRef.current.focus();
        }
    }, [isOpen]);

    const handleToggle = (optValue: string) => {
        if (value.includes(optValue)) {
            onChange(value.filter((v) => v !== optValue));
        } else {
            onChange([...value, optValue]);
        }
    };

    const handleClear = (e: React.MouseEvent) => {
        e.stopPropagation();
        onChange([]);
    };

    const handleRemoveTag = (e: React.MouseEvent, optValue: string) => {
        e.stopPropagation();
        onChange(value.filter((v) => v !== optValue));
    };

    return (
        <div ref={containerRef} className={`relative ${className}`}>
            <label className="block text-xs font-medium text-muted-foreground mb-1">
                {label}
            </label>
            <button
                type="button"
                onClick={() => setIsOpen(!isOpen)}
                className="flex min-h-9 w-full items-center justify-between border border-input bg-background px-3 py-1.5 text-sm shadow-sm hover:bg-muted"
            >
                <div className="flex flex-wrap gap-1 flex-1">
                    {selectedOptions.length > 0 ? (
                        selectedOptions.slice(0, 2).map((opt) => (
                            <span
                                key={opt.value}
                                className="inline-flex items-center gap-1 px-2 py-0.5 text-xs bg-primary/10 text-primary rounded"
                            >
                                <span className="truncate max-w-[120px]">{opt.label}</span>
                                <X className="h-3 w-3 cursor-pointer hover:text-primary/80" onClick={(e) => handleRemoveTag(e, opt.value)} />
                            </span>
                        ))
                    ) : (
                        <span className="text-muted-foreground">{placeholder}</span>
                    )}
                    {selectedOptions.length > 2 && (
                        <span className="inline-flex items-center px-2 py-0.5 text-xs bg-muted rounded">
                            +{selectedOptions.length - 2}
                        </span>
                    )}
                </div>
                <div className="flex items-center gap-1 ml-2 shrink-0">
                    {value.length > 0 && (
                        <X className="h-3.5 w-3.5 text-muted-foreground hover:text-foreground" onClick={handleClear} />
                    )}
                    <ChevronDown className={`h-4 w-4 text-muted-foreground transition-transform ${isOpen ? "rotate-180" : ""}`} />
                </div>
            </button>

            {isOpen && (
                <div className="absolute z-50 mt-1 w-full border border-input bg-background shadow-lg">
                    <div className="p-2 border-b border-input">
                        <div className="relative">
                            <Search className="absolute left-2 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
                            <input
                                ref={inputRef}
                                type="text"
                                value={searchTerm}
                                onChange={(e) => setSearchTerm(e.target.value)}
                                placeholder={searchPlaceholder}
                                className="h-8 w-full pl-8 pr-3 text-sm border border-input bg-background focus:outline-none focus:ring-1 focus:ring-ring"
                            />
                        </div>
                    </div>
                    <div className="max-h-60 overflow-y-auto">
                        {filteredOptions.length === 0 ? (
                            <div className="px-3 py-2 text-sm text-muted-foreground">Không tìm thấy kết quả</div>
                        ) : (
                            filteredOptions.map((opt) => (
                                <button
                                    key={opt.value}
                                    type="button"
                                    onClick={() => handleToggle(opt.value)}
                                    className={`flex w-full items-center justify-between px-3 py-2 text-sm text-left hover:bg-muted ${value.includes(opt.value) ? "bg-muted" : ""
                                        }`}
                                >
                                    <span className="truncate">{opt.label}</span>
                                    {value.includes(opt.value) && <Check className="h-4 w-4 text-primary shrink-0" />}
                                </button>
                            ))
                        )}
                    </div>
                    {value.length > 0 && (
                        <div className="p-2 border-t border-input text-xs text-muted-foreground">
                            Đã chọn: {value.length}
                        </div>
                    )}
                </div>
            )}
        </div>
    );
}
