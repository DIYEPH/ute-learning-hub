using MediatR;
using UteLearningHub.Domain.Constaints.Enums;

namespace UteLearningHub.Application.Events;

public record DocumentReviewedEvent(
    Guid DocumentFileId,
    Guid ReviewerId,
    Guid? CreatorId,
    DocumentReviewType? OldType,
    DocumentReviewType? NewType
) : INotification;
