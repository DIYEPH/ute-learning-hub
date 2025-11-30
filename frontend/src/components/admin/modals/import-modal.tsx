"use client";

import { ReactNode } from "react";
import { BaseModal, BaseModalProps } from "./base-modal";
import { Button } from "@/src/components/ui/button";
import { useTranslations } from "next-intl";

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

  const handleSubmit = async () => {
    // Let children handle file selection and import
    if (children) {
      // Children component will handle the import
      return;
    }
  };

  const handleCancel = () => {
    if (onCancel) {
      onCancel();
    } else {
      onOpenChange(false);
    }
  };

  const footer = children ? undefined : (
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
        disabled={loading || disabled}
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
      </div>
    </BaseModal>
  );
}

