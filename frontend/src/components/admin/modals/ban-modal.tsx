"use client";

import { useState, useEffect } from "react";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/src/components/ui/dialog";
import { Button } from "@/src/components/ui/button";
import { Label } from "@/src/components/ui/label";
import { Input } from "@/src/components/ui/input";
import { useTranslations } from "next-intl";

interface BanModalProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  userName?: string | null;
  onConfirm: (banUntil: string | null) => void | Promise<void>;
  loading?: boolean;
}

export function BanModal({
  open,
  onOpenChange,
  userName,
  onConfirm,
  loading = false,
}: BanModalProps) {
  const t = useTranslations("admin.users");
  const [banType, setBanType] = useState<"permanent" | "temporary">("temporary");
  const [banDays, setBanDays] = useState<number>(1);
  const [banHours, setBanHours] = useState<number>(0);
  const [banUntil, setBanUntil] = useState<string>("");

  useEffect(() => {
    if (open) {
      // Reset form when modal opens
      setBanType("temporary");
      setBanDays(1);
      setBanHours(0);
      setBanUntil("");
    }
  }, [open]);

  useEffect(() => {
    if (banType === "temporary" && (banDays > 0 || banHours > 0)) {
      const now = new Date();
      const banDate = new Date(now);
      banDate.setDate(banDate.getDate() + banDays);
      banDate.setHours(banDate.getHours() + banHours);
      setBanUntil(banDate.toISOString());
    } else {
      setBanUntil("");
    }
  }, [banType, banDays, banHours]);

  const handleConfirm = async () => {
    const finalBanUntil = banType === "permanent" ? null : banUntil || null;
    await onConfirm(finalBanUntil);
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[500px]">
        <DialogHeader>
          <DialogTitle>{t("banModal.title")}</DialogTitle>
          <DialogDescription>
            {t("banModal.description", { name: userName || t("banModal.user") })}
          </DialogDescription>
        </DialogHeader>
        <div className="space-y-4 py-4">
          <div className="space-y-2">
            <Label>{t("banModal.banType")}</Label>
            <div className="flex gap-4">
              <label className="flex items-center gap-2 cursor-pointer">
                <input
                  type="radio"
                  name="banType"
                  value="temporary"
                  checked={banType === "temporary"}
                  onChange={(e) => setBanType(e.target.value as "temporary" | "permanent")}
                  className="cursor-pointer"
                />
                <span className="text-sm">{t("banModal.temporary")}</span>
              </label>
              <label className="flex items-center gap-2 cursor-pointer">
                <input
                  type="radio"
                  name="banType"
                  value="permanent"
                  checked={banType === "permanent"}
                  onChange={(e) => setBanType(e.target.value as "temporary" | "permanent")}
                  className="cursor-pointer"
                />
                <span className="text-sm">{t("banModal.permanent")}</span>
              </label>
            </div>
          </div>

          {banType === "temporary" && (
            <div className="space-y-4">
              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="banDays">{t("banModal.days")}</Label>
                  <Input
                    id="banDays"
                    type="number"
                    min="0"
                    value={banDays}
                    onChange={(e) => setBanDays(Math.max(0, parseInt(e.target.value) || 0))}
                    className="w-full"
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="banHours">{t("banModal.hours")}</Label>
                  <Input
                    id="banHours"
                    type="number"
                    min="0"
                    max="23"
                    value={banHours}
                    onChange={(e) => setBanHours(Math.max(0, Math.min(23, parseInt(e.target.value) || 0)))}
                    className="w-full"
                  />
                </div>
              </div>
              {banUntil && (
                <div className="text-sm text-slate-600 dark:text-slate-400">
                  {t("banModal.banUntil")}: {new Date(banUntil).toLocaleString("vi-VN")}
                </div>
              )}
            </div>
          )}

          {banType === "permanent" && (
            <div className="text-sm text-amber-600 dark:text-amber-400 bg-amber-50 dark:bg-amber-950 p-2 rounded">
              {t("banModal.permanentWarning")}
            </div>
          )}
        </div>
        <DialogFooter>
          <Button
            variant="outline"
            onClick={() => onOpenChange(false)}
            disabled={loading}
          >
            {t("banModal.cancel")}
          </Button>
          <Button
            variant="destructive"
            onClick={handleConfirm}
            disabled={loading || (banType === "temporary" && banDays === 0 && banHours === 0)}
          >
            {loading ? t("banModal.banning") : t("banModal.confirm")}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}


