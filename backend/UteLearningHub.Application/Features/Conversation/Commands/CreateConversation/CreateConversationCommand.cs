using MediatR;
using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Features.Conversation.Commands.CreateConversation;

public record CreateConversationCommand : CreateConversationRequest, IRequest<ConversationDetailDto>;