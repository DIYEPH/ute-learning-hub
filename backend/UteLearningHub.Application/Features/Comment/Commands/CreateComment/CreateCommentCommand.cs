using MediatR;
using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Features.Comment.Commands.CreateComment;

public record CreateCommentCommand : CreateCommentRequest, IRequest<CommentDto>;
