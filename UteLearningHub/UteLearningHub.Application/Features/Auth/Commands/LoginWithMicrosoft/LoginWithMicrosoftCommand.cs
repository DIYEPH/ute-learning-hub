using MediatR;

namespace UteLearningHub.Application.Features.Auth.Commands.LoginWithMicrosoft;

public record LoginWithMicrosoftCommand : LoginWithMicrosoftRequest, IRequest<LoginWithMicrosoftResponse>
{
}
