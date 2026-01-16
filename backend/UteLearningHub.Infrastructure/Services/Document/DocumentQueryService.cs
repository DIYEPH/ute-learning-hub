using Microsoft.EntityFrameworkCore;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Document;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Persistence;

namespace UteLearningHub.Infrastructure.Services.Document;

public class DocumentQueryService(ApplicationDbContext db, ICurrentUserService currentUserService) : IDocumentQueryService
{
    public async Task<DocumentDetailDto?> GetDetailByIdAsync(Guid id, CancellationToken ct = default)
    {
        var userId = currentUserService.UserId;

        return await db.Documents
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
                Type = new TypeDto { Id = d.Type.Id, TypeName = d.Type.TypeName },
                Tags = d.DocumentTags.Select(dt => new TagDto { Id = dt.Tag.Id, TagName = dt.Tag.TagName }).ToList(),
                Authors = d.DocumentAuthors.Select(da => new AuthorDto { Id = da.Author.Id, FullName = da.Author.FullName }).ToList(),
                Files = d.DocumentFiles
                    .Where(df => !df.IsDeleted)
                    .OrderBy(df => df.Order).ThenBy(df => df.CreatedAt)
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
                        MyReviewType = userId.HasValue
                            ? d.Reviews.Where(r => r.DocumentFileId == df.Id && r.CreatedById == userId.Value)
                                .Select(r => (DocumentReviewType?)r.DocumentReviewType).FirstOrDefault()
                            : null
                    }).ToList(),
                UsefulCount = d.Reviews.Count(r => r.DocumentReviewType == DocumentReviewType.Useful),
                NotUsefulCount = d.Reviews.Count(r => r.DocumentReviewType == DocumentReviewType.NotUseful),
                CommentCount = d.DocumentFiles.Where(df => !df.IsDeleted).Sum(df => df.Comments.Count(c => !c.IsDeleted)),
                TotalViewCount = d.DocumentFiles.Where(df => !df.IsDeleted).Sum(df => df.ViewCount),
                CreatedById = d.CreatedById,
                CreatedByName = db.Users.Where(u => u.Id == d.CreatedById).Select(u => u.FullName ?? u.UserName).FirstOrDefault(),
                CreatedByAvatarUrl = db.Users.Where(u => u.Id == d.CreatedById).Select(u => u.AvatarUrl).FirstOrDefault(),
                CreatedAt = d.CreatedAt,
                UpdatedAt = d.UpdatedAt
            })
            .FirstOrDefaultAsync(ct);
    }

    public async Task<DocumentFileDto?> GetDocumentFileByIdAsync(Guid fileId, CancellationToken ct = default)
    {
        var userId = currentUserService.UserId;

        return await db.DocumentFiles
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
                MyReviewType = userId.HasValue
                    ? df.Document.Reviews.Where(r => r.DocumentFileId == df.Id && r.CreatedById == userId.Value)
                        .Select(r => (DocumentReviewType?)r.DocumentReviewType).FirstOrDefault()
                    : null
            })
            .FirstOrDefaultAsync(ct);
    }
}
