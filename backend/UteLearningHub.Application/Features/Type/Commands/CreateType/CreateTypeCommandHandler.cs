using MediatR;
using Microsoft.EntityFrameworkCore;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;
using TypeEntity = UteLearningHub.Domain.Entities.Type;

namespace UteLearningHub.Application.Features.Type.Commands.CreateType;

public class CreateTypeCommandHandler : IRequestHandler<CreateTypeCommand, TypeDetailDto>
{
    private readonly ITypeRepository _typeRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public CreateTypeCommandHandler(
        ITypeRepository typeRepository,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider)
    {
        _typeRepository = typeRepository;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<TypeDetailDto> Handle(CreateTypeCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated)
            throw new UnauthorizedException("You must be authenticated to create types");

        var userId = _currentUserService.UserId ?? throw new UnauthorizedException();

        // Validate TypeName is not empty
        if (string.IsNullOrWhiteSpace(request.TypeName))
            throw new BadRequestException("TypeName cannot be empty");

        // Check if type name already exists
        var existingType = await _typeRepository.GetQueryableSet()
            .Where(t => t.TypeName.ToLower() == request.TypeName.ToLower() && !t.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        if (existingType != null)
            throw new BadRequestException($"Type with name '{request.TypeName}' already exists");

        // Create type
        var type = new TypeEntity
        {
            Id = Guid.NewGuid(),
            TypeName = request.TypeName,
            CreatedAt = _dateTimeProvider.OffsetNow
        };

        await _typeRepository.AddAsync(type, cancellationToken);
        await _typeRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        return new TypeDetailDto
        {
            Id = type.Id,
            TypeName = type.TypeName,
            DocumentCount = 0
        };
    }
}