namespace UteLearningHub.Domain.Constaints.Enums;

/// <summary>
/// Lý do báo cáo vi phạm
/// </summary>
public enum ReportReason
{
    /// <summary>
    /// Khác (mô tả chi tiết trong nội dung)
    /// </summary>
    Other = 0,

    /// <summary>
    /// Vi phạm bản quyền
    /// </summary>
    Copyright = 1,

    /// <summary>
    /// Nội dung xúc phạm, lăng mạ
    /// </summary>
    Offensive = 2,

    /// <summary>
    /// Nội dung spam, quảng cáo
    /// </summary>
    Spam = 3,

    /// <summary>
    /// Thông tin sai lệch, gây hiểu nhầm
    /// </summary>
    Misinformation = 4,

    /// <summary>
    /// Nội dung bạo lực, ghê rợn
    /// </summary>
    Violence = 5,

    /// <summary>
    /// Nội dung khiêu dâm, không phù hợp
    /// </summary>
    Inappropriate = 6,

    /// <summary>
    /// Quấy rối, bắt nạt
    /// </summary>
    Harassment = 7,

    /// <summary>
    /// Đạo văn, sao chép
    /// </summary>
    Plagiarism = 8
}
