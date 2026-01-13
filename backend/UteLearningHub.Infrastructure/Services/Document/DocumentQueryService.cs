using Microsoft.EntityFrameworkCore;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Document;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Persistence;

namespace UteLearningHub.Infrastructure.Services.Document;

public class DocumentQueryService : IDocumentQueryService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public DocumentQueryService(ApplicationDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<DocumentDetailDto?> GetDetailByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var currentUserId = _currentUserService.UserId;
        
        return await _dbContext.Documents
            .AsNoTracking()
            .Where(d => d.Id == id && !d.IsDeleted)
            .Select(d => new DocumentDetailDto
            {
                Id = d.Id,
                DocumentName = d.DocumentName,
                Description = d.Description,
                Visibility = d.Visibility,
                CoverFileId = d.CoverFileId,
                Subject = d.Subject != null ? new SubjectDto
                {
                    Id = d.Subject.Id,
                    SubjectName = d.Subject.SubjectName,
                    SubjectCode = d.Subject.SubjectCode
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
                        TotalPages = df.TotalPages,
                        CoverFileId = df.CoverFileId,
                        Status = df.Status,
                        ReviewedById = df.ReviewedById,
                        ReviewedAt = df.ReviewedAt,
                        ReviewNote = df.ReviewNote,
                        CommentCount = df.Comments.Count(c => !c.IsDeleted),
                        UsefulCount = d.Reviews.Count(r => r.DocumentFileId == df.Id && r.DocumentReviewType == DocumentReviewType.Useful),
                        NotUsefulCount = d.Reviews.Count(r => r.DocumentFileId == df.Id && r.DocumentReviewType == DocumentReviewType.NotUseful),
                        ViewCount = df.ViewCount,
                        MyReviewType = currentUserId.HasValue 
                            ? d.Reviews.Where(r => r.DocumentFileId == df.Id && r.CreatedById == currentUserId.Value)
                                .Select(r => (DocumentReviewType?)r.DocumentReviewType)
                                .FirstOrDefault()
                            : null
                    }).ToList(),
                UsefulCount = d.Reviews.Count(r => r.DocumentReviewType == DocumentReviewType.Useful),
                NotUsefulCount = d.Reviews.Count(r => r.DocumentReviewType == DocumentReviewType.NotUseful),
                CommentCount = d.DocumentFiles.Where(df => !df.IsDeleted).Sum(df => df.Comments.Count(c => !c.IsDeleted)),
                TotalViewCount = d.DocumentFiles.Where(df => !df.IsDeleted).Sum(df => df.ViewCount),
                CreatedById = d.CreatedById,
                CreatedByName = _dbContext.Users.Where(u => u.Id == d.CreatedById).Select(u => u.FullName ?? u.UserName).FirstOrDefault(),
                CreatedByAvatarUrl = _dbContext.Users.Where(u => u.Id == d.CreatedById).Select(u => u.AvatarUrl).FirstOrDefault(),
                CreatedAt = d.CreatedAt,
                UpdatedAt = d.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<DocumentFileDto?> GetDocumentFileByIdAsync(Guid fileId, CancellationToken cancellationToken = default)
    {
        var currentUserId = _currentUserService.UserId;

        return await _dbContext.DocumentFiles
            .AsNoTracking()
            .Where(df => df.Id == fileId && !df.IsDeleted)
            .Select(df => new DocumentFileDto
            {
                Id = df.Id,
                FileId = df.FileId,
                FileSize = df.File.FileSize,
                MimeType = df.File.MimeType,
                Title = df.Title,
                Order = df.Order,
                TotalPages = df.TotalPages,
                CoverFileId = df.CoverFileId,
                Status = df.Status,
                ReviewedById = df.ReviewedById,
                ReviewedAt = df.ReviewedAt,
                ReviewNote = df.ReviewNote,
                CommentCount = df.Comments.Count(c => !c.IsDeleted),
                UsefulCount = df.Document.Reviews.Count(r => r.DocumentFileId == df.Id && r.DocumentReviewType == DocumentReviewType.Useful),
                NotUsefulCount = df.Document.Reviews.Count(r => r.DocumentFileId == df.Id && r.DocumentReviewType == DocumentReviewType.NotUseful),
                ViewCount = df.ViewCount,
                MyReviewType = currentUserId.HasValue
                    ? df.Document.Reviews.Where(r => r.DocumentFileId == df.Id && r.CreatedById == currentUserId.Value)
                        .Select(r => (DocumentReviewType?)r.DocumentReviewType)
                        .FirstOrDefault()
                    : null
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
