"use client";

import { useState, useEffect } from "react";
import { Loader2, GraduationCap } from "lucide-react";
import { Dialog, DialogContent, DialogDescription, DialogHeader, DialogTitle } from "@/src/components/ui/dialog";
import { Button } from "@/src/components/ui/button";
import { putApiAccountProfile } from "@/src/api";
import { getApiMajor } from "@/src/api";
import type { MajorDetailDto } from "@/src/api/database/types.gen";
import { useNotification } from "@/src/components/providers/notification-provider";

interface ProfileCompletionDialogProps {
    open: boolean;
    onComplete: () => void;
}

export default function ProfileCompletionDialog({ open, onComplete }: ProfileCompletionDialogProps) {
    const { success: notifySuccess, error: notifyError } = useNotification();
    const [majors, setMajors] = useState<MajorDetailDto[]>([]);
    const [selectedMajorId, setSelectedMajorId] = useState<string>("");
    const [introduction, setIntroduction] = useState<string>("");
    const [loading, setLoading] = useState(false);
    const [loadingMajors, setLoadingMajors] = useState(true);

    useEffect(() => {
        if (!open) return;

        const fetchMajors = async () => {
            setLoadingMajors(true);
            try {
                const response = await getApiMajor({ query: { Page: 1, PageSize: 1000 } });
                const data = (response as unknown as { data: { items?: MajorDetailDto[] } })?.data || response as { items?: MajorDetailDto[] };
                setMajors(data?.items || []);
            } catch (err) {
                console.error("Error loading majors:", err);
            } finally {
                setLoadingMajors(false);
            }
        };

        fetchMajors();
    }, [open]);

    const handleSubmit = async () => {
        if (!selectedMajorId) {
            notifyError("Vui lòng chọn ngành học của bạn.");
            return;
        }

        setLoading(true);
        try {
            await putApiAccountProfile<true>({
                body: {
                    majorId: selectedMajorId,
                    introduction: introduction.trim() || null
                },
                throwOnError: true
            });
            notifySuccess("Cập nhật thông tin thành công!");
            onComplete();
        } catch (err: any) {
            const msg = err?.response?.data?.message || err?.message || "Không thể cập nhật thông tin";
            notifyError(msg);
        } finally {
            setLoading(false);
        }
    };

    return (
        <Dialog open={open} onOpenChange={() => { }}>
            <DialogContent className="max-w-md p-6" onPointerDownOutside={(e) => e.preventDefault()} onEscapeKeyDown={(e) => e.preventDefault()}>
                <DialogHeader>
                    <DialogTitle className="flex items-center gap-2">
                        <GraduationCap className="h-5 w-5 text-primary" />
                        Hoàn tất hồ sơ
                    </DialogTitle>
                    <DialogDescription>
                        Vui lòng hoàn tất thông tin để tiếp tục sử dụng hệ thống.
                    </DialogDescription>
                </DialogHeader>

                <div className="mt-4 space-y-4">
                    <div>
                        <label className="text-sm font-medium text-foreground">
                            Ngành học <span className="text-red-500">*</span>
                        </label>
                        <select
                            className="mt-1 flex h-10 w-full border border-input bg-background px-3 py-2 text-sm shadow-sm transition-colors focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-50"
                            value={selectedMajorId}
                            onChange={(e) => setSelectedMajorId(e.target.value)}
                            disabled={loadingMajors || loading}
                        >
                            <option value="">
                                {loadingMajors ? "Đang tải..." : "-- Chọn ngành học --"}
                            </option>
                            {majors.map((major) => (
                                <option key={major.id} value={major.id}>
                                    {major.majorName} ({major.majorCode})
                                </option>
                            ))}
                        </select>
                    </div>

                    <div>
                        <label className="text-sm font-medium text-foreground">
                            Giới thiệu bản thân <span className="text-muted-foreground text-xs">(tùy chọn)</span>
                        </label>
                        <textarea
                            className="mt-1 flex min-h-[80px] w-full border border-input bg-background px-3 py-2 text-sm shadow-sm transition-colors focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-50 resize-none"
                            placeholder="Giới thiệu ngắn gọn về bản thân..."
                            value={introduction}
                            onChange={(e) => setIntroduction(e.target.value)}
                            disabled={loading}
                            maxLength={500}
                        />
                        <p className="mt-1 text-xs text-muted-foreground text-right">
                            {introduction.length}/500
                        </p>
                    </div>

                    <Button
                        type="button"
                        className="w-full h-11"
                        onClick={handleSubmit}
                        disabled={!selectedMajorId || loading || loadingMajors}
                    >
                        {loading ? (
                            <>
                                <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                                Đang lưu...
                            </>
                        ) : (
                            "Xác nhận"
                        )}
                    </Button>

                    <p className="text-xs text-muted-foreground text-center">
                        Thông tin này giúp chúng tôi đề xuất nội dung phù hợp với bạn.
                    </p>
                </div>
            </DialogContent>
        </Dialog>
    );
}
