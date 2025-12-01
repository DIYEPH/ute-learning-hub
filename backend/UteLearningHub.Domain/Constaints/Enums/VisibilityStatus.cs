namespace UteLearningHub.Domain.Constaints.Enums;

public enum VisibilityStatus
{
    Public,   // Tài liệu công khai ai cũng xem được
    Private,  // Chỉ người tạo (hoặc được chia sẻ cụ thể) xem được
    Internal  // Tài liệu nội bộ: chỉ người dùng đã đăng nhập trong hệ thống xem được
}
