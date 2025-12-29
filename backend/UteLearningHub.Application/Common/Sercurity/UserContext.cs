namespace UteLearningHub.Application.Common.Sercurity;

public record UserContext(Guid? UserId, bool IsAuthenticated, bool IsAdmin);