using MediatR;
using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Features.Message.Queries.GetMessages;

public record GetMessagesQuery : GetMessagesRequest, IRequest<PagedResponse<MessageDto>>;