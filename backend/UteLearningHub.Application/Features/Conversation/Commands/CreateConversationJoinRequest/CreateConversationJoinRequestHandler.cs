using MediatR;
using Microsoft.EntityFrameworkCore;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;
using DomainConversationJoinRequest = UteLearningHub.Domain.Entities.ConversationJoinRequest;

namespace UteLearningHub.Application.Features.Conversation.Commands.CreateConversationJoinRequest;

public class CreateConversationJoinRequestHandler(
    IConversationRepository conversationRepository,
    IIdentityService identityService,
    ICurrentUserService currentUserService,
    IDateTimeProvider dateTimeProvider) : IRequestHandler<CreateConversationJoinRequestCommand, ConversationJoinRequestDto>
{
    public async Task<ConversationJoinRequestDto> Handle(CreateConversationJoinRequestCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedException("You must be authenticated to create join request");

        var userId = currentUserService.UserId ?? throw new UnauthorizedException();

        var conversation = await conversationRepository.GetByIdAsync(request.ConversationId, disableTracking: true, cancellationToken);
        if (conversation == null)
            throw new NotFoundException($"Conversation with id {request.ConversationId} not found");

        if (conversation.ConversationStatus != ConversationStatus.Active)
            throw new BadRequestException("Conversation is not active");

        // Chỉ nhóm Private mới cần join request
        if (conversation.Visibility != ConversationVisibility.Private)
            throw new BadRequestException("Join requests are only available for private conversations");

        // Kiểm tra đã là member chưa
        var isMember = await conversationRepository.GetQueryableSet()
            .Where(c => c.Id == request.ConversationId)
            .SelectMany(c => c.Members)
            .AnyAsync(m => m.UserId == userId && !m.IsDeleted, cancellationToken);

        if (isMember)
            throw new BadRequestException("You are already a member of this conversation");

        // Kiểm tra đã có request pending chưa
        var existingRequest = await conversationRepository.GetJoinRequestsQueryable()
            .Where(r => r.ConversationId == request.ConversationId
                     && r.CreatedById == userId
                     && r.Status == ContentStatus.PendingReview
                     && !r.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        if (existingRequest != null)
            throw new BadRequestException("You already have a pending join request for this conversation");

        var joinRequest = new DomainConversationJoinRequest
        {
            Id = Guid.NewGuid(),
            ConversationId = request.ConversationId,
            Content = request.Content,
            Status = ContentStatus.PendingReview,
            CreatedById = userId,
            CreatedAt = dateTimeProvider.OffsetNow
        };

        await conversationRepository.AddJoinRequestAsync(joinRequest, cancellationToken);
        await conversationRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        // Reload để lấy relationship
        joinRequest = await conversationRepository.GetJoinRequestsQueryable()
            .Include(r => r.Conversation)
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == joinRequest.Id, cancellationToken);

        if (joinRequest == null)
            throw new NotFoundException("Failed to create join request");

        var requester = await identityService.FindByIdAsync(userId);
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
            Status = joinRequest.Status,
            ReviewNote = joinRequest.ReviewNote,
            CreatedAt = joinRequest.CreatedAt
        };
    }
}