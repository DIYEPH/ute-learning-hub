using MediatR;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Conversation;
using UteLearningHub.Application.Services.File;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Application.Services.Recommendation;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Entities;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;
using DomainTag = UteLearningHub.Domain.Entities.Tag;

namespace UteLearningHub.Application.Features.Conversation.Commands.UpdateConversation;

public class UpdateConversationHandler : IRequestHandler<UpdateConversationCommand, ConversationDetailDto>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly ITagRepository _tagRepository;
    private readonly IFileUsageService _fileUsageService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IVectorMaintenanceService _vectorMaintenanceService;
    private readonly IConversationQueryService _conversationQueryService;

    public UpdateConversationHandler(
        IConversationRepository conversationRepository,
        ITagRepository tagRepository,
        IFileUsageService fileUsageService,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider,
        IVectorMaintenanceService vectorMaintenanceService,
        IConversationQueryService conversationQueryService)
    {
        _conversationRepository = conversationRepository;
        _tagRepository = tagRepository;
        _fileUsageService = fileUsageService;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
        _vectorMaintenanceService = vectorMaintenanceService;
        _conversationQueryService = conversationQueryService;
    }

    public async Task<ConversationDetailDto> Handle(UpdateConversationCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated)
            throw new UnauthorizedException("You must be authenticated to update a conversation");

        var userId = _currentUserService.UserId ?? throw new UnauthorizedException();

        var conversation = await _conversationRepository.GetByIdWithDetailsAsync(request.Id, disableTracking: false, cancellationToken);

        if (conversation == null || conversation.IsDeleted)
            throw new NotFoundException($"Conversation with id {request.Id} not found");

        var isAdmin = _currentUserService.IsInRole("Admin");
        var isOwnerOrDeputy = conversation.Members.Any(m =>
            m.UserId == userId &&
            (m.ConversationMemberRoleType == ConversationMemberRoleType.Owner ||
             m.ConversationMemberRoleType == ConversationMemberRoleType.Deputy) &&
            !m.IsDeleted);

        if (!isAdmin && !isOwnerOrDeputy)
            throw new UnauthorizedException("Only administrators, conversation owners, or deputies can update conversations");

        if (!string.IsNullOrWhiteSpace(request.ConversationName))
            conversation.ConversationName = request.ConversationName;

        var previousAvatarUrl = conversation.AvatarUrl;

        if (!string.IsNullOrWhiteSpace(request.AvatarUrl))
            conversation.AvatarUrl = request.AvatarUrl;

        if (request.ConversationType.HasValue)
            conversation.ConversationType = request.ConversationType.Value;

        if (request.ConversationStatus.HasValue)
            conversation.ConversationStatus = request.ConversationStatus.Value;

        var subjectChanged = request.SubjectId.HasValue && conversation.SubjectId != request.SubjectId;
        var tagsChanged = request.TagIds != null || request.TagNames != null;

        if (request.SubjectId.HasValue)
            conversation.SubjectId = request.SubjectId;

        if (request.IsAllowMemberPin.HasValue)
            conversation.IsAllowMemberPin = request.IsAllowMemberPin.Value;

        if (request.TagIds != null || request.TagNames != null)
        {
            conversation.ConversationTags.Clear();
            var tagIdsToAdd = new List<Guid>();

            if (request.TagIds != null && request.TagIds.Any())
            {
                var existingTags = await _tagRepository.GetByIdsAsync(request.TagIds, cancellationToken: cancellationToken);

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
                    var existingTag = await _tagRepository.FindByNameAsync(normalizedName, cancellationToken: cancellationToken);

                    if (existingTag != null)
                    {
                        if (!tagIdsToAdd.Contains(existingTag.Id))
                            tagIdsToAdd.Add(existingTag.Id);
                    }
                    else
                    {
                        var titleCaseName = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(normalizedName.ToLower());

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

            if (tagIdsToAdd.Any())
            {
                var tags = await _tagRepository.GetByIdsAsync(tagIdsToAdd, cancellationToken: cancellationToken);

                foreach (var tag in tags)
                {
                    conversation.ConversationTags.Add(new ConversationTag
                    {
                        ConversationId = conversation.Id,
                        TagId = tag.Id
                    });
                }
            }
        }

        conversation.UpdatedById = userId;
        conversation.UpdatedAt = _dateTimeProvider.OffsetNow;

        await _conversationRepository.UpdateAsync(conversation, cancellationToken);
        await _conversationRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        if (subjectChanged || tagsChanged)
        {
            _ = Task.Run(async () =>
            {
                try { await _vectorMaintenanceService.UpdateConversationVectorAsync(conversation.Id, cancellationToken); }
                catch { }
            }, cancellationToken);
        }

        if (!string.IsNullOrWhiteSpace(request.AvatarUrl) &&
            !string.Equals(previousAvatarUrl, request.AvatarUrl, StringComparison.OrdinalIgnoreCase))
        {
            if (!string.IsNullOrWhiteSpace(previousAvatarUrl))
            {
                var oldAvatar = await _fileUsageService.TryGetByUrlAsync(previousAvatarUrl, cancellationToken);
                if (oldAvatar != null)
                    await _fileUsageService.DeleteFileAsync(oldAvatar, cancellationToken);
            }
        }

        var result = await _conversationQueryService.GetDetailByIdAsync(request.Id, cancellationToken);
        return result ?? throw new NotFoundException("Failed to update conversation");
    }
}
