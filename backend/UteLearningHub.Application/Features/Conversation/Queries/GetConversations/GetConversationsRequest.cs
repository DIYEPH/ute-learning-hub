using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Domain.Constaints.Enums;

namespace UteLearningHub.Application.Features.Conversation.Queries.GetConversations;

public record GetConversationsRequest : PagedRequest
{
    public Guid? SubjectId { get; init; }
    public Guid? TagId { get; init; } 
    public ConversitionType? ConversationType { get; init; }
    public ConversationStatus? ConversationStatus { get; init; }
    public Guid? CreatedById { get; init; }
    public Guid? MemberId { get; init; } 
    public string? SearchTerm { get; init; }
    public string? SortBy { get; init; }
    public bool SortDescending { get; init; } = true;
}
