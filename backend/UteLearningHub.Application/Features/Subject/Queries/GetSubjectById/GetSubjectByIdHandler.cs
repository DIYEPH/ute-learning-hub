using MediatR;
using Microsoft.EntityFrameworkCore;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;
using UteLearningHub.Application.Services.Identity;

namespace UteLearningHub.Application.Features.Subject.Queries.GetSubjectById;

public class GetSubjectByIdHandler : IRequestHandler<GetSubjectByIdQuery, SubjectDetailDto>
{
    private readonly ISubjectRepository _subjectRepository;
    private readonly ICurrentUserService _currentUserService;
    public GetSubjectByIdHandler(ISubjectRepository subjectRepository, ICurrentUserService currentUserService)
    {
        _subjectRepository = subjectRepository;
        _currentUserService = currentUserService;
    }

    public async Task<SubjectDetailDto> Handle(GetSubjectByIdQuery request, CancellationToken cancellationToken)
    {
        var subject = await _subjectRepository.GetQueryableSet()
            .Include(s => s.Major)
                .ThenInclude(m => m.Faculty)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (subject == null)
            throw new NotFoundException($"Subject with id {request.Id} not found");

        var isAdmin = _currentUserService.IsAuthenticated && _currentUserService.IsInRole("Admin");
        if (!isAdmin && subject.ReviewStatus != ReviewStatus.Approved)
            throw new NotFoundException($"Subject with id {request.Id} not found");

        var documentCount = await _subjectRepository.GetQueryableSet()
            .Where(s => s.Id == request.Id)
            .Select(s => s.Documents.Count)
            .FirstOrDefaultAsync(cancellationToken);

        return new SubjectDetailDto
        {
            Id = subject.Id,
            SubjectName = subject.SubjectName,
            SubjectCode = subject.SubjectCode,
            Major = new MajorDto
            {
                Id = subject.Major.Id,
                MajorName = subject.Major.MajorName,
                MajorCode = subject.Major.MajorCode,
                Faculty = subject.Major.Faculty != null ? new FacultyDto
                {
                    Id = subject.Major.Faculty.Id,
                    FacultyName = subject.Major.Faculty.FacultyName,
                    FacultyCode = subject.Major.Faculty.FacultyCode
                } : null
            },
            DocumentCount = documentCount
        };
    }
}
