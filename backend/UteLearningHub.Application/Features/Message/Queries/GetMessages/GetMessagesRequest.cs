namespace UteLearningHub.Application.Features.Message.Queries.GetMessages;

public record GetMessagesRequest
{
    public Guid ConversationId { get; init; }
    public Guid? ParentId { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 50;

    public int Skip => (Page - 1) * PageSize;
    public int Take => PageSize;
}