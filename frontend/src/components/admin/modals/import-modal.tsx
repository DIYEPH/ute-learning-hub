"use client";

import { ReactNode, useState, useRef } from "react";
import { BaseModal, BaseModalProps } from "./base-modal";
import { Button } from "@/src/components/ui/button";
import { useTranslations } from "next-intl";
import { Upload, FileText, X } from "lucide-react";

export interface ImportModalProps extends Omit<BaseModalProps, "footer" | "children"> {
  onImport: (file: File) => void | Promise<void>;
  onCancel?: () => void;
  accept?: string;
  acceptLabel?: string;
  submitLabel?: string;
  cancelLabel?: string;
  loading?: boolean;
  disabled?: boolean;
  children?: ReactNode;
}

export function ImportModal({
  open,
  onOpenChange,
  title,
  description,
  onImport,
  onCancel,
  accept = ".csv,.xlsx,.xls",
  acceptLabel,
  submitLabel,
  cancelLabel,
  loading = false,
  disabled = false,
  children,
  size = "md",
}: ImportModalProps) {
  const t = useTranslations("common");
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const fileInputRef = useRef<HTMLInputElement>(null);

  const handleFileSelect = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) {
      setSelectedFile(file);
    }
  };

  const handleRemoveFile = () => {
    setSelectedFile(null);
    if (fileInputRef.current) {
      fileInputRef.current.value = "";
    }
  };

  const handleSubmit = async () => {
    if (selectedFile) {
      await onImport(selectedFile);
      setSelectedFile(null);
      if (fileInputRef.current) {
        fileInputRef.current.value = "";
      }
    }
  };

  const handleCancel = () => {
    setSelectedFile(null);
    if (fileInputRef.current) {
      fileInputRef.current.value = "";
    }
    if (onCancel) {
      onCancel();
    } else {
      onOpenChange(false);
    }
  };

  const footer = (
    <>
      <Button
        type="button"
        variant="outline"
        onClick={handleCancel}
        disabled={loading}
      >
        {cancelLabel || t("cancel")}
      </Button>
      <Button
        type="button"
        onClick={handleSubmit}
        disabled={loading || disabled || !selectedFile}
      >
        {loading ? t("loading") : submitLabel || t("submit")}
      </Button>
    </>
  );

  return (
    <BaseModal
      open={open}
      onOpenChange={onOpenChange}
      title={title || "Import từ file"}
      description={description}
      size={size}
      footer={footer}
    >
      <div className="space-y-4">
        {children}

        <div className="space-y-2">
          <label className="text-sm font-medium text-foreground">
            {acceptLabel || "Chọn file"}
          </label>
          <div className="flex items-center gap-4">
            <input
              ref={fileInputRef}
              type="file"
              accept={accept}
              onChange={handleFileSelect}
              className="hidden"
              id="file-upload"
            />
            <label
              htmlFor="file-upload"
              className="flex items-center gap-2 px-4 py-2 border border-slate-300 dark:border-slate-700 rounded-md cursor-pointer hover:bg-slate-50 dark:hover:bg-slate-800 transition-colors"
            >
              <Upload size={18} />
              <span className="text-sm">Chọn file</span>
            </label>
            {selectedFile && (
              <div className="flex items-center gap-2 px-3 py-2 bg-slate-100 dark:bg-slate-800 rounded-md">
                <FileText size={16} />
                <span className="text-sm text-foreground">
                  {selectedFile.name}
                </span>
                <button
                  type="button"
                  onClick={handleRemoveFile}
                  className="ml-2 text-slate-500 hover:text-slate-700 dark:text-slate-400 dark:hover:text-slate-200"
                >
                  <X size={16} />
                </button>
              </div>
            )}
          </div>
          <p className="text-xs text-slate-500 dark:text-slate-400">
            Chấp nhận: {accept}
          </p>
        </div>
      </div>
    </BaseModal>
  );
}

