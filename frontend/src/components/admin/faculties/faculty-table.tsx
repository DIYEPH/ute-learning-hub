"use client";

import { Button } from "@/src/components/ui/button";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/src/components/ui/table";
import type { FacultyDto2 } from "@/src/api/database/types.gen";

interface FacultyTableProps {
  faculties: FacultyDto2[];
  onEdit?: (faculty: FacultyDto2) => void;
  onDelete?: (faculty: FacultyDto2) => void;
  loading?: boolean;
}

export function FacultyTable({
  faculties,
  onEdit,
  onDelete,
  loading,
}: FacultyTableProps) {
  if (loading) {
    return (
      <div className="flex items-center justify-center py-8">
        <p className="text-slate-600 dark:text-slate-400">Đang tải...</p>
      </div>
    );
  }

  if (faculties.length === 0 && !loading) {
    return (
      <div className="flex items-center justify-center py-8">
        <p className="text-slate-600 dark:text-slate-400">Không có dữ liệu</p>
      </div>
    );
  }

  if (faculties.length === 0) {
    return null;
  }

  return (
    <div className="border rounded overflow-x-auto">
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead className="min-w-[80px]">Logo</TableHead>
            <TableHead className="min-w-[200px]">Tên khoa</TableHead>
            <TableHead className="min-w-[120px]">Mã khoa</TableHead>
            <TableHead className="text-right min-w-[200px]">Thao tác</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {faculties.map((faculty) => (
            <TableRow key={faculty.id}>
              <TableCell>
                {(faculty as any).logo ? (
                  <div className="relative h-10 w-10">
                    <img
                      src={(faculty as any).logo}
                      alt={faculty.facultyName || "Logo"}
                      className="h-10 w-10 object-contain rounded"
                      onError={(e) => {
                        (e.target as HTMLImageElement).style.display = "none";
                        const parent = (e.target as HTMLImageElement).parentElement;
                        if (parent) {
                          parent.innerHTML = '<div class="h-10 w-10 bg-slate-100 rounded flex items-center justify-center text-xs text-slate-400">No logo</div>';
                        }
                      }}
                    />
                  </div>
                ) : (
                  <div className="h-10 w-10 bg-slate-100 rounded flex items-center justify-center text-xs text-slate-400">
                    -
                  </div>
                )}
              </TableCell>
              <TableCell>
                <div className="font-medium">{faculty.facultyName}</div>
              </TableCell>
              <TableCell>
                <div className="text-sm">{faculty.facultyCode}</div>
              </TableCell>
              <TableCell className="text-right">
                <div className="flex items-center justify-end gap-1 flex-wrap">
                  {onEdit && (
                    <Button
                      variant="ghost"
                      size="sm"
                      onClick={() => onEdit(faculty)}
                      className="h-7 px-2 text-xs sm:text-sm"
                    >
                      Sửa
                    </Button>
                  )}
                  {onDelete && (
                    <Button
                      variant="ghost"
                      size="sm"
                      onClick={() => onDelete(faculty)}
                      className="h-7 px-2 text-xs sm:text-sm text-red-600 hover:text-red-700"
                    >
                      Xóa
                    </Button>
                  )}
                </div>
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </div>
  );
}

