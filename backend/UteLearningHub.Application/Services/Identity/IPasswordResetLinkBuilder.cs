namespace UteLearningHub.Application.Services.Identity;

public interface IPasswordResetLinkBuilder
{
    string BuildResetLink(string email, string token);
}


