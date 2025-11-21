using MediatR;

namespace UteLearningHub.Application.Features.Account.Queries.GetProfile;

public record GetProfileQuery : GetProfileRequest, IRequest<GetProfileResponse>;
