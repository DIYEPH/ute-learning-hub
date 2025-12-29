using MediatR;
using Microsoft.EntityFrameworkCore;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Application.Services.Message;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Entities;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;

namespace UteLearningHub.Application.Features.Conversation.Commands.GetOrCreateDM;

public class GetOrCreateDMHandler : IRequestHandler<GetOrCreateDMCommand, GetOrCreateDMResponse>
{
    private readonly IConversationRepository _convRepo;
    private readonly IMessageRepository _msgRepo;
    private readonly ICurrentUserService _currentUser;
    private readonly IIdentityService _identity;
    private readonly IDateTimeProvider _dateTime;
    private readonly IMessageHubService _msgHub;

    public GetOrCreateDMHandler(
        IConversationRepository convRepo,
        IMessageRepository msgRepo,
        ICurrentUserService currentUser,
        IIdentityService identity,
        IDateTimeProvider dateTime,
        IMessageHubService msgHub)
    {
        _convRepo = convRepo;
        _msgRepo = msgRepo;
        _currentUser = currentUser;
        _identity = identity;
        _dateTime = dateTime;
        _msgHub = msgHub;
    }

    public async Task<GetOrCreateDMResponse> Handle(GetOrCreateDMCommand req, CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated)
            throw new UnauthorizedException("Must be authenticated");

        var userId = _currentUser.UserId ?? throw new UnauthorizedException();

        if (userId == req.TargetUserId)
            throw new BadRequestException("Cannot create DM with yourself");

        // Kiểm tra target user tồn tại
        var targetUser = await _identity.FindByIdAsync(req.TargetUserId);
        if (targetUser == null)
            throw new NotFoundException("User not found");

        // Tìm conversation Private giữa 2 user
        var existingConv = await _convRepo.GetQueryableSet()
            .Where(c => c.ConversationType == ConversitionType.Private && !c.IsDeleted)
            .Where(c => c.Members.Any(m => m.UserId == userId && !m.IsDeleted))
            .Where(c => c.Members.Any(m => m.UserId == req.TargetUserId && !m.IsDeleted))
            .Include(c => c.Members)
            .Include(c => c.ConversationTags).ThenInclude(ct => ct.Tag)
            .FirstOrDefaultAsync(ct);

        if (existingConv != null)
        {
            // DM đã tồn tại - trả về luôn
            return new GetOrCreateDMResponse
            {
                Conversation = await MapToDto(existingConv),
                FirstMessageSent = null,
                IsNewConversation = false
            };
        }

        // DM chưa tồn tại - yêu cầu phải có FirstMessage
        if (string.IsNullOrWhiteSpace(req.FirstMessage))
            throw new BadRequestException("First message is required to start a new DM");

        // Tạo conversation mới
        var currentUser = await _identity.FindByIdAsync(userId);
        var convName = $"{currentUser?.FullName ?? "User"} & {targetUser.FullName}";

        var newConv = new Domain.Entities.Conversation
        {
            Id = Guid.NewGuid(),
            ConversationName = convName,
            ConversationType = ConversitionType.Private,
            Visibility = ConversationVisibility.Private,
            ConversationStatus = ConversationStatus.Active,
            IsSuggestedByAI = false,
            IsAllowMemberPin = true,
            CreatedById = userId,
            CreatedAt = _dateTime.OffsetNow
        };

        _convRepo.Add(newConv);

        // Thêm 2 members (cả 2 đều là Owner trong DM)
        await _convRepo.AddMemberAsync(new ConversationMember
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ConversationId = newConv.Id,
            ConversationMemberRoleType = ConversationMemberRoleType.Owner,
            IsMuted = false
        }, ct);

        await _convRepo.AddMemberAsync(new ConversationMember
        {
            Id = Guid.NewGuid(),
            UserId = req.TargetUserId,
            ConversationId = newConv.Id,
            ConversationMemberRoleType = ConversationMemberRoleType.Owner,
            IsMuted = false
        }, ct);

        // Tạo tin nhắn đầu tiên
        var firstMsg = new Domain.Entities.Message
        {
            Id = Guid.NewGuid(),
            ConversationId = newConv.Id,
            Content = req.FirstMessage,
            IsPined = false,
            CreatedById = userId,
            CreatedAt = _dateTime.OffsetNow
        };

        _msgRepo.Add(firstMsg);
        newConv.LastMessage = firstMsg.Id;

        await _convRepo.UnitOfWork.SaveChangesAsync(ct);

        // Lấy lại conversation với đầy đủ thông tin
        var createdConv = await _convRepo.GetByIdWithDetailsAsync(newConv.Id, true, ct);

        var msgDto = new MessageDto
        {
            Id = firstMsg.Id,
            ConversationId = firstMsg.ConversationId,
            Content = firstMsg.Content,
            IsPined = firstMsg.IsPined,
            CreatedById = firstMsg.CreatedById,
            SenderName = currentUser?.FullName ?? "Unknown",
            SenderAvatarUrl = currentUser?.AvatarUrl,
            CreatedAt = firstMsg.CreatedAt
        };

        // Broadcast tin nhắn qua SignalR
        await _msgHub.BroadcastMessageCreatedAsync(msgDto, ct);

        return new GetOrCreateDMResponse
        {
            Conversation = await MapToDto(createdConv!),
            FirstMessageSent = msgDto,
            IsNewConversation = true
        };
    }

    private async Task<ConversationDetailDto> MapToDto(Domain.Entities.Conversation conv)
    {
        var memberUserIds = conv.Members.Where(m => !m.IsDeleted).Select(m => m.UserId).ToList();
        var memberInfo = new Dictionary<Guid, (string FullName, string? AvatarUrl)>();

        foreach (var memberId in memberUserIds)
        {
            var user = await _identity.FindByIdAsync(memberId);
            if (user != null)
                memberInfo[memberId] = (user.FullName, user.AvatarUrl);
        }

        return new ConversationDetailDto
        {
            Id = conv.Id,
            ConversationName = conv.ConversationName,
            Tags = conv.ConversationTags.Select(ct => new TagDto
            {
                Id = ct.Tag?.Id ?? Guid.Empty,
                TagName = ct.Tag?.TagName ?? ""
            }).ToList(),
            ConversationType = conv.ConversationType,
            ConversationStatus = conv.ConversationStatus,
            IsSuggestedByAI = conv.IsSuggestedByAI,
            IsAllowMemberPin = conv.IsAllowMemberPin,
            Members = conv.Members.Where(m => !m.IsDeleted).Select(m => new ConversationMemberDto
            {
                Id = m.Id,
                UserId = m.UserId,
                UserName = memberInfo.TryGetValue(m.UserId, out var info) ? info.FullName : "Unknown",
                UserAvatarUrl = memberInfo.TryGetValue(m.UserId, out var member) ? member.AvatarUrl : null,
                RoleType = m.ConversationMemberRoleType,
                IsMuted = m.IsMuted,
                JoinedAt = m.CreatedAt
            }).ToList(),
            MessageCount = 0,
            LastMessageId = conv.LastMessage,
            CreatedById = conv.CreatedById,
            CreatedAt = conv.CreatedAt,
            UpdatedAt = conv.UpdatedAt
        };
    }
}
