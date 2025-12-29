using MediatR;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Application.Services.Type;
using UteLearningHub.Domain.Exceptions;

namespace UteLearningHub.Application.Features.Type.Commands.DeleteType;

public class DeleteTypeCommandHandler(ITypeService typeService, ICurrentUserService currentUserService) : IRequestHandler<DeleteTypeCommand>
{
    private readonly ITypeService _typeService = typeService;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    public async Task Handle(DeleteTypeCommand request, CancellationToken ct)
    {
        if (!_currentUserService.IsInRole("Admin"))
            throw new ForbiddenException("Only admin can delete types");

        var actorId = _currentUserService.UserId!.Value;

        await _typeService.SoftDeleteAsync(request.Id, actorId, ct);
    }
}