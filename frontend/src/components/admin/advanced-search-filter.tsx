"use client";

import { useState } from "react";
import { InputWithIcon } from "@/src/components/ui/input-with-icon";
import { Button } from "@/src/components/ui/button";
import { Search, Filter, X, ChevronDown, ChevronUp } from "lucide-react";
import { useTranslations } from "next-intl";

export interface FilterOption {
  key: string;
  label: string;
  type: "select" | "multiselect" | "checkbox" | "text";
  options?: { value: string; label: string }[];
  value?: string | string[] | boolean | null;
}

export interface AdvancedSearchFilterProps {
  searchTerm: string;
  onSearchChange: (value: string) => void;
  onSearchSubmit?: () => void;
  filters?: FilterOption[];
  onFilterChange?: (key: string, value: any) => void;
  onReset?: () => void;
  placeholder?: string;
  showAdvanced?: boolean;
  onToggleAdvanced?: (show: boolean) => void;
}

export function AdvancedSearchFilter({
  searchTerm,
  onSearchChange,
  onSearchSubmit,
  filters = [],
  onFilterChange,
  onReset,
  placeholder,
  showAdvanced: controlledShowAdvanced,
  onToggleAdvanced,
}: AdvancedSearchFilterProps) {
  const t = useTranslations("common");
  const [internalShowAdvanced, setInternalShowAdvanced] = useState(false);

  const showAdvanced = controlledShowAdvanced !== undefined
    ? controlledShowAdvanced
    : internalShowAdvanced;

  const setShowAdvanced = (value: boolean) => {
    if (onToggleAdvanced) {
      onToggleAdvanced(value);
    } else {
      setInternalShowAdvanced(value);
    }
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (onSearchSubmit) {
      onSearchSubmit();
    }
  };

  const hasActiveFilters = filters.some((filter) => {
    if (filter.value === undefined || filter.value === null) return false;
    if (filter.type === "multiselect") {
      return Array.isArray(filter.value) && filter.value.length > 0;
    }
    if (filter.type === "checkbox") {
      return filter.value === true;
    }
    return filter.value !== "" && filter.value !== false;
  });

  return (
    <div className="space-y-3">
      <form onSubmit={handleSubmit} className="flex gap-2">
        <div className="flex-1">
          <InputWithIcon
            prefixIcon={Search}
            placeholder={placeholder || t("searchPlaceholder")}
            value={searchTerm}
            onChange={(e) => onSearchChange(e.target.value)}
          />
        </div>
        {filters.length > 0 && (
          <Button
            type="button"
            variant={showAdvanced || hasActiveFilters ? "default" : "outline"}
            onClick={() => setShowAdvanced(!showAdvanced)}
            className="relative"
          >
            <Filter size={16} className="mr-1" />
            {t("filter")}
            {hasActiveFilters && (
              <span className="ml-1 px-1.5 py-0.5 text-xs bg-white/20 rounded-full">
                {filters.filter(
                  (f) => f.value !== undefined && f.value !== null && f.value !== "" && f.value !== false
                ).length}
              </span>
            )}
          </Button>
        )}
        {onReset && (searchTerm || hasActiveFilters) && (
          <Button
            type="button"
            variant="outline"
            onClick={onReset}
          >
            <X size={16} className="mr-1" />
            {t("reset")}
          </Button>
        )}
      </form>

      {showAdvanced && filters.length > 0 && (
        <div className="p-4 border  bg-slate-50 dark:bg-slate-900 space-y-3">
          <div className="flex items-center justify-between">
            <h3 className="text-sm font-semibold text-foreground">{t("advancedFilters")}</h3>
            <Button
              type="button"
              variant="ghost"
              size="sm"
              onClick={() => setShowAdvanced(false)}
            >
              <ChevronUp size={16} />
            </Button>
          </div>
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-3">
            {filters.map((filter) => (
              <div key={filter.key}>
                <label className="block text-sm font-medium text-foreground mb-1">
                  {filter.label}
                </label>
                {filter.type === "select" && (
                  <select
                    value={(filter.value as string) || ""}
                    onChange={(e) => onFilterChange?.(filter.key, e.target.value || null)}
                    className="w-full h-9  border border-input bg-background text-foreground px-3 py-1 text-sm shadow-sm transition-colors focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-50"
                  >
                    <option value="">{t("all")}</option>
                    {filter.options
                      ?.filter((option) => option.value !== "") // Remove empty value option to avoid duplicate
                      .map((option) => (
                        <option key={option.value} value={option.value}>
                          {option.label}
                        </option>
                      ))}
                  </select>
                )}
                {filter.type === "checkbox" && (
                  <label className="flex items-center gap-2 cursor-pointer">
                    <input
                      type="checkbox"
                      checked={filter.value === true}
                      onChange={(e) => onFilterChange?.(filter.key, e.target.checked)}
                      className="cursor-pointer"
                    />
                    <span className="text-sm text-foreground">{filter.label}</span>
                  </label>
                )}
                {filter.type === "multiselect" && (
                  <select
                    multiple
                    value={(filter.value as string[]) || []}
                    onChange={(e) => {
                      const selectedOptions = Array.from(e.target.selectedOptions);
                      const selectedValues = selectedOptions.map((option) => option.value);
                      onFilterChange?.(filter.key, selectedValues);
                    }}
                    size={5}
                    className="w-full  border border-input bg-background text-foreground px-3 py-2 text-sm shadow-sm transition-colors focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-50"
                  >
                    {filter.options?.map((option) => (
                      <option key={option.value} value={option.value}>
                        {option.label}
                      </option>
                    ))}
                  </select>
                )}
                {filter.type === "text" && (
                  <input
                    type="text"
                    value={(filter.value as string) || ""}
                    onChange={(e) => onFilterChange?.(filter.key, e.target.value || null)}
                    className="w-full h-9  border border-input bg-background text-foreground px-3 py-1 text-sm shadow-sm transition-colors focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring"
                  />
                )}
              </div>
            ))}
          </div>
        </div>
      )}
    </div>
  );
}

