using Microsoft.EntityFrameworkCore;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Entities;
using UteLearningHub.Domain.Repositories;
using UteLearningHub.Persistence.Repositories.Common;

namespace UteLearningHub.Persistence.Repositories;

public class SubjectRepository : Repository<Subject, Guid>, ISubjectRepository
{
    public SubjectRepository(ApplicationDbContext dbContext, IDateTimeProvider dateTimeProvider) : base(dbContext, dateTimeProvider)
    {
    }

    public async Task AddSubjectMajorRelationshipsAsync(Guid subjectId, List<Guid> majorIds, CancellationToken cancellationToken = default)
    {
        if (!majorIds.Any()) return;

        var subjectMajors = majorIds.Select(majorId => new SubjectMajor
        {
            SubjectId = subjectId,
            MajorId = majorId
        }).ToList();

        await _dbContext.Set<SubjectMajor>().AddRangeAsync(subjectMajors, cancellationToken);
    }

    public async Task UpdateSubjectMajorRelationshipsAsync(Guid subjectId, List<Guid> majorIds, CancellationToken cancellationToken = default)
    {
        // Remove existing relationships
        var existingRelationships = await _dbContext.Set<SubjectMajor>()
            .Where(sm => sm.SubjectId == subjectId)
            .ToListAsync(cancellationToken);

        if (existingRelationships.Any())
        {
            _dbContext.Set<SubjectMajor>().RemoveRange(existingRelationships);
        }

        // Add new relationships
        if (majorIds.Any())
        {
            await AddSubjectMajorRelationshipsAsync(subjectId, majorIds, cancellationToken);
        }
    }

    public async Task RemoveSubjectMajorRelationshipsAsync(Guid subjectId, CancellationToken cancellationToken = default)
    {
        var relationships = await _dbContext.Set<SubjectMajor>()
            .Where(sm => sm.SubjectId == subjectId)
            .ToListAsync(cancellationToken);

        if (relationships.Any())
        {
            _dbContext.Set<SubjectMajor>().RemoveRange(relationships);
        }
    }
}
