using MediatR;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Application.Services.Type;
using UteLearningHub.Domain.Exceptions;

namespace UteLearningHub.Application.Features.Type.Commands.CreateType;

public class CreateTypeCommandHandler(ITypeService typeService, ICurrentUserService currentUserService) : IRequestHandler<CreateTypeCommand, TypeDetailDto>
{
    private readonly ITypeService _typeService = typeService;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    public async Task<TypeDetailDto> Handle(CreateTypeCommand request, CancellationToken ct)
    {
        if (!_currentUserService.IsInRole("Admin"))
            throw new ForbiddenException("Only admin can create types");

        var actorId = _currentUserService.UserId!.Value;

        return await _typeService.CreateAsync(actorId, request, ct);
    }
}