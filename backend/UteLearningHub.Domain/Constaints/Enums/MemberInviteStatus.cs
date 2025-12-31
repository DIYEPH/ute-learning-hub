namespace UteLearningHub.Domain.Constaints.Enums;

/// <summary>
/// Trạng thái lời mời tham gia nhóm (dùng cho Proposal)
/// </summary>
public enum MemberInviteStatus
{
    Joined,     // Đã là thành viên (nhóm bình thường)
    Pending,    // Chờ phản hồi (proposal)
    Accepted,   // Đã đồng ý tham gia (proposal)
    Declined    // Từ chối tham gia (proposal)
}

