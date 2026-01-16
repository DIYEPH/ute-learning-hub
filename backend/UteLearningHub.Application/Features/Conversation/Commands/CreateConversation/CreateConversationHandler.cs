using MediatR;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Conversation;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Application.Services.Message;
using UteLearningHub.Application.Services.Recommendation;
using UteLearningHub.Application.Services.User;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Entities;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;
using DomainConversation = UteLearningHub.Domain.Entities.Conversation;
using DomainTag = UteLearningHub.Domain.Entities.Tag;

namespace UteLearningHub.Application.Features.Conversation.Commands.CreateConversation;

public class CreateConversationHandler(
    IConversationRepository conversationRepository,
    ITagRepository tagRepository,
    IIdentityService identityService,
    ICurrentUserService currentUserService,
    IUserService userService,
    IDateTimeProvider dateTimeProvider,
    IConversationSystemMessageService systemMessageService,
    IVectorMaintenanceService vectorMaintenanceService,
    IConversationQueryService conversationQueryService) : IRequestHandler<CreateConversationCommand, ConversationDetailDto>
{
    public async Task<ConversationDetailDto> Handle(CreateConversationCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedException("You must be authenticated to create a conversation");

        var userId = currentUserService.UserId ?? throw new UnauthorizedException();

        // Kiểm tra quyền: Admin hoặc Contributor trở lên
        var isAdmin = currentUserService.IsInRole("Admin");
        if (!isAdmin)
        {
            var trustLevel = await userService.GetTrustLevelAsync(userId, cancellationToken);
            if (!trustLevel.HasValue || trustLevel.Value < TrustLever.Contributor)
                throw new ForbiddenException("Bạn cần đạt cấp độ Contributor trở lên để tạo nhóm học tập. Hãy đóng góp thêm tài liệu để tăng điểm uy tín!");
        }

        var creator = await identityService.FindByIdAsync(userId)
            ?? throw new UnauthorizedException();

        var tagIdsToAdd = new List<Guid>();

        // Xử lý tags từ IDs
        if (request.TagIds != null && request.TagIds.Any())
        {
            var existingTags = await tagRepository.GetByIdsAsync(request.TagIds, cancellationToken: cancellationToken);
            if (existingTags.Count != request.TagIds.Count)
                throw new NotFoundException("One or more tags not found");

            tagIdsToAdd.AddRange(existingTags.Select(t => t.Id));
        }

        // Xử lý tags từ tên (tìm hoặc tạo mới)
        if (request.TagNames != null && request.TagNames.Any())
        {
            foreach (var tagName in request.TagNames)
            {
                if (string.IsNullOrWhiteSpace(tagName)) continue;

                var normalizedName = tagName.Trim();
                var existingTag = await tagRepository.FindByNameAsync(normalizedName, cancellationToken: cancellationToken);

                if (existingTag != null)
                {
                    if (!tagIdsToAdd.Contains(existingTag.Id))
                        tagIdsToAdd.Add(existingTag.Id);
                }
                else
                {
                    // Tạo tag mới với TitleCase
                    var titleCaseName = System.Globalization.CultureInfo
                        .CurrentCulture.TextInfo.ToTitleCase(normalizedName.ToLower());

                    var newTag = new DomainTag
                    {
                        Id = Guid.NewGuid(),
                        TagName = titleCaseName,
                        Status = ContentStatus.Approved,
                        CreatedById = userId,
                        CreatedAt = dateTimeProvider.OffsetNow
                    };

                    tagRepository.Add(newTag);
                    tagIdsToAdd.Add(newTag.Id);
                }
            }
        }

        if (!tagIdsToAdd.Any())
            throw new BadRequestException("Conversation must have at least one tag");

        var conversation = new DomainConversation
        {
            Id = Guid.NewGuid(),
            ConversationName = request.ConversationName,
            AvatarUrl = request.AvatarUrl,
            ConversationType = request.ConversationType,
            Visibility = request.Visibility,
            ConversationStatus = ConversationStatus.Active,
            SubjectId = request.SubjectId,
            IsSuggestedByAI = request.IsSuggestedByAI,
            IsAllowMemberPin = request.IsAllowMemberPin,
            CreatedById = userId,
            CreatedAt = dateTimeProvider.OffsetNow
        };

        // Gắn tags vào conversation
        var tags = await tagRepository.GetByIdsAsync(tagIdsToAdd, cancellationToken: cancellationToken);
        foreach (var tag in tags)
        {
            conversation.ConversationTags.Add(new ConversationTag
            {
                ConversationId = conversation.Id,
                TagId = tag.Id
            });
        }

        // Thêm owner làm member đầu tiên
        var ownerMember = new ConversationMember
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ConversationId = conversation.Id,
            ConversationMemberRoleType = ConversationMemberRoleType.Owner,
            IsMuted = false,
            CreatedAt = dateTimeProvider.OffsetNow
        };

        conversation.Members.Add(ownerMember);

        conversationRepository.Add(conversation);
        await conversationRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        // Tạo tin nhắn hệ thống
        await systemMessageService.CreateAsync(
            conversation.Id, userId, MessageType.ConversationCreated, null, cancellationToken);

        // Update vector background
        _ = Task.Run(async () =>
        {
            try { await vectorMaintenanceService.UpdateConversationVectorAsync(conversation.Id, cancellationToken); }
            catch { }
        }, cancellationToken);

        var result = await conversationQueryService.GetDetailByIdAsync(conversation.Id, cancellationToken);
        return result ?? throw new NotFoundException("Failed to create conversation");
    }
}