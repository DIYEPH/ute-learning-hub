namespace UteLearningHub.Domain.Constaints.Enums;

/// <summary>
/// Status for user-generated content (DocumentFile, Comment)
/// </summary>
public enum ContentStatus
{
    /// <summary>
    /// Chờ duyệt (nếu user trust level thấp)
    /// </summary>
    PendingReview,

    /// <summary>
    /// Đã duyệt, hiển thị bình thường
    /// </summary>
    Approved,

    /// <summary>
    /// Bị ẩn do vi phạm (report được approve)
    /// </summary>
    Hidden
}
