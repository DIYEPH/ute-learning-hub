"use client";

import { useTranslations } from "next-intl";
import { BaseTable, BaseTableColumn } from "@/src/components/admin/tables/base-table";
import type { SubjectDto2 } from "@/src/api/database/types.gen";

interface SubjectTableProps {
  subjects: SubjectDto2[];
  onEdit?: (subject: SubjectDto2) => void;
  onDelete?: (subject: SubjectDto2) => void;
  onBulkDelete?: (ids: string[]) => void | Promise<void>;
  loading?: boolean;
  onSort?: (sortKey: string, direction: "asc" | "desc" | null) => void;
  sortKey?: string | null;
  sortDirection?: "asc" | "desc" | null;
  enableClientSort?: boolean;
}

export function SubjectTable({
  subjects,
  onEdit,
  onDelete,
  onBulkDelete,
  loading,
  onSort,
  sortKey,
  sortDirection,
  enableClientSort,
}: SubjectTableProps) {
  const t = useTranslations("admin.subjects");

  const columns: BaseTableColumn<SubjectDto2>[] = [
    {
      key: "subjectName",
      header: t("table.subjectName"),
      className: "min-w-[200px]",
      sortable: true,
      render: (subject) => (
        <div className="font-medium text-foreground">{subject.subjectName}</div>
      ),
    },
    {
      key: "subjectCode",
      header: t("table.subjectCode"),
      className: "min-w-[120px]",
      sortable: true,
      render: (subject) => (
        <div className="text-sm text-foreground">{subject.subjectCode}</div>
      ),
    },
    {
      key: "majors",
      header: t("table.majors"),
      className: "min-w-[250px]",
      sortable: false, // Complex field, disable sorting
      render: (subject) => (
        <div className="text-sm text-foreground">
          {subject.majors && subject.majors.length > 0 ? (
            <div className="flex flex-wrap gap-1">
              {subject.majors.map((major, index) => (
                <span
                  key={major.id || index}
                  className="inline-flex items-center px-2 py-0.5 rounded text-xs bg-slate-100 dark:bg-slate-800 text-slate-700 dark:text-slate-300"
                >
                  {major.majorName}
                  {major.faculty && (
                    <span className="ml-1 text-slate-500 dark:text-slate-400">
                      ({major.faculty.facultyCode})
                    </span>
                  )}
                </span>
              ))}
            </div>
          ) : (
            <span className="text-slate-400">-</span>
          )}
        </div>
      ),
    },
  ];

  return (
    <BaseTable
      data={subjects}
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

