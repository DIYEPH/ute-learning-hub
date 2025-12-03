using MediatR;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Common.Events;
using UteLearningHub.Application.Services.File;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Application.Services.Message;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;
using DomainMessage = UteLearningHub.Domain.Entities.Message;
using DomainFile = UteLearningHub.Domain.Entities.File;

namespace UteLearningHub.Application.Features.Message.Commands.CreateMessage;

public class CreateMessageCommandHandler : IRequestHandler<CreateMessageCommand, MessageDto>
{
    private readonly IMessageRepository _messageRepository;
    private readonly IConversationRepository _conversationRepository;
    private readonly IFileUsageService _fileUsageService;
    private readonly IIdentityService _identityService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IMessageQueueProducer _messageQueueProducer;

    public CreateMessageCommandHandler(
        IMessageRepository messageRepository,
        IConversationRepository conversationRepository,
        IFileUsageService fileUsageService,
        IIdentityService identityService,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider,
        IMessageQueueProducer messageQueueProducer)
    {
        _messageRepository = messageRepository;
        _conversationRepository = conversationRepository;
        _fileUsageService = fileUsageService;
        _identityService = identityService;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
        _messageQueueProducer = messageQueueProducer;
    }

    public async Task<MessageDto> Handle(CreateMessageCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated)
            throw new UnauthorizedException("You must be authenticated to send messages");

        var userId = _currentUserService.UserId ?? throw new UnauthorizedException();

        // Validate conversation exists and user is a member
        var conversation = await _conversationRepository.GetByIdWithDetailsAsync(
            request.ConversationId, 
            disableTracking: false, 
            cancellationToken);
        
        if (conversation == null || conversation.IsDeleted)
            throw new NotFoundException($"Conversation with id {request.ConversationId} not found");

        if (conversation.ConversationStatus != Domain.Constaints.Enums.ConversationStatus.Active)
            throw new BadRequestException("Conversation is not active");

        // Check if user is a member
        var isMember = conversation.Members.Any(m => 
            m.UserId == userId && !m.IsDeleted);
        
        if (!isMember)
            throw new ForbidenException("You are not a member of this conversation");

        // Validate parent message if provided
        if (request.ParentId.HasValue)
        {
            var parentMessage = await _messageRepository.GetByIdAsync(
                request.ParentId.Value, 
                disableTracking: true, 
                cancellationToken);
            
            if (parentMessage == null || parentMessage.IsDeleted || 
                parentMessage.ConversationId != request.ConversationId)
                throw new NotFoundException($"Parent message with id {request.ParentId.Value} not found");
        }

        // Create message
        var message = new DomainMessage
        {
            Id = Guid.NewGuid(),
            ConversationId = request.ConversationId,
            ParentId = request.ParentId,
            Content = request.Content,
            IsEdit = false,
            IsPined = false,
            CreatedById = userId,
            CreatedAt = _dateTimeProvider.OffsetNow
        };

        var filesToPromote = new List<DomainFile>();

        if (request.FileIds != null && request.FileIds.Any())
        {
            var files = await _fileUsageService.EnsureFilesAsync(request.FileIds, cancellationToken);
            filesToPromote.AddRange(files);

            foreach (var file in files)
            {
                message.MessageFiles.Add(new Domain.Entities.MessageFile
                {
                    MessageId = message.Id,
                    FileId = file.Id
                });
            }
        }

        await _messageRepository.AddAsync(message, cancellationToken);

        // Update conversation's LastMessage
        conversation.LastMessage = message.Id;
        conversation.UpdatedAt = _dateTimeProvider.OffsetNow;
        await _conversationRepository.UpdateAsync(conversation, cancellationToken);

        await _messageRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        if (filesToPromote.Any())
        {
            var fileIds = filesToPromote.Select(f => f.Id).ToList();
            await _fileUsageService.MarkFilesAsPermanentAsync(fileIds, cancellationToken);
        }

        // Reload message with details
        var createdMessage = await _messageRepository.GetByIdWithDetailsAsync(
            message.Id, 
            disableTracking: true, 
            cancellationToken);

        if (createdMessage == null)
            throw new NotFoundException("Failed to create message");

        // Get sender information
        var sender = await _identityService.FindByIdAsync(userId);
        if (sender == null)
            throw new UnauthorizedException();

        var messageDto = new MessageDto
        {
            Id = createdMessage.Id,
            ConversationId = createdMessage.ConversationId,
            ParentId = createdMessage.ParentId,
            Content = createdMessage.Content,
            IsEdit = createdMessage.IsEdit,
            IsPined = createdMessage.IsPined,
            Type = createdMessage.Type,
            CreatedById = createdMessage.CreatedById,
            SenderName = sender.FullName,
            SenderAvatarUrl = sender.AvatarUrl,
            Files = createdMessage.MessageFiles.Select(mf => new MessageFileDto
            {
                FileId = mf.File.Id,
                FileName = mf.File.FileName,
                FileUrl = mf.File.FileUrl,
                FileSize = mf.File.FileSize,
                MimeType = mf.File.MimeType
            }).ToList(),
            CreatedAt = createdMessage.CreatedAt,
            UpdatedAt = createdMessage.UpdatedAt
        };

        await _messageQueueProducer.PublishMessageCreatedAsync(messageDto, cancellationToken);

        return messageDto;
    }
}