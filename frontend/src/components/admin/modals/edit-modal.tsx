"use client";

import { BaseModal, BaseModalProps } from "./base-modal";
import { Button } from "@/src/components/ui/button";
import { useTranslations } from "next-intl";

export interface EditModalProps extends Omit<BaseModalProps, "footer"> {
  onSubmit: () => void | Promise<void>;
  onCancel?: () => void;
  submitLabel?: string;
  cancelLabel?: string;
  loading?: boolean;
  disabled?: boolean;
}

export function EditModal({open, onOpenChange, title, description, children, onSubmit, onCancel, submitLabel, cancelLabel, loading = false, disabled = false, size = "md"}: EditModalProps) {
  const t = useTranslations("common");

  const handleSubmit = async (e?: React.FormEvent) => {
    if (e) 
      e.preventDefault();
    await onSubmit();
  };

  const handleCancel = () => {
    if (onCancel) 
      onCancel();
    else 
      onOpenChange(false);
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
        disabled={loading || disabled}
      >
        {loading ? t("loading") : submitLabel || t("update")}
      </Button>
    </>
  );

  return (
    <BaseModal
      open={open}
      onOpenChange={onOpenChange}
      title={title}
      description={description}
      size={size}
      footer={footer}
    >
      {children}
    </BaseModal>
  );
}


