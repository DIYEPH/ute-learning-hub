using MediatR;

namespace UteLearningHub.Application.Features.Auth.Commands.CompleteAccountSetup;

public record CompleteAccountSetupCommand(string? Username, string? Password) 
    : IRequest<CompleteAccountSetupResponse>;
