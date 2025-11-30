"use client";

import { useState } from "react";
import { Label } from "@/src/components/ui/label";
import { Button } from "@/src/components/ui/button";
import { FileText, Download } from "lucide-react";
import { useTranslations } from "next-intl";

interface UserImportFormProps {
  onImport: (file: File) => void | Promise<void>;
  loading?: boolean;
}

export function UserImportForm({ onImport, loading }: UserImportFormProps) {
  const t = useTranslations("common");
  const [file, setFile] = useState<File | null>(null);
  const [error, setError] = useState<string | null>(null);

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const selectedFile = e.target.files?.[0];
    if (selectedFile) {
      const validExtensions = [".csv", ".xlsx", ".xls"];
      const fileExtension = selectedFile.name
        .substring(selectedFile.name.lastIndexOf("."))
        .toLowerCase();

      if (!validExtensions.includes(fileExtension)) {
        setError("Chỉ chấp nhận file CSV hoặc Excel (.csv, .xlsx, .xls)");
        setFile(null);
        return;
      }

      setError(null);
      setFile(selectedFile);
    }
  };

  const handleRemoveFile = () => {
    setFile(null);
    setError(null);
  };

  const handleDownloadTemplate = () => {
    const template = `Email,FullName,Username,Password,MajorCode,Gender
student1@student.ute.edu.vn,Nguyễn Văn A,student1,Password123!,7480201,Male
student2@student.ute.edu.vn,Trần Thị B,student2,Password123!,7480201,Female`;

    const blob = new Blob([template], { type: "text/csv;charset=utf-8;" });
    const link = document.createElement("a");
    const url = URL.createObjectURL(blob);
    link.setAttribute("href", url);
    link.setAttribute("download", "user_import_template.csv");
    link.style.visibility = "hidden";
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  };

  return (
    <div className="space-y-4">
      <div className="space-y-2">
        <div className="flex items-center justify-between">
          <Label>Chọn file để import</Label>
          <Button
            type="button"
            variant="outline"
            size="sm"
            onClick={handleDownloadTemplate}
            className="gap-2"
          >
            <Download size={16} />
            Tải mẫu
          </Button>
        </div>
        <input
          type="file"
          accept=".csv,.xlsx,.xls"
          onChange={handleFileChange}
          className="hidden"
          id="file-upload"
        />
        <label
          htmlFor="file-upload"
          className="flex items-center gap-2 px-4 py-3 border-2 border-dashed border-slate-300 dark:border-slate-700 rounded-md cursor-pointer hover:bg-slate-50 dark:hover:bg-slate-800 transition-colors"
        >
          <FileText size={20} />
          <span className="text-sm">
            {file ? file.name : "Chọn file CSV hoặc Excel"}
          </span>
        </label>
        {file && (
          <div className="flex items-center justify-between p-3 bg-slate-100 dark:bg-slate-800 rounded-md">
            <div className="flex items-center gap-2">
              <FileText size={16} />
              <span className="text-sm text-foreground">{file.name}</span>
            </div>
            <Button
              type="button"
              variant="ghost"
              size="sm"
              onClick={handleRemoveFile}
            >
              Xóa
            </Button>
          </div>
        )}
        {error && (
          <p className="text-sm text-destructive">{error}</p>
        )}
        <p className="text-xs text-slate-500 dark:text-slate-400">
          Chấp nhận: CSV, Excel (.csv, .xlsx, .xls)
        </p>
      </div>

      <div className="p-4 bg-blue-50 dark:bg-blue-950 rounded-md border border-blue-200 dark:border-blue-800">
        <h4 className="text-sm font-medium text-blue-900 dark:text-blue-100 mb-2">
          Hướng dẫn:
        </h4>
        <ul className="text-xs text-blue-800 dark:text-blue-200 space-y-1 list-disc list-inside">
          <li>File phải có các cột: Email, FullName, Username, Password, MajorCode, Gender</li>
          <li>Email và FullName là bắt buộc</li>
          <li>Password tối thiểu 6 ký tự</li>
          <li>Gender: Male, Female, hoặc Other</li>
          <li>MajorCode là mã ngành (có thể để trống)</li>
        </ul>
      </div>
    </div>
  );
}

