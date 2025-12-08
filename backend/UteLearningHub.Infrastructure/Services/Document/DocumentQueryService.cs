using Microsoft.EntityFrameworkCore;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Document;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Persistence;

namespace UteLearningHub.Infrastructure.Services.Document;

public class DocumentQueryService : IDocumentQueryService
{
    private readonly ApplicationDbContext _dbContext;

    public DocumentQueryService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DocumentDetailDto?> GetDetailByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Documents
            .AsNoTracking()
            .Where(d => d.Id == id && !d.IsDeleted)
            .Select(d => new DocumentDetailDto
            {
                Id = d.Id,
                DocumentName = d.DocumentName,
                Description = d.Description,
                IsDownload = d.IsDownload,
                Visibility = d.Visibility,
                CoverFileId = d.CoverFileId,
                Subject = d.Subject != null ? new SubjectDto
                {
                    Id = d.Subject.Id,
                    SubjectName = d.Subject.SubjectName,
                    SubjectCode = d.Subject.SubjectCode,
                    Majors = d.Subject.SubjectMajors.Select(sm => new MajorDto
                    {
                        Id = sm.Major.Id,
                        MajorName = sm.Major.MajorName,
                        MajorCode = sm.Major.MajorCode,
                        Faculty = sm.Major.Faculty != null ? new FacultyDto
                        {
                            Id = sm.Major.Faculty.Id,
                            FacultyName = sm.Major.Faculty.FacultyName,
                            FacultyCode = sm.Major.Faculty.FacultyCode,
                            Logo = sm.Major.Faculty.Logo
                        } : null
                    }).ToList()
                } : null,
                Type = new TypeDto
                {
                    Id = d.Type.Id,
                    TypeName = d.Type.TypeName
                },
                Tags = d.DocumentTags.Select(dt => new TagDto
                {
                    Id = dt.Tag.Id,
                    TagName = dt.Tag.TagName
                }).ToList(),
                Authors = d.DocumentAuthors.Select(da => new AuthorDto
                {
                    Id = da.Author.Id,
                    FullName = da.Author.FullName
                }).ToList(),
                Files = d.DocumentFiles
                    .Where(df => !df.IsDeleted)
                    .OrderBy(df => df.Order)
                    .ThenBy(df => df.CreatedAt)
                    .Select(df => new DocumentFileDto
                    {
                        Id = df.Id,
                        FileId = df.FileId,
                        FileSize = df.File.FileSize,
                        MimeType = df.File.MimeType,
                        Title = df.Title,
                        Order = df.Order,
                        IsPrimary = df.IsPrimary,
                        TotalPages = df.TotalPages,
                        CoverFileId = df.CoverFileId,
                        ReviewStatus = df.ReviewStatus,
                        ReviewedById = df.ReviewedById,
                        ReviewedAt = df.ReviewedAt,
                        ReviewNote = df.ReviewNote,
                        CommentCount = df.Comments.Count(c => !c.IsDeleted),
                        UsefulCount = d.Reviews.Count(r => r.DocumentFileId == df.Id && r.DocumentReviewType == DocumentReviewType.Useful),
                        NotUsefulCount = d.Reviews.Count(r => r.DocumentFileId == df.Id && r.DocumentReviewType == DocumentReviewType.NotUseful)
                    }).ToList(),
                UsefulCount = d.Reviews.Count(r => r.DocumentReviewType == DocumentReviewType.Useful),
                NotUsefulCount = d.Reviews.Count(r => r.DocumentReviewType == DocumentReviewType.NotUseful),
                CommentCount = d.DocumentFiles.Where(df => !df.IsDeleted).Sum(df => df.Comments.Count(c => !c.IsDeleted)),
                CreatedById = d.CreatedById,
                CreatedAt = d.CreatedAt,
                UpdatedAt = d.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
