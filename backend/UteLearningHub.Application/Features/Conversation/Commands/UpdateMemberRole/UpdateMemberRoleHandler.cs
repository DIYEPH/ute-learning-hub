using MediatR;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Application.Services.Message;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;

namespace UteLearningHub.Application.Features.Conversation.Commands.UpdateMemberRole;

public class UpdateMemberRoleHandler : IRequestHandler<UpdateMemberRoleCommand>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IIdentityService _identityService;
    private readonly IConversationSystemMessageService _systemMessageService;

    public UpdateMemberRoleHandler(
        IConversationRepository conversationRepository,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider,
        IIdentityService identityService,
        IConversationSystemMessageService systemMessageService)
    {
        _conversationRepository = conversationRepository;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
        _identityService = identityService;
        _systemMessageService = systemMessageService;
    }

    public async Task Handle(UpdateMemberRoleCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated)
            throw new UnauthorizedException("You must be authenticated to update member roles");

        var userId = _currentUserService.UserId ?? throw new UnauthorizedException();

        var conversation = await _conversationRepository.GetByIdWithDetailsAsync(
            request.ConversationId,
            disableTracking: false,
            cancellationToken);

        if (conversation == null || conversation.IsDeleted)
            throw new NotFoundException($"Conversation with id {request.ConversationId} not found");

        var currentUserMember = conversation.Members.FirstOrDefault(m =>
            m.UserId == userId && !m.IsDeleted);

        if (currentUserMember == null)
            throw new ForbidenException("You are not a member of this conversation");

        if (currentUserMember.ConversationMemberRoleType != ConversationMemberRoleType.Owner)
            throw new UnauthorizedException("Only owners can update member roles");

        var targetMember = conversation.Members.FirstOrDefault(m =>
            m.Id == request.MemberId && !m.IsDeleted);

        if (targetMember == null)
            throw new NotFoundException($"Member with id {request.MemberId} not found");

        if (targetMember.UserId == userId)
            throw new BadRequestException("You cannot change your own role");

        if (targetMember.ConversationMemberRoleType == ConversationMemberRoleType.Owner)
            throw new BadRequestException("Cannot change owner's role");

        if (request.RoleType == ConversationMemberRoleType.Owner)
        {
            if (targetMember.ConversationMemberRoleType != ConversationMemberRoleType.Deputy)
                throw new BadRequestException("Can only promote Deputy to Owner. Please promote to Deputy first.");
        }

        targetMember.ConversationMemberRoleType = request.RoleType;
        targetMember.UpdatedAt = _dateTimeProvider.OffsetNow;

        await _conversationRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        await _systemMessageService.CreateAsync(
            conversation.Id,
            userId,
            MessageType.MemberRoleUpdated,
            targetMember.UserId,
            cancellationToken);
    }
}