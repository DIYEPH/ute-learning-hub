using MediatR;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;

namespace UteLearningHub.Application.Features.Message.Commands.UpdateMessage;

public class UpdateMessageCommandHandler : IRequestHandler<UpdateMessageCommand, MessageDto>
{
    private readonly IMessageRepository _messageRepository;
    private readonly IConversationRepository _conversationRepository;
    private readonly IIdentityService _identityService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public UpdateMessageCommandHandler(
        IMessageRepository messageRepository,
        IConversationRepository conversationRepository,
        IIdentityService identityService,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider)
    {
        _messageRepository = messageRepository;
        _conversationRepository = conversationRepository;
        _identityService = identityService;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<MessageDto> Handle(UpdateMessageCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated)
            throw new UnauthorizedException("You must be authenticated to update messages");

        var userId = _currentUserService.UserId ?? throw new UnauthorizedException();

        // Get message with details
        var message = await _messageRepository.GetByIdWithDetailsAsync(
            request.Id, 
            disableTracking: false, 
            cancellationToken);

        if (message == null || message.IsDeleted)
            throw new NotFoundException($"Message with id {request.Id} not found");

        if (message.ConversationId != request.ConversationId)
            throw new BadRequestException("Message does not belong to the specified conversation");

        // Only owner can update
        if (message.CreatedById != userId)
            throw new UnauthorizedException("You don't have permission to update this message");

        // Validate conversation is still active
        var conversation = await _conversationRepository.GetByIdAsync(
            request.ConversationId, 
            disableTracking: true, 
            cancellationToken);

        if (conversation == null || conversation.IsDeleted)
            throw new NotFoundException("Conversation not found");

        if (conversation.ConversationStatus != Domain.Constaints.Enums.ConversationStatus.Active)
            throw new BadRequestException("Conversation is not active");

        // Update message
        message.Content = request.Content;
        message.IsEdit = true;
        message.UpdatedById = userId;
        message.UpdatedAt = _dateTimeProvider.OffsetNow;

        await _messageRepository.UpdateAsync(message, cancellationToken);
        await _messageRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        // Reload message with details
        var updatedMessage = await _messageRepository.GetByIdWithDetailsAsync(
            message.Id, 
            disableTracking: true, 
            cancellationToken);

        if (updatedMessage == null)
            throw new NotFoundException("Failed to update message");

        // Get sender information
        var sender = await _identityService.FindByIdAsync(userId);
        if (sender == null)
            throw new UnauthorizedException();

        return new MessageDto
        {
            Id = updatedMessage.Id,
            ConversationId = updatedMessage.ConversationId,
            ParentId = updatedMessage.ParentId,
            Content = updatedMessage.Content,
            IsEdit = updatedMessage.IsEdit,
            IsPined = updatedMessage.IsPined,
            Type = updatedMessage.Type,
            CreatedById = updatedMessage.CreatedById,
            SenderName = sender.FullName,
            SenderAvatarUrl = sender.AvatarUrl,
            Files = updatedMessage.MessageFiles.Select(mf => new MessageFileDto
            {
                FileId = mf.File.Id,
                FileName = mf.File.FileName,
                FileUrl = mf.File.FileUrl,
                FileSize = mf.File.FileSize,
                MimeType = mf.File.MimeType
            }).ToList(),
            CreatedAt = updatedMessage.CreatedAt,
            UpdatedAt = updatedMessage.UpdatedAt
        };
    }
}