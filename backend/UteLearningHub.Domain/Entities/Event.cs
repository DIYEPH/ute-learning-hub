namespace UteLearningHub.Domain.Entities;

using UteLearningHub.Domain.Entities.Base;

public class Event : BaseEntity<Guid>, IAggregateRoot
{
    public string Title { get; set; } = default!;
    public string? ShortDescription { get; set; }
    public string? Content { get; set; }              // Có thể là HTML/Markdown
    public string? ImageUrl { get; set; }            // Banner
    public string? RedirectUrl { get; set; }         // Link khi click
    public DateTimeOffset StartAt { get; set; }
    public DateTimeOffset EndAt { get; set; }
    public bool IsActive { get; set; } = true;
    public int Priority { get; set; } = 0;           // Ưu tiên hiển thị
}