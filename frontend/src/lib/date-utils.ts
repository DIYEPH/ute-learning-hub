export function getRelativeTime(date: Date): string {
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffSec = Math.floor(diffMs / 1000);
    const diffMin = Math.floor(diffSec / 60);
    const diffHour = Math.floor(diffMin / 60);
    const diffDay = Math.floor(diffHour / 24);
    const diffWeek = Math.floor(diffDay / 7);

    if (diffSec < 60) return "Vừa xong";
    if (diffMin < 60) return `${diffMin} phút`;
    if (diffHour < 24) return `${diffHour} giờ`;
    if (diffDay === 1) return "Hôm qua";
    if (diffDay < 7) return `${diffDay} ngày`;
    if (diffWeek < 4) return `${diffWeek} tuần`;
    return date.toLocaleDateString("vi-VN");
}