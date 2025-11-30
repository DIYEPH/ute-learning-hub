"use client";

import { useState, useEffect, ReactNode } from "react";
import { Button } from "@/src/components/ui/button";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/src/components/ui/table";
import { useTranslations } from "next-intl";
import { Trash2, ArrowUpDown, ArrowUp, ArrowDown } from "lucide-react";

export type SortDirection = "asc" | "desc" | null;

export interface BaseTableColumn<T> {
  key: string;
  header: string;
  render: (item: T, index: number) => ReactNode;
  className?: string;
  sortable?: boolean;
  sortKey?: string; // Key to use for sorting (defaults to column.key)
}

export interface BaseTableProps<T extends { id?: string }> {
  data: T[];
  columns: BaseTableColumn<T>[];
  loading?: boolean;
  onEdit?: (item: T) => void;
  onDelete?: (item: T) => void;
  onBulkDelete?: (ids: string[]) => void | Promise<void>;
  editLabel?: string;
  deleteLabel?: string;
  loadingLabel?: string;
  noDataLabel?: string;
  selectedCountLabel?: string;
  deleteSelectedLabel?: string;
  actionsColumnClassName?: string;
  showCheckbox?: boolean;
  showActions?: boolean;
  getId?: (item: T) => string | undefined;
  onSort?: (sortKey: string, direction: SortDirection) => void;
  sortKey?: string | null;
  sortDirection?: SortDirection;
  enableClientSort?: boolean; // If true, sort data client-side instead of calling onSort
  getSortValue?: (item: T, sortKey: string) => any; // Custom function to get sort value
}

