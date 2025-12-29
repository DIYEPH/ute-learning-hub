"use client";

import { useState, useId } from "react";
import { BaseModal, BaseModalProps } from "./base-modal";
import { Button } from "@/src/components/ui/button";
import { Label } from "@/src/components/ui/label";
import { FileText, Download } from "lucide-react";
import { useTranslations } from "next-intl";

export interface ImportModalProps extends Omit<BaseModalProps, "footer" | "children"> {
  onImport: (file: File) => void | Promise<void>;
  onCancel?: () => void;
  accept?: string;
  submitLabel?: string;
  cancelLabel?: string;
  loading?: boolean;
  templateContent?: string;
  templateFileName?: string;
  helpText?: string;
  requiredColumns?: string;
}

export function ImportModal({
  open, onOpenChange, title, description, onImport, onCancel, accept = ".csv,.xlsx,.xls", submitLabel, cancelLabel, loading = false, size = "md", templateContent, templateFileName = "import_template.csv", helpText, requiredColumns
}: ImportModalProps) {
  const t = useTranslations("common");
  const fileInputId = useId();
  const [file, setFile] = useState<File | null>(null);
  const [error, setError] = useState<string | null>(null);

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const selectedFile = e.target.files?.[0];
    if (selectedFile) {
      const validExtensions = accept.split(",").map(ext => ext.trim().toLowerCase());
      const fileExtension = selectedFile.name.substring(selectedFile.name.lastIndexOf(".")).toLowerCase();

      if (!validExtensions.includes(fileExtension)) {
        setError(`Chỉ chấp nhận file: ${accept}`);
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
    const input = document.getElementById(fileInputId) as HTMLInputElement;
    if (input) input.value = "";
  };

  const handleImport = async () => {
    if (file) {
      await onImport(file);
      handleRemoveFile();
    }
  };

  const handleCancel = () => {
    handleRemoveFile();
    if (onCancel) onCancel();
    else onOpenChange(false);
  };

  const handleDownloadTemplate = () => {
    if (!templateContent) return;
    const blob = new Blob([templateContent], { type: "text/csv;charset=utf-8;" });
    const link = document.createElement("a");
    link.href = URL.createObjectURL(blob);
    link.download = templateFileName;
    link.style.visibility = "hidden";
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  };

  const footer = (
    <>
      <Button type="button" variant="outline" onClick={handleCancel} disabled={loading}>
        {cancelLabel || t("cancel")}
      </Button>
      <Button type="button" onClick={handleImport} disabled={loading || !file}>
        {loading ? t("loading") : submitLabel || "Import"}
      </Button>
    </>
  );

  return (
    <BaseModal open={open} onOpenChange={onOpenChange} title={title || "Import từ file"} description={description} size={size} footer={footer}>
      <div className="space-y-4">
        <div className="space-y-2">
          <div className="flex items-center justify-between">
            <Label>Chọn file để import</Label>
            {templateContent && (
              <Button type="button" variant="outline" size="sm" onClick={handleDownloadTemplate} className="gap-2">
                <Download size={16} />
                Tải mẫu
              </Button>
            )}
          </div>
          <input type="file" accept={accept} onChange={handleFileChange} className="hidden" id={fileInputId} />
          <label
            htmlFor={fileInputId}
            className="flex items-center gap-2 px-4 py-3 border-2 border-dashed border-border cursor-pointer hover:bg-muted transition-colors"
          >
            <FileText size={20} />
            <span className="text-sm">{file ? file.name : "Chọn file CSV hoặc Excel"}</span>
          </label>
          {file && (
            <div className="flex items-center justify-between p-3 bg-muted">
              <div className="flex items-center gap-2">
                <FileText size={16} />
                <span className="text-sm text-foreground">{file.name}</span>
              </div>
              <Button type="button" variant="ghost" size="sm" onClick={handleRemoveFile}>Xóa</Button>
            </div>
          )}
          {error && <p className="text-sm text-destructive">{error}</p>}
          {helpText && <p className="text-xs text-muted-foreground">{helpText}</p>}
        </div>
        {requiredColumns && (
          <div className="p-3 bg-muted border border-border">
            <p className="text-xs text-muted-foreground">{requiredColumns}</p>
          </div>
        )}
      </div>
    </BaseModal>
  );
}

