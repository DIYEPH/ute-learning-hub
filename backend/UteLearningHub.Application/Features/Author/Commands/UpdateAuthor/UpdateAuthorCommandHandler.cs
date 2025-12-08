using MediatR;
using UteLearningHub.Application.Features.Author.Queries.GetAuthorById;
using UteLearningHub.Application.Services.Author;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;

namespace UteLearningHub.Application.Features.Author.Commands.UpdateAuthor;

public class UpdateAuthorCommandHandler : IRequestHandler<UpdateAuthorCommand, AuthorDetailDto>
{
    private readonly IAuthorRepository _authorRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IAuthorQueryService _authorQueryService;

    public UpdateAuthorCommandHandler(
        IAuthorRepository authorRepository,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider,
        IAuthorQueryService authorQueryService)
    {
        _authorRepository = authorRepository;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
        _authorQueryService = authorQueryService;
    }

    public async Task<AuthorDetailDto> Handle(UpdateAuthorCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated)
            throw new UnauthorizedException("You must be authenticated to update authors");

        var userId = _currentUserService.UserId ?? throw new UnauthorizedException();

        var author = await _authorRepository.GetByIdAsync(request.Id, disableTracking: false, cancellationToken);

        if (author == null || author.IsDeleted)
            throw new NotFoundException($"Author with id {request.Id} not found");

        if (!string.IsNullOrWhiteSpace(request.FullName) && request.FullName != author.FullName)
        {
            var existingAuthor = await _authorRepository.FindByNameAsync(request.FullName, cancellationToken: cancellationToken);

            if (existingAuthor != null && existingAuthor.Id != request.Id)
                throw new BadRequestException($"Author with name '{request.FullName}' already exists");

            author.FullName = request.FullName;
        }

        if (request.Description != null)
            author.Description = request.Description;

        author.UpdatedById = userId;
        author.UpdatedAt = _dateTimeProvider.OffsetNow;

        await _authorRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        var result = await _authorQueryService.GetByIdAsync(request.Id, cancellationToken);
        return result ?? throw new NotFoundException($"Author with id {request.Id} not found after update");
    }
}
