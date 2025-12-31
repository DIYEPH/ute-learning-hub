export function getProfileLink(targetUserId: string | undefined | null, currentUserId?: string): string {
    if (!targetUserId) return "/profile";
    if (currentUserId && targetUserId === currentUserId) return "/profile";
    return `/profile/${targetUserId}`;
}
