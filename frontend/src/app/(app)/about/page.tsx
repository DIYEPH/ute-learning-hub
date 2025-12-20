"use client";

export default function AboutPage() {
    return (
        <div className="max-w-xl mx-auto py-8 px-4">
            <h1 className="text-xl font-bold mb-4">Hệ thống điểm</h1>

            <div className="space-y-3 text-sm">
                <p>-<b>5 điểm</b> - Newbie</p>
                <p>-<b>10 điểm</b> - Contribute</p>
                <p>-<b>30 điểm</b> - TrustMember</p>
                <p>-<b>Trên 60 điểm</b> -Moderator</p>
            </div>

            <p className="text-xs text-slate-500 mt-6 italic">
                Upload tài liệu +5đ, được đánh giá hữu ích +1đ, báo cáo đúng +3,2,1đ.
            </p>
        </div>
    );
}
