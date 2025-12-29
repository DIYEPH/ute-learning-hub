using MediatR;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Application.Services.Type;
using UteLearningHub.Domain.Exceptions;

namespace UteLearningHub.Application.Features.Type.Commands.UpdateType;

public class UpdateTypeCommandHandler(ITypeService typeService, ICurrentUserService currentUserService) : IRequestHandler<UpdateTypeCommand, TypeDetailDto>
{
    private readonly ITypeService _typeService = typeService;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    public async Task<TypeDetailDto> Handle(UpdateTypeCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsInRole("Admin"))
            throw new ForbiddenException("Only admin can delete types");

        var actorId = _currentUserService.UserId!.Value;

        return await _typeService.UpdateAsync(actorId, request, cancellationToken);
    }
}
