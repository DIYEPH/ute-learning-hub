using MediatR;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Application.Services.Tag;
using UteLearningHub.Domain.Exceptions;

namespace UteLearningHub.Application.Features.Tag.Commands.UpdateTag;

public class UpdateTagCommandHandler(ITagService tagService, ICurrentUserService currentUserService) : IRequestHandler<UpdateTagCommand, TagDetailDto>
{
    private readonly ITagService _tagService = tagService;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    public async Task<TagDetailDto> Handle(UpdateTagCommand request, CancellationToken cancellationToken)
    {

        if (!_currentUserService.IsInRole("Admin"))
            throw new ForbiddenException("Only admin can delete tags");

        var actorId = _currentUserService.UserId!.Value;

        return await _tagService.UpdateAsync(actorId, request, cancellationToken);
    }
}