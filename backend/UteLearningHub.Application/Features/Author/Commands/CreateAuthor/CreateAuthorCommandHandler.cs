using MediatR;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Author;
using UteLearningHub.Application.Services.Identity;

namespace UteLearningHub.Application.Features.Author.Commands.CreateAuthor;

public class CreateAuthorCommandHandler(ICurrentUserService currentUserService, IAuthorService authorService) : IRequestHandler<CreateAuthorCommand, AuthorDetailDto>
{
    private readonly IAuthorService _authorService = authorService;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    public async Task<AuthorDetailDto> Handle(CreateAuthorCommand request, CancellationToken ct)
    {
        var actor = _currentUserService.UserId!.Value;
        return await _authorService.CreateAsync(actor, request, ct);
    }
}
