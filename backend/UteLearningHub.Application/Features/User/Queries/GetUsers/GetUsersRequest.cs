using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Domain.Constaints.Enums;

namespace UteLearningHub.Application.Features.User.Queries.GetUsers;

public record GetUsersRequest : PagedRequest
{
    public string? SearchTerm { get; init; }
    public Guid? MajorId { get; init; }
    public TrustLever? TrustLevel { get; init; }
    public bool? EmailConfirmed { get; init; }
    public bool? IsDeleted { get; init; }
    public string? SortBy { get; init; } // "fullName", "email", "createdAt", "trustScore", "lastLoginAt"
    public bool SortDescending { get; init; } = true;
}