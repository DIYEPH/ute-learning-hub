using MediatR;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Application.Services.Message;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;

namespace UteLearningHub.Application.Features.Message.Commands.PinMessage;

public class PinMessageCommandHandler : IRequestHandler<PinMessageCommand, Unit>
{
    private readonly IMessageRepository _messageRepository;
    private readonly IConversationRepository _conversationRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IIdentityService _identityService;
    private readonly IConversationSystemMessageService _systemMessageService;

    public PinMessageCommandHandler(
        IMessageRepository messageRepository,
        IConversationRepository conversationRepository,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider,
        IIdentityService identityService,
        IConversationSystemMessageService systemMessageService)
    {
        _messageRepository = messageRepository;
        _conversationRepository = conversationRepository;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
        _identityService = identityService;
        _systemMessageService = systemMessageService;
    }

    public async Task<Unit> Handle(PinMessageCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated)
            throw new UnauthorizedException("You must be authenticated to pin messages");

        var userId = _currentUserService.UserId ?? throw new UnauthorizedException();

        var message = await _messageRepository.GetByIdAsync(
            request.Id, 
            disableTracking: false, 
            cancellationToken);

        if (message == null || message.IsDeleted)
            throw new NotFoundException($"Message with id {request.Id} not found");

        if (message.ConversationId != request.ConversationId)
            throw new BadRequestException("Message does not belong to the specified conversation");
            
        var conversation = await _conversationRepository.GetByIdWithDetailsAsync(
            request.ConversationId, 
            disableTracking: false, 
            cancellationToken);

        if (conversation == null || conversation.IsDeleted)
            throw new NotFoundException("Conversation not found");

        if (conversation.ConversationStatus != ConversationStatus.Active)
            throw new BadRequestException("Conversation is not active");

        var member = conversation.Members.FirstOrDefault(m =>
            m.UserId == userId && !m.IsDeleted);

        if (member == null)
            throw new ForbidenException("You are not a member of this conversation");

        var isOwnerOrDeputy = member.ConversationMemberRoleType == ConversationMemberRoleType.Owner ||
                              member.ConversationMemberRoleType == ConversationMemberRoleType.Deputy;
        var canPin = isOwnerOrDeputy || conversation.IsAllowMemberPin;

        if (!canPin)
            throw new UnauthorizedException("You don't have permission to pin messages in this conversation");

        message.IsPined = request.IsPined;
        message.UpdatedById = userId;
        message.UpdatedAt = _dateTimeProvider.OffsetNow;

        await _messageRepository.UpdateAsync(message, cancellationToken);
        await _messageRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        await _systemMessageService.CreateAsync(
            conversation.Id,
            userId,
            request.IsPined ? MessageType.MessagePinned : MessageType.MessageUnpinned,
            message.Id,
            cancellationToken);

        return Unit.Value;
    }

}