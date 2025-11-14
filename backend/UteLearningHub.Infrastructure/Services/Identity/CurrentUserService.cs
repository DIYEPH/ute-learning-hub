using UteLearningHub.Application.Services.Identity;

namespace UteLearningHub.Infrastructure.Services.Identity
{
    public class CurrentUserService : ICurrentUserService
    {
        public Guid? UserId => throw new NotImplementedException();

        public string? Email => throw new NotImplementedException();

        public bool IsAuthenticated => throw new NotImplementedException();

        public bool IsInRole(string role)
        {
            throw new NotImplementedException();
        }
    }
}
