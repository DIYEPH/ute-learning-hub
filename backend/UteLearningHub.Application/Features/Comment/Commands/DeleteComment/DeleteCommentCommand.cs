using MediatR;

namespace UteLearningHub.Application.Features.Comment.Commands.DeleteComment;

public record DeleteCommentCommand : DeleteCommentRequest, IRequest<Unit>;
