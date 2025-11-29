using MediatR;
using Microsoft.EntityFrameworkCore;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;

namespace UteLearningHub.Application.Features.Major.Queries.GetMajorById;

public class GetMajorByIdHandler : IRequestHandler<GetMajorByIdQuery, MajorDetailDto>
{
    private readonly IMajorRepository _majorRepository;

    public GetMajorByIdHandler(IMajorRepository majorRepository)
    {
        _majorRepository = majorRepository;
    }

    public async Task<MajorDetailDto> Handle(GetMajorByIdQuery request, CancellationToken cancellationToken)
    {
        var major = await _majorRepository.GetQueryableSet()
            .Include(m => m.Faculty)
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

        if (major == null)
            throw new NotFoundException($"Major with id {request.Id} not found");

        var subjectCount = await _majorRepository.GetQueryableSet()
            .Where(m => m.Id == request.Id)
            .Select(m => m.SubjectMajors.Count)
            .FirstOrDefaultAsync(cancellationToken);

        return new MajorDetailDto
        {
            Id = major.Id,
            MajorName = major.MajorName,
            MajorCode = major.MajorCode,
            Faculty = new FacultyDto
            {
                Id = major.Faculty.Id,
                FacultyName = major.Faculty.FacultyName,
                FacultyCode = major.Faculty.FacultyCode
            },
            SubjectCount = subjectCount
        };
    }
}
