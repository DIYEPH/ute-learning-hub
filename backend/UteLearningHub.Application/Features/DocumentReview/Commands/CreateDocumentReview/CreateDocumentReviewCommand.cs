using MediatR;
using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Features.DocumentReview.Commands.CreateDocumentReview;

public record CreateDocumentReviewCommand : CreateDocumentReviewRequest, IRequest<DocumentReviewDto>;
