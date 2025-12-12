using MediatR;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;
using TagEntity = UteLearningHub.Domain.Entities.Tag;

namespace UteLearningHub.Application.Features.Tag.Commands.CreateTag;

public class CreateTagCommandHandler : IRequestHandler<CreateTagCommand, TagDetailDto>
{
    private readonly ITagRepository _tagRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public CreateTagCommandHandler(
        ITagRepository tagRepository,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider)
    {
        _tagRepository = tagRepository;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<TagDetailDto> Handle(CreateTagCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated)
            throw new UnauthorizedException("You must be authenticated to create tags");

        var userId = _currentUserService.UserId ?? throw new UnauthorizedException();

        if (string.IsNullOrWhiteSpace(request.TagName))
            throw new BadRequestException("TagName cannot be empty");

        var existingTag = await _tagRepository.FindByNameAsync(request.TagName, cancellationToken: cancellationToken);

        if (existingTag != null)
            throw new BadRequestException($"Tag with name '{request.TagName}' already exists");

        var tag = new TagEntity
        {
            Id = Guid.NewGuid(),
            TagName = request.TagName,
            Status = ContentStatus.Approved,
            CreatedById = userId,
            CreatedAt = _dateTimeProvider.OffsetNow
        };

        await _tagRepository.AddAsync(tag, cancellationToken);
        await _tagRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        return new TagDetailDto
        {
            Id = tag.Id,
            TagName = tag.TagName,
            DocumentCount = 0
        };
    }
}
