using MediatR;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;

namespace UteLearningHub.Application.Features.Author.Commands.DeleteAuthor;

public class DeleteAuthorCommandHandler : IRequestHandler<DeleteAuthorCommand>
{
    private readonly IAuthorRepository _authorRepository;
    private readonly ICurrentUserService _currentUserService;

    public DeleteAuthorCommandHandler(
        IAuthorRepository authorRepository,
        ICurrentUserService currentUserService)
    {
        _authorRepository = authorRepository;
        _currentUserService = currentUserService;
    }

    public async Task Handle(DeleteAuthorCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated)
            throw new UnauthorizedException("You must be authenticated to delete authors");

        var userId = _currentUserService.UserId ?? throw new UnauthorizedException();

        var author = await _authorRepository.GetByIdAsync(request.Id, disableTracking: false, cancellationToken);

        if (author == null || author.IsDeleted)
            throw new NotFoundException($"Author with id {request.Id} not found");

        await _authorRepository.DeleteAsync(author, userId, cancellationToken);
        await _authorRepository.UnitOfWork.SaveChangesAsync(cancellationToken);
    }
}
