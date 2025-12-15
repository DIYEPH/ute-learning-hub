"use client";

export default function AboutPage() {
    return (
        <div className="max-w-xl mx-auto py-8 px-4">
            <h1 className="text-xl font-bold mb-4">Hệ thống điểm</h1>

            <div className="space-y-3 text-sm">
                <p>-<b>5 điểm</b> - Tân Binh Học Thuật</p>
                <p>-<b>15 điểm</b> - Cộng Tác Viên: Được tạo nhóm</p>
                <p>-<b>60 điểm</b> - Người Thẩm Định: Báo cáo được ưu tiên xử lý</p>
                <p>-<b>120 điểm</b> -Người Hướng Dẫn: Được quyền duyệt báo cáo và tài liệu</p>
            </div>

            <p className="text-xs text-slate-500 mt-6 italic">
                Upload tài liệu +5đ, được đánh giá hữu ích +1đ, báo cáo đúng +2đ.
            </p>
        </div>
    );
}
