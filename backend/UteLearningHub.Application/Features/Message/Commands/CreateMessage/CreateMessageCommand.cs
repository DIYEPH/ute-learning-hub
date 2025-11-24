using MediatR;
using Microsoft.AspNetCore.Http;
using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Features.Message.Commands.CreateMessage;

public record CreateMessageCommand : CreateMessageRequest, IRequest<MessageDto>
{
    public IList<IFormFile>? Files { get; init; } // Files upload trực tiếp
}