namespace UteLearningHub.Domain.Constaints.Enums;

/// <summary>
/// Loại entity liên quan đến thay đổi điểm uy tín
/// </summary>
public enum TrustEntityType
{
    /// <summary>Upload file tài liệu, like/dislike</summary>
    DocumentFile = 0,
    
    /// <summary>Báo cáo vi phạm được duyệt</summary>
    Report = 1,
    
    /// <summary>Bình luận (tương lai)</summary>
    Comment = 2,
    
    /// <summary>Admin điều chỉnh thủ công</summary>
    Manual = 3
}