export function BaseTable<T extends { id?: string }>({
  data,
  columns,
  loading = false,
  onEdit,
  onDelete,
  onBulkDelete,
  editLabel,
  deleteLabel,
  loadingLabel,
  noDataLabel,
  selectedCountLabel,
  deleteSelectedLabel,
  actionsColumnClassName = "text-right min-w-[200px]",
  showCheckbox = true,
  showActions = true,
  getId = (item) => item.id,
  onSort,
  sortKey: externalSortKey,
  sortDirection: externalSortDirection,
  enableClientSort = false,
  getSortValue,
}: BaseTableProps<T>) {
  const t = useTranslations("common");
  const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set());
  const [internalSortKey, setInternalSortKey] = useState<string | null>(null);
  const [internalSortDirection, setInternalSortDirection] = useState<SortDirection>(null);

  const sortKey = externalSortKey !== undefined ? externalSortKey : internalSortKey;
  const sortDirection = externalSortDirection !== undefined ? externalSortDirection : internalSortDirection;

  const handleSort = (columnKey: string, columnSortKey?: string) => {
    const key = columnSortKey || columnKey;
    let newDirection: SortDirection = "asc";
    
    if (sortKey === key) {
      if (sortDirection === "asc") {
        newDirection = "desc";
      } else if (sortDirection === "desc") {
        newDirection = null;
      }
    }

    if (externalSortKey === undefined) {
      setInternalSortKey(newDirection ? key : null);
      setInternalSortDirection(newDirection);
    }

    if (onSort) {
      onSort(newDirection ? key : "", newDirection);
    }
  };

  useEffect(() => {
    setSelectedIds(new Set());
  }, [data]);

  const handleSelectAll = (checked: boolean) => {
    if (checked) {
      const allIds = data
        .map((item) => getId(item))
        .filter((id): id is string => !!id);
      setSelectedIds(new Set(allIds));
    } else {
      setSelectedIds(new Set());
    }
  };

  const handleSelectOne = (id: string, checked: boolean) => {
    const newSelected = new Set(selectedIds);
    if (checked) {
      newSelected.add(id);
    } else {
      newSelected.delete(id);
    }
    setSelectedIds(newSelected);
  };

  const handleBulkDelete = async () => {
    if (selectedIds.size > 0 && onBulkDelete) {
      await onBulkDelete(Array.from(selectedIds));
      setSelectedIds(new Set());
    }
  };

  const isAllSelected = data.length > 0 && selectedIds.size === data.filter((item) => getId(item)).length;
  const isIndeterminate = selectedIds.size > 0 && selectedIds.size < data.filter((item) => getId(item)).length;

  // Client-side sorting
  const sortedData = enableClientSort && sortKey && sortDirection ? (() => {
    const sorted = [...data];
    sorted.sort((a, b) => {
      const getValue = getSortValue || ((item: T, key: string) => {
        const keys = key.split('.');
        let value: any = item;
        for (const k of keys) {
          value = value?.[k];
        }
        return value;
      });
      
      const aValue = getValue(a, sortKey);
      const bValue = getValue(b, sortKey);
      
      if (aValue === null || aValue === undefined) return 1;
      if (bValue === null || bValue === undefined) return -1;
      
      if (typeof aValue === 'string' && typeof bValue === 'string') {
        return sortDirection === 'asc' 
          ? aValue.localeCompare(bValue)
          : bValue.localeCompare(aValue);
      }
      
      if (typeof aValue === 'number' && typeof bValue === 'number') {
        return sortDirection === 'asc' ? aValue - bValue : bValue - aValue;
      }
      
      return sortDirection === 'asc'
        ? String(aValue).localeCompare(String(bValue))
        : String(bValue).localeCompare(String(aValue));
    });
    return sorted;
  })() : data;

  if (loading) {
    return (
      <div className="flex items-center justify-center py-8">
        <p className="text-slate-600 dark:text-slate-400">
          {loadingLabel || t("loading")}
        </p>
      </div>
    );
  }

  if (data.length === 0) {
    return (
      <div className="flex items-center justify-center py-8">
        <p className="text-slate-600 dark:text-slate-400">
          {noDataLabel || t("noData") || "Không có dữ liệu"}
        </p>
      </div>
    );
  }

  return (
    <div className="space-y-2">
      {selectedIds.size > 0 && onBulkDelete && (
        <div className="flex items-center justify-between p-2 bg-blue-50 dark:bg-blue-950 rounded border border-blue-200 dark:border-blue-800">
          <span className="text-sm text-blue-900 dark:text-blue-100">
            {selectedCountLabel || `${selectedIds.size} mục đã chọn`}
          </span>
          <Button
            variant="destructive"
            size="sm"
            onClick={handleBulkDelete}
            className="h-7 px-2 text-xs sm:text-sm"
          >
            <Trash2 size={14} className="mr-1" />
            {deleteSelectedLabel || "Xóa đã chọn"}
          </Button>
        </div>
      )}
      <div className="border rounded overflow-x-auto">
        <Table>
          <TableHeader>
            <TableRow>
              {showCheckbox && (
                <TableHead className="w-12">
                  <input
                    type="checkbox"
                    checked={isAllSelected}
                    ref={(input) => {
                      if (input) input.indeterminate = isIndeterminate;
                    }}
                    onChange={(e) => handleSelectAll(e.target.checked)}
                    className="cursor-pointer"
                  />
                </TableHead>
              )}
              {columns.map((column) => {
                const columnSortKey = column.sortKey || column.key;
                const isSorted = sortKey === columnSortKey;
                const isAsc = isSorted && sortDirection === "asc";
                const isDesc = isSorted && sortDirection === "desc";
                
                return (
                  <TableHead key={column.key} className={column.className}>
                    <div className="flex items-center gap-2">
                      <span>{column.header}</span>
                      {column.sortable !== false && onSort && (
                        <button
                          type="button"
                          onClick={() => handleSort(column.key, columnSortKey)}
                          className="inline-flex items-center justify-center hover:bg-slate-100 dark:hover:bg-slate-800 rounded p-1 transition-colors"
                          aria-label={`Sort by ${column.header}`}
                        >
                          {isAsc ? (
                            <ArrowUp size={14} className="text-primary" />
                          ) : isDesc ? (
                            <ArrowDown size={14} className="text-primary" />
                          ) : (
                            <ArrowUpDown size={14} className="text-slate-400" />
                          )}
                        </button>
                      )}
                    </div>
                  </TableHead>
                );
              })}
              {showActions && (onEdit || onDelete) && (
                <TableHead className={actionsColumnClassName}>
                  {t("actions")}
                </TableHead>
              )}
            </TableRow>
          </TableHeader>
          <TableBody>
            {sortedData.map((item, index) => {
              const itemId = getId(item);
              const isSelected = itemId ? selectedIds.has(itemId) : false;
              return (
                <TableRow key={itemId || index}>
                  {showCheckbox && (
                    <TableCell>
                      {itemId && (
                        <input
                          type="checkbox"
                          checked={isSelected}
                          onChange={(e) => handleSelectOne(itemId, e.target.checked)}
                          className="cursor-pointer"
                        />
                      )}
                    </TableCell>
                  )}
                  {columns.map((column) => (
                    <TableCell key={column.key}>
                      {column.render(item, index)}
                    </TableCell>
                  ))}
                  {showActions && (onEdit || onDelete) && (
                    <TableCell className="text-right">
                      <div className="flex items-center justify-end gap-1 flex-wrap">
                        {onEdit && (
                          <Button
                            variant="ghost"
                            size="sm"
                            onClick={() => onEdit(item)}
                            className="h-7 px-2 text-xs sm:text-sm"
                          >
                            {editLabel || t("edit")}
                          </Button>
                        )}
                        {onDelete && (
                          <Button
                            variant="ghost"
                            size="sm"
                            onClick={() => onDelete(item)}
                            className="h-7 px-2 text-xs sm:text-sm text-red-600 hover:text-red-700 dark:text-red-400 dark:hover:text-red-300"
                          >
                            {deleteLabel || t("delete")}
                          </Button>
                        )}
                      </div>
                    </TableCell>
                  )}
                </TableRow>
              );
            })}
          </TableBody>
        </Table>
      </div>
    </div>
  );
}

