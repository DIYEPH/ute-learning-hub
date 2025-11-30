"use client";

import { useTranslations } from "next-intl";
import { BaseTable, BaseTableColumn } from "@/src/components/admin/tables/base-table";
import type { MajorDto2 } from "@/src/api/database/types.gen";

interface MajorTableProps {
  majors: MajorDto2[];
  onEdit?: (major: MajorDto2) => void;
  onDelete?: (major: MajorDto2) => void;
  onBulkDelete?: (ids: string[]) => void | Promise<void>;
  loading?: boolean;
  onSort?: (sortKey: string, direction: "asc" | "desc" | null) => void;
  sortKey?: string | null;
  sortDirection?: "asc" | "desc" | null;
  enableClientSort?: boolean;
}

export function MajorTable({
  majors,
  onEdit,
  onDelete,
  onBulkDelete,
  loading,
  onSort,
  sortKey,
  sortDirection,
  enableClientSort,
}: MajorTableProps) {
  const t = useTranslations("admin.majors");

  const columns: BaseTableColumn<MajorDto2>[] = [
    {
      key: "majorName",
      header: t("table.majorName"),
      className: "min-w-[200px]",
      sortable: true,
      render: (major) => (
        <div className="font-medium text-foreground">{major.majorName}</div>
      ),
    },
    {
      key: "majorCode",
      header: t("table.majorCode"),
      className: "min-w-[120px]",
      sortable: true,
      render: (major) => (
        <div className="text-sm text-foreground">{major.majorCode}</div>
      ),
    },
    {
      key: "faculty",
      header: t("table.faculty"),
      className: "min-w-[200px]",
      sortable: true,
      sortKey: "faculty.facultyName",
      render: (major) => (
        <div className="text-sm text-foreground">
          {major.faculty?.facultyName || "-"}
        </div>
      ),
    },
  ];

  return (
    <BaseTable
      data={majors}
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
