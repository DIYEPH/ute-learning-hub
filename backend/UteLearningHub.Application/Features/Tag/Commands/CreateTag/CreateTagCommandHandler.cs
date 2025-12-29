using MediatR;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Application.Services.Tag;
using UteLearningHub.Domain.Exceptions;

namespace UteLearningHub.Application.Features.Tag.Commands.CreateTag;

public class CreateTagCommandHandler(ITagService tagService, ICurrentUserService currentUserService) : IRequestHandler<CreateTagCommand, TagDetailDto>
{
    private readonly ITagService _tagService = tagService;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    public async Task<TagDetailDto> Handle(CreateTagCommand request, CancellationToken ct)
    {
        if (!_currentUserService.IsAuthenticated)
            throw new UnauthorizedException("You must be authenticated to create tags");

        var actor = _currentUserService.UserId!.Value;

        return await _tagService.CreateAsync(actor, request.TagName, ct);
    }
}
