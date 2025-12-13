using MediatR;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;

namespace UteLearningHub.Application.Features.Conversation.Queries.GetConversationById;

public class GetConversationByIdHandler : IRequestHandler<GetConversationByIdQuery, ConversationDetailDto>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IIdentityService _identityService;

    public GetConversationByIdHandler(
        IConversationRepository conversationRepository,
        IIdentityService identityService)
    {
        _conversationRepository = conversationRepository;
        _identityService = identityService;
    }

    public async Task<ConversationDetailDto> Handle(GetConversationByIdQuery request, CancellationToken cancellationToken)
    {
        var conversation = await _conversationRepository.GetByIdWithDetailsAsync(
            request.Id,
            disableTracking: true,
            cancellationToken);

        if (conversation == null || conversation.IsDeleted)
            throw new NotFoundException($"Conversation with id {request.Id} not found");

        // Get creator information
        var creator = await _identityService.FindByIdAsync(conversation.CreatedById);
        if (creator == null)
            throw new NotFoundException("Creator not found");

        // Get member information
        var memberUserIds = conversation.Members
            .Where(m => !m.IsDeleted)
            .Select(m => m.UserId)
            .Distinct()
            .ToList();

        var memberInfo = new Dictionary<Guid, (string FullName, string? AvatarUrl)>();

        foreach (var userId in memberUserIds)
        {
            var user = await _identityService.FindByIdAsync(userId);
            if (user != null)
            {
                memberInfo[userId] = (user.FullName, user.AvatarUrl);
            }
        }

        return new ConversationDetailDto
        {
            Id = conversation.Id,
            ConversationName = conversation.ConversationName,
            Tags = conversation.ConversationTags
                .Select(ct => new TagDto
                {
                    Id = ct.Tag.Id,
                    TagName = ct.Tag.TagName
                })
                .ToList(),
            ConversationType = conversation.ConversationType,
            Visibility = conversation.Visibility,
            ConversationStatus = conversation.ConversationStatus,
            IsSuggestedByAI = conversation.IsSuggestedByAI,
            IsAllowMemberPin = conversation.IsAllowMemberPin,
            Subject = conversation.Subject != null ? new SubjectDto
            {
                Id = conversation.Subject.Id,
                SubjectName = conversation.Subject.SubjectName,
                SubjectCode = conversation.Subject.SubjectCode
            } : null,
            AvatarUrl = conversation.AvatarUrl,
            Members = conversation.Members
                .Where(m => !m.IsDeleted)
                .Select(m => new ConversationMemberDto
                {
                    Id = m.Id,
                    UserId = m.UserId,
                    UserName = memberInfo.TryGetValue(m.UserId, out var info)
                        ? info.FullName
                        : "Unknown",
                    UserAvatarUrl = memberInfo.TryGetValue(m.UserId, out var member)
                        ? member.AvatarUrl
                        : null,
                    RoleType = m.ConversationMemberRoleType,
                    IsMuted = m.IsMuted,
                    JoinedAt = m.CreatedAt
                }).ToList(),
            MessageCount = 0,
            LastMessageId = conversation.LastMessage,
            CreatedById = conversation.CreatedById,
            CreatedAt = conversation.CreatedAt,
            UpdatedAt = conversation.UpdatedAt
        };
    }
}