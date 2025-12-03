using MediatR;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;
using DocumentReviewEntity = UteLearningHub.Domain.Entities.DocumentReview;

namespace UteLearningHub.Application.Features.DocumentReview.Commands.CreateDocumentReview;

public class CreateDocumentReviewCommandHandler : IRequestHandler<CreateDocumentReviewCommand, DocumentReviewDto>
{
    private readonly IDocumentReviewRepository _documentReviewRepository;
    private readonly IDocumentRepository _documentRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public CreateDocumentReviewCommandHandler(
        IDocumentReviewRepository documentReviewRepository,
        IDocumentRepository documentRepository,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider)
    {
        _documentReviewRepository = documentReviewRepository;
        _documentRepository = documentRepository;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<DocumentReviewDto> Handle(CreateDocumentReviewCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated)
            throw new UnauthorizedException("You must be authenticated to review documents");

        var userId = _currentUserService.UserId ?? throw new UnauthorizedException();

        var documentId = await _documentRepository.GetDocumentIdByDocumentFileIdAsync(request.DocumentFileId, cancellationToken);
        if (!documentId.HasValue)
            throw new NotFoundException($"Document file with id {request.DocumentFileId} not found");

        var existingReview = await _documentReviewRepository.GetByDocumentFileIdAndUserIdAsync(
            request.DocumentFileId,
            userId,
            disableTracking: false,
            cancellationToken);
        if (existingReview != null && existingReview.DocumentReviewType == request.DocumentReviewType)
        {
            await _documentReviewRepository.DeleteAsync(existingReview, cancellationToken);
            await _documentReviewRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

            return new DocumentReviewDto
            {
                Id = existingReview.Id,
                DocumentId = existingReview.DocumentId,
                DocumentFileId = existingReview.DocumentFileId,
                DocumentReviewType = existingReview.DocumentReviewType,
                CreatedById = existingReview.CreatedById,
                CreatedAt = existingReview.CreatedAt,
                UpdatedAt = _dateTimeProvider.OffsetNow
            };
        }

        DocumentReviewEntity review;

        if (existingReview != null)
        {
            existingReview.DocumentReviewType = request.DocumentReviewType;
            existingReview.UpdatedById = userId;
            existingReview.UpdatedAt = _dateTimeProvider.OffsetNow;
            await _documentReviewRepository.UpdateAsync(existingReview, cancellationToken);
            review = existingReview;
        }
        else
        {
            review = new DocumentReviewEntity
            {
                Id = Guid.NewGuid(),
                DocumentId = documentId.Value,
                DocumentFileId = request.DocumentFileId,
                DocumentReviewType = request.DocumentReviewType,
                CreatedById = userId,
                CreatedAt = _dateTimeProvider.OffsetNow
            };
            await _documentReviewRepository.AddAsync(review, cancellationToken);
        }

        await _documentReviewRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        return new DocumentReviewDto
        {
            Id = review.Id,
            DocumentId = review.DocumentId,
            DocumentFileId = review.DocumentFileId,
            DocumentReviewType = review.DocumentReviewType,
            CreatedById = review.CreatedById,
            CreatedAt = review.CreatedAt,
            UpdatedAt = review.UpdatedAt
        };
    }
}
