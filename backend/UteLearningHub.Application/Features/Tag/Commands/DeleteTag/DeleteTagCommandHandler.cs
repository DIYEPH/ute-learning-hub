using MediatR;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Application.Services.Tag;
using UteLearningHub.Domain.Exceptions;

namespace UteLearningHub.Application.Features.Tag.Commands.DeleteTag;

public class DeleteTagCommandHandler(ITagService tagService, ICurrentUserService currentUserService) : IRequestHandler<DeleteTagCommand>
{
    private readonly ITagService _tagService = tagService;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    public async Task Handle(DeleteTagCommand request, CancellationToken ct)
    {
        if (!_currentUserService.IsInRole("Admin"))
            throw new ForbiddenException("Only admin can delete tags");

        var actorId = _currentUserService.UserId!.Value;

        await _tagService.SoftDeleteAsync(request.Id, actorId, ct);
    }
}