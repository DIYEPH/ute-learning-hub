"use client";

import { useState, useRef, useEffect } from "react";
import { Search, ChevronDown, X, Check } from "lucide-react";

interface FilterOption {
    value: string;
    label: string;
}

interface SearchableFilterProps {
    label: string;
    placeholder?: string;
    searchPlaceholder?: string;
    options: FilterOption[];
    value: string | null;
    onChange: (value: string | null) => void;
    className?: string;
}

export function SearchableFilter({
    label,
    placeholder = "Chọn...",
    searchPlaceholder = "Tìm kiếm...",
    options,
    value,
    onChange,
    className = "",
}: SearchableFilterProps) {
    const [isOpen, setIsOpen] = useState(false);
    const [searchTerm, setSearchTerm] = useState("");
    const containerRef = useRef<HTMLDivElement>(null);
    const inputRef = useRef<HTMLInputElement>(null);

    const selectedOption = options.find((opt) => opt.value === value);

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

    const handleSelect = (optValue: string) => {
        onChange(optValue === value ? null : optValue);
        setIsOpen(false);
        setSearchTerm("");
    };

    const handleClear = (e: React.MouseEvent) => {
        e.stopPropagation();
        onChange(null);
    };

    return (
        <div ref={containerRef} className={`relative ${className}`}>
            <label className="block text-xs font-medium text-slate-600 dark:text-slate-300 mb-1">
                {label}
            </label>
            <button
                type="button"
                onClick={() => setIsOpen(!isOpen)}
                className="flex h-9 w-full items-center justify-between border border-input bg-background px-3 text-sm shadow-sm hover:bg-slate-50 dark:hover:bg-slate-800"
            >
                <span className={selectedOption ? "text-foreground" : "text-muted-foreground"}>
                    {selectedOption ? selectedOption.label : placeholder}
                </span>
                <div className="flex items-center gap-1">
                    {value && (
                        <X className="h-3.5 w-3.5 text-slate-400 hover:text-slate-600" onClick={handleClear} />
                    )}
                    <ChevronDown className={`h-4 w-4 text-slate-400 transition-transform ${isOpen ? "rotate-180" : ""}`} />
                </div>
            </button>

            {isOpen && (
                <div className="absolute z-50 mt-1 w-full border border-input bg-background shadow-lg">
                    <div className="p-2 border-b border-input">
                        <div className="relative">
                            <Search className="absolute left-2 top-1/2 h-4 w-4 -translate-y-1/2 text-slate-400" />
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
                                    onClick={() => handleSelect(opt.value)}
                                    className={`flex w-full items-center justify-between px-3 py-2 text-sm text-left hover:bg-slate-100 dark:hover:bg-slate-800 ${opt.value === value ? "bg-slate-100 dark:bg-slate-800" : ""
                                        }`}
                                >
                                    <span className="truncate">{opt.label}</span>
                                    {opt.value === value && <Check className="h-4 w-4 text-sky-500 shrink-0" />}
                                </button>
                            ))
                        )}
                    </div>
                </div>
            )}
        </div>
    );
}
