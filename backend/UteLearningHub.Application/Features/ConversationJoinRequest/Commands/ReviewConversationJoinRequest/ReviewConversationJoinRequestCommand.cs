using MediatR;

namespace UteLearningHub.Application.Features.ConversationJoinRequest.Commands.ReviewConversationJoinRequest;

public record ReviewConversationJoinRequestCommand : ReviewConversationJoinRequestRequest, IRequest<Unit>;
