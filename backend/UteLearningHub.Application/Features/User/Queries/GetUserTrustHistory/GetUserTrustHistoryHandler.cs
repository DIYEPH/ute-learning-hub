using MediatR;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Application.Services.User;
using UteLearningHub.Domain.Exceptions;

namespace UteLearningHub.Application.Features.User.Queries.GetUserTrustHistory;

public class GetUserTrustHistoryHandler : IRequestHandler<GetUserTrustHistoryQuery, IList<UserTrustHistoryDto>>
{
    private readonly IUserService _userService;
    private readonly ICurrentUserService _currentUserService;

    public GetUserTrustHistoryHandler(
        IUserService userService,
        ICurrentUserService currentUserService)
    {
        _userService = userService;
        _currentUserService = currentUserService;
    }

    public async Task<IList<UserTrustHistoryDto>> Handle(GetUserTrustHistoryQuery request, CancellationToken cancellationToken)
    {
        // Only admin can view user trust history
        if (!_currentUserService.IsAuthenticated)
            throw new UnauthorizedException("You must be authenticated to view user trust history");

        var isAdmin = _currentUserService.IsInRole("Admin");
        if (!isAdmin)
            throw new UnauthorizedException("Only administrators can view user trust history");

        // Verify user exists
        var user = await _userService.GetUserByIdAsync(request.UserId, cancellationToken);
        if (user == null)
            throw new NotFoundException($"User with id {request.UserId} not found");

        return await _userService.GetUserTrustHistoryAsync(request.UserId, cancellationToken);
    }
}
