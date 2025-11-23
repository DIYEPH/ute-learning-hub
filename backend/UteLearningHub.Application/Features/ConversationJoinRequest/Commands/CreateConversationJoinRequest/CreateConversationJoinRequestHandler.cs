using MediatR;
using Microsoft.EntityFrameworkCore;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;
using DomainConversationJoinRequest = UteLearningHub.Domain.Entities.ConversationJoinRequest;

namespace UteLearningHub.Application.Features.ConversationJoinRequest.Commands.CreateConversationJoinRequest;

public class CreateConversationJoinRequestHandler : IRequestHandler<CreateConversationJoinRequestCommand, ConversationJoinRequestDto>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IIdentityService _identityService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public CreateConversationJoinRequestHandler(
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

    public async Task<ConversationJoinRequestDto> Handle(CreateConversationJoinRequestCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated)
            throw new UnauthorizedException("You must be authenticated to create join request");

        var userId = _currentUserService.UserId ?? throw new UnauthorizedException();

        // Verify conversation exists and is active
        var conversation = await _conversationRepository.GetByIdAsync(request.ConversationId, disableTracking: true, cancellationToken);
        if (conversation == null || conversation.IsDeleted)
            throw new NotFoundException($"Conversation with id {request.ConversationId} not found");

        if (conversation.ConversationStatus != ConversationStatus.Active)
            throw new BadRequestException("Conversation is not active");

        // Check if user is already a member
        var isMember = await _conversationRepository.GetQueryableSet()
            .Where(c => c.Id == request.ConversationId)
            .SelectMany(c => c.Members)
            .AnyAsync(m => m.UserId == userId && !m.IsDeleted, cancellationToken);

        if (isMember)
            throw new BadRequestException("You are already a member of this conversation");

        // Check if user already has a pending request
        var existingRequest = await _conversationRepository.GetJoinRequestsQueryable()
            .Where(r => r.ConversationId == request.ConversationId 
                     && r.CreatedById == userId 
                     && r.ReviewStatus == ReviewStatus.PendingReview
                     && !r.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        if (existingRequest != null)
            throw new BadRequestException("You already have a pending join request for this conversation");

        // Create join request
        var joinRequest = new DomainConversationJoinRequest
        {
            Id = Guid.NewGuid(),
            ConversationId = request.ConversationId,
            Content = request.Content,
            ReviewStatus = ReviewStatus.PendingReview,
            CreatedById = userId,
            CreatedAt = _dateTimeProvider.OffsetNow
        };

        // Add through conversation's navigation property
        conversation = await _conversationRepository.GetByIdWithDetailsAsync(request.ConversationId, disableTracking: false, cancellationToken);
        if (conversation == null)
            throw new NotFoundException($"Conversation with id {request.ConversationId} not found");

        conversation.ConversationJoinRequests.Add(joinRequest);
        await _conversationRepository.UpdateAsync(conversation, cancellationToken);
        await _conversationRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        // Reload to get relationships
        joinRequest = await _conversationRepository.GetJoinRequestsQueryable()
            .Include(r => r.Conversation)
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == joinRequest.Id, cancellationToken);

        if (joinRequest == null)
            throw new NotFoundException("Failed to create join request");

        // Get requester info
        var requester = await _identityService.FindByIdAsync(userId);
        if (requester == null)
            throw new UnauthorizedException();

        return new ConversationJoinRequestDto
        {
            Id = joinRequest.Id,
            ConversationId = joinRequest.ConversationId,
            ConversationName = joinRequest.Conversation.ConversationName,
            Content = joinRequest.Content,
            RequesterName = requester.FullName,
            RequesterAvatarUrl = requester.AvatarUrl,
            CreatedById = joinRequest.CreatedById,
            ReviewStatus = joinRequest.ReviewStatus,
            ReviewNote = joinRequest.ReviewNote,
            ReviewedAt = joinRequest.ReviewedAt,
            CreatedAt = joinRequest.CreatedAt
        };
    }
}