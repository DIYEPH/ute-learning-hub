"use client";

import { ReactNode } from "react";
import { BaseModal, BaseModalProps } from "./base-modal";
import { Button } from "@/src/components/ui/button";
import { useTranslations } from "next-intl";
import { AlertTriangle } from "lucide-react";

export interface DeleteModalProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  title?: string;
  description?: string;
  itemName?: string;
  onConfirm: () => void | Promise<void>;
  onCancel?: () => void;
  confirmLabel?: string;
  cancelLabel?: string;
  loading?: boolean;
}

export function DeleteModal({
  open,
  onOpenChange,
  title,
  description,
  itemName,
  onConfirm,
  onCancel,
  confirmLabel,
  cancelLabel,
  loading = false,
}: DeleteModalProps) {
  const t = useTranslations("common");

  const handleConfirm = async () => {
    await onConfirm();
  };

  const handleCancel = () => {
    if (onCancel) {
      onCancel();
    } else {
      onOpenChange(false);
    }
  };

  const defaultTitle = title || t("delete");
  const defaultDescription =
    description ||
    (itemName
      ? `Bạn có chắc chắn muốn xóa "${itemName}"? Hành động này không thể hoàn tác.`
      : "Bạn có chắc chắn muốn xóa? Hành động này không thể hoàn tác.");

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
        variant="destructive"
        onClick={handleConfirm}
        disabled={loading}
      >
        {loading ? t("loading") : confirmLabel || t("delete")}
      </Button>
    </>
  );

  return (
    <BaseModal
      open={open}
      onOpenChange={onOpenChange}
      title={defaultTitle}
      description={defaultDescription}
      size="sm"
      footer={footer}
    >
      <div className="flex items-center gap-4 py-4">
        <div className="flex-shrink-0">
          <AlertTriangle className="h-12 w-12 text-destructive" />
        </div>
        <div className="flex-1">
          <p className="text-sm text-slate-600 dark:text-slate-400">
            {defaultDescription}
          </p>
        </div>
      </div>
    </BaseModal>
  );
}

