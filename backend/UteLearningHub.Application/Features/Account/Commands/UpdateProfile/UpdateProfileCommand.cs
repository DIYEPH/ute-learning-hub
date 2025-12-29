using MediatR;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Domain.Constaints.Enums;

namespace UteLearningHub.Application.Features.Account.Commands.UpdateProfile;

public record UpdateProfileCommand : UpdateProfileCommandRequest, IRequest<ProfileDetailDto>
{
    public Guid Id { get; init; }
}