using MediatR;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Application.Services.Message;
using UteLearningHub.Application.Services.Recommendation;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Entities;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;

namespace UteLearningHub.Application.Features.Conversation.Commands.JoinConversation;

public class JoinConversationHandler : IRequestHandler<JoinConversationCommand, ConversationDetailDto>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IIdentityService _identityService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IConversationSystemMessageService _systemMessageService;
    private readonly IVectorMaintenanceService _vectorMaintenanceService;

    public JoinConversationHandler(
        IConversationRepository conversationRepository,
        IIdentityService identityService,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider,
        IConversationSystemMessageService systemMessageService,
        IVectorMaintenanceService vectorMaintenanceService)
    {
        _conversationRepository = conversationRepository;
        _identityService = identityService;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
        _systemMessageService = systemMessageService;
        _vectorMaintenanceService = vectorMaintenanceService;
    }

    public async Task<ConversationDetailDto> Handle(JoinConversationCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated)
            throw new UnauthorizedException("You must be authenticated to join a conversation");

        var userId = _currentUserService.UserId ?? throw new UnauthorizedException();

        // Get conversation with members
        var conversation = await _conversationRepository.GetByIdWithDetailsAsync(
            request.ConversationId,
            disableTracking: false,
            cancellationToken);

        if (conversation == null || conversation.IsDeleted)
            throw new NotFoundException($"Conversation with id {request.ConversationId} not found");

        if (conversation.ConversationStatus != ConversationStatus.Active)
            throw new BadRequestException("Conversation is not active");

        var isActiveMember = conversation.Members.Any(m => m.UserId == userId && !m.IsDeleted);
        if (isActiveMember)
            throw new BadRequestException("You are already a member of this conversation");

        if (conversation.ConversationType == ConversitionType.Private)
            throw new BadRequestException("Private conversations require a join request. Please use the join request endpoint.");

        var deletedMember = await _conversationRepository.GetDeletedMemberAsync(conversation.Id, userId, cancellationToken);

        if (deletedMember != null)
        {
            deletedMember.ConversationMemberRoleType = ConversationMemberRoleType.Member;
            deletedMember.IsMuted = false;
            await _conversationRepository.RestoreMemberAsync(deletedMember, cancellationToken);
        }
        else
        {
            var newMember = new ConversationMember
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ConversationId = conversation.Id,
                ConversationMemberRoleType = ConversationMemberRoleType.Member,
                IsMuted = false
            };

            await _conversationRepository.AddMemberAsync(newMember, cancellationToken);
        }

        await _conversationRepository.UnitOfWork.SaveChangesAsync(cancellationToken);
        await _systemMessageService.CreateAsync(
            conversation.Id,
            userId,
            MessageType.MemberJoined,
            null,
            cancellationToken);

        // Cập nhật user vector (async, không block response)
        _ = Task.Run(async () =>
        {
            try
            {
                await _vectorMaintenanceService.UpdateUserVectorAsync(userId, cancellationToken);
            }
            catch
            {
                // Log error nhưng không throw - vector sẽ được update bởi background job
            }
        }, cancellationToken);

        var updatedConversation = await _conversationRepository.GetByIdWithDetailsAsync(
            conversation.Id,
            disableTracking: true,
            cancellationToken);

        if (updatedConversation == null)
            throw new NotFoundException("Failed to join conversation");

        var creator = await _identityService.FindByIdAsync(updatedConversation.CreatedById);
        if (creator == null)
            throw new NotFoundException("Creator not found");

        var memberUserIds = updatedConversation.Members
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
            Id = updatedConversation.Id,
            ConversationName = updatedConversation.ConversationName,
            Tags = updatedConversation.ConversationTags
                .Select(ct => new TagDto
                {
                    Id = ct.Tag.Id,
                    TagName = ct.Tag.TagName
                })
                .ToList(),
            ConversationType = updatedConversation.ConversationType,
            ConversationStatus = updatedConversation.ConversationStatus,
            IsSuggestedByAI = updatedConversation.IsSuggestedByAI,
            IsAllowMemberPin = updatedConversation.IsAllowMemberPin,
            Subject = updatedConversation.Subject != null ? new SubjectDto
            {
                Id = updatedConversation.Subject.Id,
                SubjectName = updatedConversation.Subject.SubjectName,
                SubjectCode = updatedConversation.Subject.SubjectCode
            } : null,
            Members = updatedConversation.Members
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
            LastMessageId = updatedConversation.LastMessage,
            CreatedById = updatedConversation.CreatedById,
            CreatedAt = updatedConversation.CreatedAt,
            UpdatedAt = updatedConversation.UpdatedAt
        };
    }
}

