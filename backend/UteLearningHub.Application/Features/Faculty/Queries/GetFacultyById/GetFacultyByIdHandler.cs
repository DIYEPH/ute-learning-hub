using MediatR;
using Microsoft.EntityFrameworkCore;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;

namespace UteLearningHub.Application.Features.Faculty.Queries.GetFacultyById;

public class GetFacultyByIdHandler : IRequestHandler<GetFacultyByIdQuery, FacultyDetailDto>
{
    private readonly IFacultyRepository _facultyRepository;

    public GetFacultyByIdHandler(IFacultyRepository facultyRepository)
    {
        _facultyRepository = facultyRepository;
    }

    public async Task<FacultyDetailDto> Handle(GetFacultyByIdQuery request, CancellationToken cancellationToken)
    {
        var faculty = await _facultyRepository.GetByIdAsync(request.Id, disableTracking: true, cancellationToken);

        if (faculty == null)
            throw new NotFoundException($"Faculty with id {request.Id} not found");

        var majorCount = await _facultyRepository.GetQueryableSet()
            .Where(f => f.Id == request.Id)
            .Select(f => f.Majors.Count)
            .FirstOrDefaultAsync(cancellationToken);

        return new FacultyDetailDto
        {
            Id = faculty.Id,
            FacultyName = faculty.FacultyName,
            FacultyCode = faculty.FacultyCode,
            MajorCount = majorCount
        };
    }
}