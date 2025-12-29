"use client";

import { useTranslations } from "next-intl";
import { BaseTable, BaseTableColumn } from "@/src/components/admin/tables/base-table";
import { Badge } from "@/src/components/ui/badge";
import { BookOpen } from "lucide-react";
import type { MajorDetailDto } from "@/src/api/database/types.gen";

interface MajorTableProps {
  majors: MajorDetailDto[];
  onEdit?: (major: MajorDetailDto) => void;
  onDelete?: (major: MajorDetailDto) => void;
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

  const columns: BaseTableColumn<MajorDetailDto>[] = [
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
      key: "facultyName",
      header: t("table.faculty"),
      className: "min-w-[200px]",
      sortable: true,
      render: (major) => (
        <div className="text-sm text-foreground">
          {major.facultyName || "-"}
        </div>
      ),
    },
    {
      key: "subjectCount",
      header: t("table.subjectCount"),
      className: "min-w-[100px]",
      sortable: true,
      render: (major) => (
        <div className="flex items-center gap-1 text-sm">
          <BookOpen className="h-4 w-4 text-muted-foreground" />
          <Badge variant="secondary">{major.subjectCount ?? 0}</Badge>
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

