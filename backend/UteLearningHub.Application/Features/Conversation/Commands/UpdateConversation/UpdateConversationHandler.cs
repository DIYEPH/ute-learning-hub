using MediatR;
using Microsoft.EntityFrameworkCore;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;

namespace UteLearningHub.Application.Features.Conversation.Commands.UpdateConversation;

public class UpdateConversationHandler : IRequestHandler<UpdateConversationCommand, ConversationDetailDto>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IIdentityService _identityService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public UpdateConversationHandler(
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

    public async Task<ConversationDetailDto> Handle(UpdateConversationCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated)
            throw new UnauthorizedException("You must be authenticated to update a conversation");

        var userId = _currentUserService.UserId ?? throw new UnauthorizedException();

        var conversation = await _conversationRepository.GetByIdWithDetailsAsync(
            request.Id,
            disableTracking: false,
            cancellationToken);

        if (conversation == null || conversation.IsDeleted)
            throw new NotFoundException($"Conversation with id {request.Id} not found");

        // Check permission: Admin or Owner
        var isAdmin = _currentUserService.IsInRole("Admin");
        var isOwner = conversation.Members.Any(m =>
            m.UserId == userId &&
            m.ConversationMemberRoleType == ConversationMemberRoleType.Owner &&
            !m.IsDeleted);

        if (!isAdmin && !isOwner)
            throw new UnauthorizedException("Only administrators or conversation owners can update conversations");

        // Update fields
        if (!string.IsNullOrWhiteSpace(request.ConversationName))
            conversation.ConversationName = request.ConversationName;

        if (!string.IsNullOrWhiteSpace(request.Topic))
            conversation.Topic = request.Topic;

        if (request.ConversationStatus.HasValue)
            conversation.ConversationStatus = request.ConversationStatus.Value;

        if (request.SubjectId.HasValue)
            conversation.SubjectId = request.SubjectId;

        if (request.IsAllowMemberPin.HasValue)
            conversation.IsAllowMemberPin = request.IsAllowMemberPin.Value;

        conversation.UpdatedById = userId;
        conversation.UpdatedAt = _dateTimeProvider.OffsetNow;

        await _conversationRepository.UpdateAsync(conversation, cancellationToken);
        await _conversationRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        // Reload with details
        var updatedConversation = await _conversationRepository.GetByIdWithDetailsAsync(
            request.Id,
            disableTracking: true,
            cancellationToken);

        if (updatedConversation == null)
            throw new NotFoundException("Failed to update conversation");

        // Get creator information
        var creator = await _identityService.FindByIdAsync(updatedConversation.CreatedById);
        if (creator == null)
            throw new NotFoundException("Creator not found");

        // Get member information
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
            Topic = updatedConversation.Topic,
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
            CreatorName = creator.FullName,
            CreatorAvatarUrl = creator.AvatarUrl,
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
