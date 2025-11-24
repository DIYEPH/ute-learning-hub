using MediatR;
using Microsoft.EntityFrameworkCore;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;

namespace UteLearningHub.Application.Features.Message.Queries.GetMessages;

public class GetMessagesHandler : IRequestHandler<GetMessagesQuery, PagedResponse<MessageDto>>
{
    private readonly IMessageRepository _messageRepository;
    private readonly IConversationRepository _conversationRepository;
    private readonly IIdentityService _identityService;
    private readonly ICurrentUserService _currentUserService;

    public GetMessagesHandler(
        IMessageRepository messageRepository,
        IConversationRepository conversationRepository,
        IIdentityService identityService,
        ICurrentUserService currentUserService)
    {
        _messageRepository = messageRepository;
        _conversationRepository = conversationRepository;
        _identityService = identityService;
        _currentUserService = currentUserService;
    }

    public async Task<PagedResponse<MessageDto>> Handle(GetMessagesQuery request, CancellationToken cancellationToken)
    {
        // Validate conversation exists
        var conversation = await _conversationRepository.GetByIdAsync(
            request.ConversationId, 
            disableTracking: true, 
            cancellationToken);
        
        if (conversation == null || conversation.IsDeleted)
            throw new NotFoundException($"Conversation with id {request.ConversationId} not found");

        // Check if user is a member (if authenticated)
        if (_currentUserService.IsAuthenticated)
        {
            var conversationWithMembers = await _conversationRepository.GetByIdWithDetailsAsync(
                request.ConversationId, 
                disableTracking: true, 
                cancellationToken);
            
            if (conversationWithMembers != null)
            {
                var userId = _currentUserService.UserId;
                var isMember = conversationWithMembers.Members.Any(m => 
                    m.UserId == userId && !m.IsDeleted);
                
                if (!isMember)
                    throw new ForbidenException("You are not a member of this conversation");
            }
        }

        var query = _messageRepository.GetQueryableWithDetails()
            .AsNoTracking()
            .Where(m => m.ConversationId == request.ConversationId && !m.IsDeleted);

        // Filter by parent
        if (request.ParentId.HasValue)
            query = query.Where(m => m.ParentId == request.ParentId.Value);
        else
            query = query.Where(m => m.ParentId == null);

        // Order by newest first
        query = query.OrderByDescending(m => m.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);

        var messages = await query
            .Skip(request.Skip)
            .Take(request.Take)
            .ToListAsync(cancellationToken);

        // Get sender information
        var senderIds = messages.Select(m => m.CreatedById).Distinct();
        var senderInfo = new Dictionary<Guid, (string FullName, string? AvatarUrl)>();

        foreach (var senderId in senderIds)
        {
            var sender = await _identityService.FindByIdAsync(senderId);
            if (sender != null)
            {
                senderInfo[senderId] = (sender.FullName, sender.AvatarUrl);
            }
        }

        var messageDtos = messages.Select(m => new MessageDto
        {
            Id = m.Id,
            ConversationId = m.ConversationId,
            ParentId = m.ParentId,
            Content = m.Content,
            IsEdit = m.IsEdit,
            IsPined = m.IsPined,
            CreatedById = m.CreatedById,
            SenderName = senderInfo.TryGetValue(m.CreatedById, out var info)
                ? info.FullName
                : "Unknown",
            SenderAvatarUrl = senderInfo.TryGetValue(m.CreatedById, out var sender)
                ? sender.AvatarUrl
                : null,
            Files = m.MessageFiles.Select(mf => new MessageFileDto
            {
                FileId = mf.File.Id,
                FileName = mf.File.FileName,
                FileUrl = mf.File.FileUrl,
                FileSize = mf.File.FileSize,
                MimeType = mf.File.MimeType
            }).ToList(),
            CreatedAt = m.CreatedAt,
            UpdatedAt = m.UpdatedAt
        }).ToList();

        return new PagedResponse<MessageDto>
        {
            Items = messageDtos,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}