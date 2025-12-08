using MediatR;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;

namespace UteLearningHub.Application.Features.Type.Commands.DeleteType;

public class DeleteTypeCommandHandler : IRequestHandler<DeleteTypeCommand, Unit>
{
    private readonly ITypeRepository _typeRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public DeleteTypeCommandHandler(
        ITypeRepository typeRepository,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider)
    {
        _typeRepository = typeRepository;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Unit> Handle(DeleteTypeCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated)
            throw new UnauthorizedException("You must be authenticated to delete types");

        var userId = _currentUserService.UserId ?? throw new UnauthorizedException();

        var type = await _typeRepository.GetByIdAsync(request.Id, disableTracking: false, cancellationToken);

        if (type == null || type.IsDeleted)
            throw new NotFoundException($"Type with id {request.Id} not found");

        var documentCount = await _typeRepository.GetDocumentCountAsync(request.Id, cancellationToken);

        if (documentCount > 0)
            throw new BadRequestException($"Cannot delete type. It is being used by {documentCount} document(s)");

        await _typeRepository.DeleteAsync(type, userId, cancellationToken);
        await _typeRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}