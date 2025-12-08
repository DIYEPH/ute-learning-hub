using MediatR;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;

namespace UteLearningHub.Application.Features.Tag.Commands.UpdateTag;

public class UpdateTagCommandHandler : IRequestHandler<UpdateTagCommand, TagDetailDto>
{
    private readonly ITagRepository _tagRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public UpdateTagCommandHandler(
        ITagRepository tagRepository,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider)
    {
        _tagRepository = tagRepository;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<TagDetailDto> Handle(UpdateTagCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated)
            throw new UnauthorizedException("You must be authenticated to update tags");

        var userId = _currentUserService.UserId ?? throw new UnauthorizedException();

        if (string.IsNullOrWhiteSpace(request.TagName))
            throw new BadRequestException("TagName cannot be empty");

        var tag = await _tagRepository.GetByIdAsync(request.Id, disableTracking: false, cancellationToken);

        if (tag == null || tag.IsDeleted)
            throw new NotFoundException($"Tag with id {request.Id} not found");

        var isAdmin = _currentUserService.IsInRole("Admin");
        if (!isAdmin && tag.CreatedById != userId)
            throw new UnauthorizedException("You don't have permission to update this tag");

        var existingTag = await _tagRepository.FindByNameAsync(request.TagName, cancellationToken: cancellationToken);

        if (existingTag != null && existingTag.Id != request.Id)
            throw new BadRequestException($"Tag with name '{request.TagName}' already exists");

        tag.TagName = request.TagName;
        tag.UpdatedById = userId;
        tag.UpdatedAt = _dateTimeProvider.OffsetNow;

        await _tagRepository.UpdateAsync(tag, cancellationToken);
        await _tagRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        var documentCount = await _tagRepository.GetDocumentCountAsync(request.Id, cancellationToken);

        return new TagDetailDto
        {
            Id = tag.Id,
            TagName = tag.TagName,
            DocumentCount = documentCount
        };
    }
}