namespace UteLearningHub.Domain.Constaints.Enums;

/// <summary>
/// Loại thông báo trong hệ thống
/// </summary>
public enum NotificationType
{
    /// <summary>Thông báo hệ thống tự động (thăng hạng, báo cáo được duyệt...)</summary>
    System = 0,

    /// <summary>Tin nhắn mới trong conversation</summary>
    Message = 1,

    /// <summary>Comment mới trên document</summary>
    Comment = 2,

    /// <summary>Document được approve/reject/publish</summary>
    Document = 3,

    /// <summary>Hành động liên quan đến user (follow, like...)</summary>
    UserAction = 4,

    /// <summary>Conversation: mời vào nhóm, yêu cầu tham gia được duyệt...</summary>
    Conversation = 5,

    /// <summary>Event: sự kiện mới, sắp diễn ra (tự động khi tạo event)</summary>
    Event = 6,

    /// <summary>Thông báo từ Admin (admin tạo thủ công)</summary>
    AdminNote = 7
}
