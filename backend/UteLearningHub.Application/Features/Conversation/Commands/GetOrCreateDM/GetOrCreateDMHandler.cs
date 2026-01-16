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

public class GetOrCreateDMHandler(
    IConversationRepository convRepo,
    IMessageRepository msgRepo,
    ICurrentUserService currentUser,
    IIdentityService identity,
    IDateTimeProvider dateTime,
    IMessageHubService msgHub) : IRequestHandler<GetOrCreateDMCommand, GetOrCreateDMResponse>
{
    public async Task<GetOrCreateDMResponse> Handle(GetOrCreateDMCommand req, CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated)
            throw new UnauthorizedException("Must be authenticated");

        var userId = currentUser.UserId ?? throw new UnauthorizedException();

        if (userId == req.TargetUserId)
            throw new BadRequestException("Cannot create DM with yourself");

        var targetUser = await identity.FindByIdAsync(req.TargetUserId);
        if (targetUser == null)
            throw new NotFoundException("User not found");

        // Tìm DM Private giữa 2 user
        var existingConv = await convRepo.GetQueryableSet()
            .Where(c => c.ConversationType == ConversitionType.Private
                && c.Members.Any(m => m.UserId == userId && !m.IsDeleted)
                && c.Members.Any(m => m.UserId == req.TargetUserId && !m.IsDeleted))
            .Include(c => c.Members)
            .Include(c => c.ConversationTags).ThenInclude(ct => ct.Tag)
            .FirstOrDefaultAsync(ct);

        if (existingConv != null)
            return new GetOrCreateDMResponse
            {
                Conversation = await MapToDto(existingConv, ct),
                FirstMessageSent = null,
                IsNewConversation = false
            };

        // DM mới cần FirstMessage
        if (string.IsNullOrWhiteSpace(req.FirstMessage))
            throw new BadRequestException("First message is required to start a new DM");

        var currentUserInfo = await identity.FindByIdAsync(userId);
        var convName = $"{currentUserInfo?.FullName ?? "User"} & {targetUser.FullName}";

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
            CreatedAt = dateTime.OffsetNow
        };

        convRepo.Add(newConv);

        // Thêm 2 members (cả 2 đều Owner trong DM)
        var now = dateTime.OffsetNow;
        await convRepo.AddMemberAsync(new ConversationMember
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ConversationId = newConv.Id,
            ConversationMemberRoleType = ConversationMemberRoleType.Owner,
            IsMuted = false,
            CreatedAt = now
        }, ct);

        await convRepo.AddMemberAsync(new ConversationMember
        {
            Id = Guid.NewGuid(),
            UserId = req.TargetUserId,
            ConversationId = newConv.Id,
            ConversationMemberRoleType = ConversationMemberRoleType.Owner,
            IsMuted = false,
            CreatedAt = now
        }, ct);

        var firstMsg = new Domain.Entities.Message
        {
            Id = Guid.NewGuid(),
            ConversationId = newConv.Id,
            Content = req.FirstMessage,
            IsPined = false,
            CreatedById = userId,
            CreatedAt = dateTime.OffsetNow
        };

        msgRepo.Add(firstMsg);
        newConv.LastMessage = firstMsg.Id;

        await convRepo.UnitOfWork.SaveChangesAsync(ct);

        var createdConv = await convRepo.GetByIdWithDetailsAsync(newConv.Id, true, ct);

        var msgDto = new MessageDto
        {
            Id = firstMsg.Id,
            ConversationId = firstMsg.ConversationId,
            Content = firstMsg.Content,
            IsPined = firstMsg.IsPined,
            CreatedById = firstMsg.CreatedById,
            SenderName = currentUserInfo?.FullName ?? "Unknown",
            SenderAvatarUrl = currentUserInfo?.AvatarUrl,
            CreatedAt = firstMsg.CreatedAt
        };

        await msgHub.BroadcastMessageCreatedAsync(msgDto, ct);

        return new GetOrCreateDMResponse
        {
            Conversation = await MapToDto(createdConv!, ct),
            FirstMessageSent = msgDto,
            IsNewConversation = true
        };
    }

    private async Task<ConversationDetailDto> MapToDto(Domain.Entities.Conversation conv, CancellationToken ct)
    {
        // Load users theo batch thay vì từng cái
        var memberUserIds = conv.Members.Where(m => !m.IsDeleted).Select(m => m.UserId).Distinct().ToList();
        var memberInfoDict = await identity.FindByIdsAsync(memberUserIds, ct);
        var memberInfo = memberInfoDict.ToDictionary(
            kvp => kvp.Key,
            kvp => (kvp.Value.FullName, kvp.Value.AvatarUrl));

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