using MediatR;
using Microsoft.EntityFrameworkCore;
using UteLearningHub.Application.Services.Message;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;

namespace UteLearningHub.Application.Features.Conversation.Queries.GetOnlineMembers;

public class GetOnlineMembersHandler : IRequestHandler<GetOnlineMembersQuery, GetOnlineMembersResponse>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IConnectionTracker _connectionTracker;

    public GetOnlineMembersHandler(
        IConversationRepository conversationRepository,
        IConnectionTracker connectionTracker)
    {
        _conversationRepository = conversationRepository;
        _connectionTracker = connectionTracker;
    }

    public async Task<GetOnlineMembersResponse> Handle(GetOnlineMembersQuery request, CancellationToken cancellationToken)
    {
        var conversation = await _conversationRepository.GetByIdWithDetailsAsync(
            request.ConversationId,
            disableTracking: true,
            cancellationToken);

        if (conversation == null || conversation.IsDeleted)
            throw new NotFoundException($"Conversation with id {request.ConversationId} not found");

        // Get all member user IDs
        var memberUserIds = conversation.Members
            .Where(m => !m.IsDeleted)
            .Select(m => m.UserId)
            .Distinct()
            .ToList();

        // Filter online users
        var onlineUserIds = _connectionTracker.GetOnlineUsers(memberUserIds).ToList();

        return new GetOnlineMembersResponse
        {
            ConversationId = request.ConversationId,
            OnlineUserIds = onlineUserIds
        };
    }
}
