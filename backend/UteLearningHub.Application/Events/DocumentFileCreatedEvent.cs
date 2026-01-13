using MediatR;

namespace UteLearningHub.Application.Events;

public record DocumentFileCreatedEvent(
    Guid DocumentId,
    Guid DocumentFileId,
    Guid? SubjectId,
    Guid UserId
) : INotification;
