using MediatR;
using UteLearningHub.Application.Services.Author;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Domain.Exceptions;

namespace UteLearningHub.Application.Features.Author.Commands.DeleteAuthor;

public class DeleteAuthorCommandHandler(IAuthorService authorService, ICurrentUserService currentUserService) : IRequestHandler<DeleteAuthorCommand>
{
    private readonly IAuthorService _authorService = authorService;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    public async Task Handle(DeleteAuthorCommand request, CancellationToken ct)
    {
        if (!_currentUserService.IsInRole("Admin"))
            throw new ForbiddenException("Only admin can delete authors");

        var actorId = _currentUserService.UserId!.Value;

        await _authorService.SoftDeleteAsync(request.Id, actorId, ct);
    }
}
