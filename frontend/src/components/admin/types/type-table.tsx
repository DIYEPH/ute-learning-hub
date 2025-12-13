"use client";

import { useTranslations } from "next-intl";
import { BaseTable, BaseTableColumn } from "@/src/components/admin/tables/base-table";
import type { TypeDto } from "@/src/api/database/types.gen";

interface TypeTableProps {
  types: TypeDto[];
  onEdit?: (type: TypeDto) => void;
  onDelete?: (type: TypeDto) => void;
  onBulkDelete?: (ids: string[]) => void | Promise<void>;
  loading?: boolean;
  onSort?: (sortKey: string, direction: "asc" | "desc" | null) => void;
  sortKey?: string | null;
  sortDirection?: "asc" | "desc" | null;
  enableClientSort?: boolean;
}

export function TypeTable({
  types,
  onEdit,
  onDelete,
  onBulkDelete,
  loading,
  onSort,
  sortKey,
  sortDirection,
  enableClientSort,
}: TypeTableProps) {
  const t = useTranslations("admin.types");

  const columns: BaseTableColumn<TypeDto>[] = [
    {
      key: "typeName",
      header: t("table.typeName"),
      className: "min-w-[200px]",
      sortable: true,
      render: (type) => (
        <div className="font-medium text-foreground">{type.typeName}</div>
      ),
    },
  ];

  return (
    <BaseTable
      data={types}
      columns={columns}
      loading={loading}
      onEdit={onEdit}
      onDelete={onDelete}
      onBulkDelete={onBulkDelete}
      editLabel={t("table.edit")}
      deleteLabel={t("table.delete")}
      loadingLabel={t("table.loading")}
      noDataLabel={t("table.noData")}
      selectedCountLabel={t("table.selectedCount", { count: 0 })}
      deleteSelectedLabel={t("table.deleteSelected")}
      onSort={onSort}
      sortKey={sortKey}
      sortDirection={sortDirection}
      enableClientSort={enableClientSort}
    />
  );
}


