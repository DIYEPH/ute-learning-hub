using MediatR;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;

namespace UteLearningHub.Application.Features.Type.Commands.UpdateType;

public class UpdateTypeCommandHandler : IRequestHandler<UpdateTypeCommand, TypeDetailDto>
{
    private readonly ITypeRepository _typeRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public UpdateTypeCommandHandler(
        ITypeRepository typeRepository,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider)
    {
        _typeRepository = typeRepository;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<TypeDetailDto> Handle(UpdateTypeCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated)
            throw new UnauthorizedException("You must be authenticated to update types");

        var userId = _currentUserService.UserId ?? throw new UnauthorizedException();

        if (string.IsNullOrWhiteSpace(request.TypeName))
            throw new BadRequestException("TypeName cannot be empty");

        var type = await _typeRepository.GetByIdAsync(request.Id, disableTracking: false, cancellationToken);

        if (type == null || type.IsDeleted)
            throw new NotFoundException($"Type with id {request.Id} not found");

        var existingType = await _typeRepository.FindByNameAsync(request.TypeName, cancellationToken: cancellationToken);

        if (existingType != null && existingType.Id != request.Id)
            throw new BadRequestException($"Type with name '{request.TypeName}' already exists");

        type.TypeName = request.TypeName;
        type.UpdatedAt = _dateTimeProvider.OffsetNow;

        await _typeRepository.UpdateAsync(type, cancellationToken);
        await _typeRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        var documentCount = await _typeRepository.GetDocumentCountAsync(request.Id, cancellationToken);

        return new TypeDetailDto
        {
            Id = type.Id,
            TypeName = type.TypeName,
            DocumentCount = documentCount
        };
    }
}
