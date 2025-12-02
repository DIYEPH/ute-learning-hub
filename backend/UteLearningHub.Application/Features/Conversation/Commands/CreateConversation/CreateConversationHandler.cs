using MediatR;
using Microsoft.EntityFrameworkCore;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Application.Services.Message;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;
using UteLearningHub.Domain.Entities;
using DomainConversation = UteLearningHub.Domain.Entities.Conversation;
using DomainTag = UteLearningHub.Domain.Entities.Tag;

namespace UteLearningHub.Application.Features.Conversation.Commands.CreateConversation;

public class CreateConversationHandler : IRequestHandler<CreateConversationCommand, ConversationDetailDto>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly ITagRepository _tagRepository;
    private readonly IIdentityService _identityService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IConversationSystemMessageService _systemMessageService;

    public CreateConversationHandler(
        IConversationRepository conversationRepository,
        ITagRepository tagRepository,
        IIdentityService identityService,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider,
        IConversationSystemMessageService systemMessageService)
    {
        _conversationRepository = conversationRepository;
        _tagRepository = tagRepository;
        _identityService = identityService;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
        _systemMessageService = systemMessageService;
    }

    public async Task<ConversationDetailDto> Handle(CreateConversationCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated)
            throw new UnauthorizedException("You must be authenticated to create a conversation");

        var userId = _currentUserService.UserId ?? throw new UnauthorizedException();

        var creator = await _identityService.FindByIdAsync(userId)
            ?? throw new UnauthorizedException();

        // Verify subject exists if provided
        if (request.SubjectId.HasValue)
        {
            // You may want to add ISubjectRepository to verify
            // For now, we'll skip this check or add it later
        }

        // Xử lý tags tương tự Document
        var tagIdsToAdd = new List<Guid>();

        if (request.TagIds != null && request.TagIds.Any())
        {
            var existingTags = await _tagRepository.GetQueryableSet()
                .Where(t => request.TagIds.Contains(t.Id) && !t.IsDeleted)
                .ToListAsync(cancellationToken);

            if (existingTags.Count != request.TagIds.Count)
                throw new NotFoundException("One or more tags not found");

            tagIdsToAdd.AddRange(existingTags.Select(t => t.Id));
        }

        if (request.TagNames != null && request.TagNames.Any())
        {
            foreach (var tagName in request.TagNames)
            {
                if (string.IsNullOrWhiteSpace(tagName)) continue;

                var normalizedName = tagName.Trim();
                var normalizedNameLower = normalizedName.ToLowerInvariant();

                var existingTag = await _tagRepository.GetQueryableSet()
                    .Where(t => !t.IsDeleted && t.TagName != null)
                    .FirstOrDefaultAsync(
                        t => t.TagName!.ToLower() == normalizedNameLower,
                        cancellationToken);

                if (existingTag != null)
                {
                    if (!tagIdsToAdd.Contains(existingTag.Id))
                        tagIdsToAdd.Add(existingTag.Id);
                }
                else
                {
                    var titleCaseName = System.Globalization.CultureInfo
                        .CurrentCulture
                        .TextInfo
                        .ToTitleCase(normalizedName.ToLower());

                    var newTag = new DomainTag
                    {
                        Id = Guid.NewGuid(),
                        TagName = titleCaseName,
                        ReviewStatus = ReviewStatus.Approved,
                        CreatedById = userId,
                        CreatedAt = _dateTimeProvider.OffsetNow
                    };

                    await _tagRepository.AddAsync(newTag, cancellationToken);
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
            ConversationType = request.ConversationType,
            ConversationStatus = ConversationStatus.Active,
            SubjectId = request.SubjectId,
            IsSuggestedByAI = request.IsSuggestedByAI,
            IsAllowMemberPin = request.IsAllowMemberPin,
            CreatedById = userId,
            CreatedAt = _dateTimeProvider.OffsetNow
        };

        // Thêm tags vào conversation
        if (tagIdsToAdd.Any())
        {
            var tags = await _tagRepository.GetQueryableSet()
                .Where(t => tagIdsToAdd.Contains(t.Id) && !t.IsDeleted)
                .ToListAsync(cancellationToken);

            foreach (var tag in tags)
            {
                conversation.ConversationTags.Add(new ConversationTag
                {
                    ConversationId = conversation.Id,
                    TagId = tag.Id
                });
            }
        }

        var ownerMember = new ConversationMember
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ConversationId = conversation.Id,
            ConversationMemberRoleType = ConversationMemberRoleType.Owner,
            IsMuted = false,
            CreatedAt = _dateTimeProvider.OffsetNow
        };

        conversation.Members.Add(ownerMember);

        await _conversationRepository.AddAsync(conversation, cancellationToken);
        await _conversationRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        await _systemMessageService.CreateAsync(
            conversation.Id,
            userId,
            MessageType.ConversationCreated,
            null,
            cancellationToken);

        var createdConversation = await _conversationRepository.GetByIdWithDetailsAsync(
            conversation.Id,
            disableTracking: true,
            cancellationToken);

        if (createdConversation == null)
            throw new NotFoundException("Failed to create conversation");

        var memberUserIds = createdConversation.Members
            .Where(m => !m.IsDeleted)
            .Select(m => m.UserId)
            .Distinct()
            .ToList();

        var memberInfo = new Dictionary<Guid, (string FullName, string? AvatarUrl)>();

        foreach (var memberUserId in memberUserIds)
        {
            var user = await _identityService.FindByIdAsync(memberUserId);
            if (user != null)
            {
                memberInfo[memberUserId] = (user.FullName, user.AvatarUrl);
            }
        }

        return new ConversationDetailDto
        {
            Id = createdConversation.Id,
            ConversationName = createdConversation.ConversationName,
            Tags = createdConversation.ConversationTags
                .Select(ct => new TagDto
                {
                    Id = ct.Tag.Id,
                    TagName = ct.Tag.TagName
                })
                .ToList(),
            ConversationType = createdConversation.ConversationType,
            ConversationStatus = createdConversation.ConversationStatus,
            IsSuggestedByAI = createdConversation.IsSuggestedByAI,
            IsAllowMemberPin = createdConversation.IsAllowMemberPin,
            Subject = createdConversation.Subject != null ? new SubjectDto
            {
                Id = createdConversation.Subject.Id,
                SubjectName = createdConversation.Subject.SubjectName,
                SubjectCode = createdConversation.Subject.SubjectCode
            } : null,
            CreatorName = creator.FullName,
            CreatorAvatarUrl = creator.AvatarUrl,
            Members = createdConversation.Members
                .Where(m => !m.IsDeleted)
                .Select(m => new ConversationMemberDto
                {
                    Id = m.Id,
                    UserId = m.UserId,
                    UserName = memberInfo.TryGetValue(m.UserId, out var info)
                        ? info.FullName
                        : "Unknown",
                    UserAvatarUrl = memberInfo.TryGetValue(m.UserId, out var member)
                        ? member.AvatarUrl
                        : null,
                    RoleType = m.ConversationMemberRoleType,
                    IsMuted = m.IsMuted,
                    JoinedAt = m.CreatedAt
                }).ToList(),
            MessageCount = 0,
            LastMessageId = createdConversation.LastMessage,
            CreatedById = createdConversation.CreatedById,
            CreatedAt = createdConversation.CreatedAt,
            UpdatedAt = createdConversation.UpdatedAt
        };
    }
}