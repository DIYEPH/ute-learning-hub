"use client";

import { useTranslations } from "next-intl";
import { BaseTable, BaseTableColumn } from "@/src/components/admin/tables/base-table";
import { Badge } from "@/src/components/ui/badge";
import { GraduationCap } from "lucide-react";
import type { FacultyDetailDto } from "@/src/api/database/types.gen";

interface FacultyTableProps {
  faculties: FacultyDetailDto[];
  onEdit?: (faculty: FacultyDetailDto) => void;
  onDelete?: (faculty: FacultyDetailDto) => void;
  onBulkDelete?: (ids: string[]) => void | Promise<void>;
  loading?: boolean;
  onSort?: (sortKey: string, direction: "asc" | "desc" | null) => void;
  sortKey?: string | null;
  sortDirection?: "asc" | "desc" | null;
  enableClientSort?: boolean;
}

export function FacultyTable({
  faculties,
  onEdit,
  onDelete,
  onBulkDelete,
  loading,
  onSort,
  sortKey,
  sortDirection,
  enableClientSort,
}: FacultyTableProps) {
  const t = useTranslations("admin.faculties");

  const columns: BaseTableColumn<FacultyDetailDto>[] = [
    {
      key: "logo",
      header: t("table.logo"),
      className: "min-w-[80px]",
      sortable: false, // Image field, disable sorting
      render: (faculty) => (
        <>
          {faculty.logo ? (
            <div className="relative h-10 w-10">
              <img
                src={faculty.logo}
                alt={faculty.facultyName || "Logo"}
                className="h-10 w-10 object-contain rounded"
                onError={(e) => {
                  (e.target as HTMLImageElement).style.display = "none";
                }}
              />
            </div>
          ) : (
            <div className="h-10 w-10 bg-muted rounded flex items-center justify-center text-xs text-muted-foreground">
              -
            </div>
          )}
        </>
      ),
    },
    {
      key: "facultyName",
      header: t("table.facultyName"),
      className: "min-w-[200px]",
      sortable: true,
      render: (faculty) => (
        <div className="font-medium text-foreground">{faculty.facultyName}</div>
      ),
    },
    {
      key: "facultyCode",
      header: t("table.facultyCode"),
      className: "min-w-[120px]",
      sortable: true,
      render: (faculty) => (
        <div className="text-sm text-foreground">{faculty.facultyCode}</div>
      ),
    },
    {
      key: "majorCount",
      header: t("table.majorCount"),
      className: "min-w-[100px]",
      sortable: true,
      render: (faculty) => (
        <div className="flex items-center gap-1 text-sm">
          <GraduationCap className="h-4 w-4 text-muted-foreground" />
          <Badge variant="secondary">{faculty.majorCount ?? 0}</Badge>
        </div>
      ),
    },
  ];

  return (
    <BaseTable
      data={faculties}
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

