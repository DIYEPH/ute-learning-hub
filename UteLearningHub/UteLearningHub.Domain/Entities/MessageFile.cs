using UteLearningHub.Domain.Entities.Base;

namespace UteLearningHub.Domain.Entities;

public class MessageFile
{
    public Guid FileId { get; set; }
    public Guid MessageId { get; set; }
    public File File { get; set; } = default!;
    public Message Message { get; set; } = default!;
}
