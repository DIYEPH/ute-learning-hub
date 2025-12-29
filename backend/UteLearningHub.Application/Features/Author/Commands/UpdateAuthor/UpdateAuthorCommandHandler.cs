using MediatR;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Author;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Domain.Exceptions;

namespace UteLearningHub.Application.Features.Author.Commands.UpdateAuthor;

public class UpdateAuthorCommandHandler(IAuthorService authorService, ICurrentUserService currentUserService) : IRequestHandler<UpdateAuthorCommand, AuthorDetailDto>
{
    private readonly IAuthorService _authorService = authorService;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    public async Task<AuthorDetailDto> Handle(UpdateAuthorCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsInRole("Admin"))
            throw new ForbiddenException("Only admin can delete authors");

        var actorId = _currentUserService.UserId!.Value;

        return await _authorService.UpdateAsync(actorId, request, cancellationToken);
    }
}
