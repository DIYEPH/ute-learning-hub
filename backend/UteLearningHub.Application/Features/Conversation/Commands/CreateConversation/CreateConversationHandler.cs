using MediatR;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;
using UteLearningHub.Domain.Entities;
using DomainConversation = UteLearningHub.Domain.Entities.Conversation;

namespace UteLearningHub.Application.Features.Conversation.Commands.CreateConversation;

public class CreateConversationHandler : IRequestHandler<CreateConversationCommand, ConversationDetailDto>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IIdentityService _identityService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public CreateConversationHandler(
        IConversationRepository conversationRepository,
        IIdentityService identityService,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider)
    {
        _conversationRepository = conversationRepository;
        _identityService = identityService;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<ConversationDetailDto> Handle(CreateConversationCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated)
            throw new UnauthorizedException("You must be authenticated to create a conversation");

        var userId = _currentUserService.UserId ?? throw new UnauthorizedException();

        // Verify subject exists if provided
        if (request.SubjectId.HasValue)
        {
            // You may want to add ISubjectRepository to verify
            // For now, we'll skip this check or add it later
        }

        var conversation = new DomainConversation
        {
            Id = Guid.NewGuid(),
            ConversationName = request.ConversationName,
            Topic = request.Topic,
            ConversationType = request.ConversationType,
            ConversationStatus = ConversationStatus.Active,
            SubjectId = request.SubjectId,
            IsSuggestedByAI = request.IsSuggestedByAI,
            IsAllowMemberPin = request.IsAllowMemberPin,
            CreatedById = userId,
            CreatedAt = _dateTimeProvider.OffsetNow
        };

        // Add creator as owner
        var ownerMember = new ConversationMember
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ConversationId = conversation.Id,
            ConversationMemberRoleType = ConversationMemberRoleType.Owner,
            IsMuted = false,
            CreatedAt = _dateTimeProvider.OffsetNow
        };

        conversation.Members.Add(ownerMember);

        await _conversationRepository.AddAsync(conversation, cancellationToken);
        await _conversationRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        // Reload with details
        var createdConversation = await _conversationRepository.GetByIdWithDetailsAsync(
            conversation.Id,
            disableTracking: true,
            cancellationToken);

        if (createdConversation == null)
            throw new NotFoundException("Failed to create conversation");

        // Get creator information
        var creator = await _identityService.FindByIdAsync(userId);
        if (creator == null)
            throw new UnauthorizedException();

        // Get member information
        var memberUserIds = createdConversation.Members
            .Where(m => !m.IsDeleted)
            .Select(m => m.UserId)
            .Distinct()
            .ToList();

        var memberInfo = new Dictionary<Guid, (string FullName, string? AvatarUrl)>();

        foreach (var memberUserId in memberUserIds)
        {
            var user = await _identityService.FindByIdAsync(memberUserId);
            if (user != null)
            {
                memberInfo[memberUserId] = (user.FullName, user.AvatarUrl);
            }
        }

        return new ConversationDetailDto
        {
            Id = createdConversation.Id,
            ConversationName = createdConversation.ConversationName,
            Topic = createdConversation.Topic,
            ConversationType = createdConversation.ConversationType,
            ConversationStatus = createdConversation.ConversationStatus,
            IsSuggestedByAI = createdConversation.IsSuggestedByAI,
            IsAllowMemberPin = createdConversation.IsAllowMemberPin,
            Subject = createdConversation.Subject != null ? new SubjectDto
            {
                Id = createdConversation.Subject.Id,
                SubjectName = createdConversation.Subject.SubjectName,
                SubjectCode = createdConversation.Subject.SubjectCode
            } : null,
            CreatorName = creator.FullName,
            CreatorAvatarUrl = creator.AvatarUrl,
            Members = createdConversation.Members
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
            LastMessageId = createdConversation.LastMessage,
            CreatedById = createdConversation.CreatedById,
            CreatedAt = createdConversation.CreatedAt,
            UpdatedAt = createdConversation.UpdatedAt
        };
    }
}