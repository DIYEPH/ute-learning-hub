namespace UteLearningHub.Domain.Constaints.Enums;

public enum TrustLever
{
    None = 0,            // Mặc định cho người mới
    Newbie = 1,          // Người mới tham gia
    Contributor = 2,     // Đã có hoạt động đóng góp
    TrustedMember = 3,   // Có tài liệu được duyệt, trust cao
    Moderator = 4,       // Có quyền xét duyệt báo cáo, tài liệu
    Master = 5           // Cấp cao nhất, gần admin
}
