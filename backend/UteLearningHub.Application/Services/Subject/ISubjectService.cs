using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Features.Subject.Commands.CreateSubject;
using UteLearningHub.Application.Features.Subject.Commands.UpdateSubject;
using UteLearningHub.Application.Features.Subject.Queries.GetSubjects;

namespace UteLearningHub.Application.Services.Subject;

public interface ISubjectService
{
    Task<SubjectDetailDto> GetSubjectByIdAsync(Guid id, bool isAdmin, CancellationToken ct);
    Task<PagedResponse<SubjectDetailDto>> GetSubjectsAsync(GetSubjectsQuery request, bool isAdmin, CancellationToken ct);
    Task<SubjectDetailDto> CreateAsync(Guid creatorId, CreateSubjectCommand request, CancellationToken ct);
    Task<SubjectDetailDto> UpdateAsync(Guid actorId, UpdateSubjectCommand request, CancellationToken ct);
    Task SoftDeleteAsync(Guid subjectId, Guid actorId, CancellationToken ct);
}
