using MediatR;
using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Features.Conversation.Commands.UpdateConversation;

public record UpdateConversationCommand : UpdateConversationRequest, IRequest<ConversationDetailDto>;
