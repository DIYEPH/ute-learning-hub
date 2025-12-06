using MediatR;
using Microsoft.EntityFrameworkCore;
using UteLearningHub.Application.Features.Author.Queries.GetAuthorById;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;
using AuthorEntity = UteLearningHub.Domain.Entities.Author;

namespace UteLearningHub.Application.Features.Author.Commands.CreateAuthor;

public class CreateAuthorCommandHandler : IRequestHandler<CreateAuthorCommand, AuthorDetailDto>
{
    private readonly IAuthorRepository _authorRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public CreateAuthorCommandHandler(
        IAuthorRepository authorRepository,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider)
    {
        _authorRepository = authorRepository;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<AuthorDetailDto> Handle(CreateAuthorCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated)
            throw new UnauthorizedException("You must be authenticated to create authors");

        var userId = _currentUserService.UserId ?? throw new UnauthorizedException();

        if (string.IsNullOrWhiteSpace(request.FullName))
            throw new BadRequestException("FullName cannot be empty");

        // Check if author name already exists
        var existingAuthor = await _authorRepository.GetQueryableSet()
            .Where(a => a.FullName.ToLower() == request.FullName.ToLower() && !a.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        if (existingAuthor != null)
            throw new BadRequestException($"Author with name '{request.FullName}' already exists");

        var author = new AuthorEntity
        {
            Id = Guid.NewGuid(),
            FullName = request.FullName,
            Description = request.Description ?? string.Empty,
            ReviewStatus = ReviewStatus.Approved,
            CreatedById = userId,
            CreatedAt = _dateTimeProvider.OffsetNow
        };

        await _authorRepository.AddAsync(author, cancellationToken);
        await _authorRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthorDetailDto
        {
            Id = author.Id,
            FullName = author.FullName,
            Description = author.Description,
            DocumentCount = 0
        };
    }
}
