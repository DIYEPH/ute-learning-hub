using MediatR;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;

namespace UteLearningHub.Application.Features.Tag.Commands.DeleteTag;

public class DeleteTagCommandHandler : IRequestHandler<DeleteTagCommand, Unit>
{
    private readonly ITagRepository _tagRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public DeleteTagCommandHandler(
        ITagRepository tagRepository,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider)
    {
        _tagRepository = tagRepository;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Unit> Handle(DeleteTagCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated)
            throw new UnauthorizedException("You must be authenticated to delete tags");

        var userId = _currentUserService.UserId ?? throw new UnauthorizedException();

        var tag = await _tagRepository.GetByIdAsync(request.Id, disableTracking: false, cancellationToken);

        if (tag == null || tag.IsDeleted)
            throw new NotFoundException($"Tag with id {request.Id} not found");

        var isAdmin = _currentUserService.IsInRole("Admin");
        if (!isAdmin && tag.CreatedById != userId)
            throw new UnauthorizedException("You don't have permission to delete this tag");

        var documentCount = await _tagRepository.GetDocumentCountAsync(request.Id, cancellationToken);

        if (documentCount > 0)
            throw new BadRequestException($"Cannot delete tag. It is being used by {documentCount} document(s)");

        await _tagRepository.DeleteAsync(tag, userId, cancellationToken);
        await _tagRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}