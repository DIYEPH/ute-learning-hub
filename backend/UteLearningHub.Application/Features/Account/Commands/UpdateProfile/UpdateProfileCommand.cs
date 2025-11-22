using MediatR;
using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Features.Account.Commands.UpdateProfile;

public record UpdateProfileCommand : UpdateProfileRequest, IRequest<ProfileDto>;