using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Application.Services.Recommendation;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;

namespace UteLearningHub.Application.Features.Document.Queries.GetDocumentRecommendations;

public class GetDocumentRecommendationsHandler(
    IEmbeddingService embeddingService,
    IRecommendationService recommendationService,
    IProfileVectorStore profileVectorStore,
    IDocumentRepository documentRepository,
    ICurrentUserService currentUserService,
    IUserDataRepository userDataRepository,
    ILogger<GetDocumentRecommendationsHandler> logger
) : IRequestHandler<GetDocumentRecommendationsQuery, GetDocumentRecommendationsResponse>
{
    public async Task<GetDocumentRecommendationsResponse> Handle(
        GetDocumentRecommendationsQuery request,
        CancellationToken ct)
    {
        logger.LogInformation("Lấy gợi ý tài liệu");

        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedException("You must be authenticated to get recommendations");

        var userId = currentUserService.UserId ?? throw new UnauthorizedException();

        // 1. lấy vector user
        var userVector = await GetOrCalcUserVectorAsync(userId, ct);
        logger.LogInformation("lấy vector user: {Len} dims", userVector.Length);

        // 2. lấy tl chưa review
        var documents = await documentRepository.GetQueryableSet()
            .Include(d => d.Subject)
            .Include(d => d.Type)
            .Include(d => d.DocumentTags).ThenInclude(dt => dt.Tag)
            .Include(d => d.DocumentFiles.Where(df => !df.IsDeleted && df.Status == ContentStatus.Approved))
                .ThenInclude(df => df.Comments.Where(c => !c.IsDeleted))
            .Include(d => d.Reviews)
            .Include(d => d.DocumentAuthors).ThenInclude(da => da.Author)
            .AsNoTracking()
            .Where(d => !d.IsDeleted
                && d.DocumentFiles.Any(df => !df.IsDeleted && df.Status == ContentStatus.Approved)
                && d.CreatedById != userId  // ! tài liệu user tự tạo
                && !d.Reviews.Any(r => r.CreatedById == userId))  // Chưa review
            .ToListAsync(ct);

        logger.LogInformation("Found {Count} documents user hasn't reviewed", documents.Count);

        if (!documents.Any())
            return new GetDocumentRecommendationsResponse
            {
                Recommendations = [],
                TotalProcessed = 0,
                ProcessingTimeMs = 0
            };

        // 3. tính
        var docVectors = new List<ConversationVectorData>(); // Reuse ConversationVectorData
        foreach (var doc in documents)
        {
            var docVector = await CalcDocVectorAsync(doc, ct);
            docVectors.Add(new ConversationVectorData(doc.Id, docVector));
        }

        // 4. Call AI service 
        var topK = request.TopK ?? 10;
        var minSimilarity = request.MinSimilarity ?? 0.3f;

        logger.LogInformation("Calling AI: {Count} docs, topK={TopK}", docVectors.Count, topK);

        var recResponse = await recommendationService.GetRecommendationsAsync(
            userVector, docVectors, topK, minSimilarity, ct);

        // 5. Map response
        var documentDict = documents.ToDictionary(d => d.Id);
        var recs = recResponse.Recommendations
            .Select(rec =>
            {
                if (!documentDict.TryGetValue(rec.ConversationId, out var doc))
                    return null;

                var approvedFiles = doc.DocumentFiles
                    .Where(df => !df.IsDeleted && df.Status == ContentStatus.Approved)
                    .ToList();

                return new DocumentDto
                {
                    Id = doc.Id,
                    DocumentName = doc.DocumentName,
                    Description = doc.Description,
                    Visibility = doc.Visibility,
                    Subject = doc.Subject != null ? new SubjectDto
                    {
                        Id = doc.Subject.Id,
                        SubjectName = doc.Subject.SubjectName,
                        SubjectCode = doc.Subject.SubjectCode
                    } : null,
                    Type = doc.Type != null ? new TypeDto
                    {
                        Id = doc.Type.Id,
                        TypeName = doc.Type.TypeName
                    } : default!,
                    Tags = doc.DocumentTags
                        .Where(dt => dt.Tag != null)
                        .Select(dt => new TagDto { Id = dt.Tag!.Id, TagName = dt.Tag.TagName })
                        .ToList(),
                    Authors = doc.DocumentAuthors
                        .Where(da => da.Author != null)
                        .Select(da => new AuthorDetailDto { Id = da.Author!.Id, FullName = da.Author.FullName })
                        .ToList(),
                    ThumbnailFileId = doc.CoverFileId ?? approvedFiles.MinBy(f => f.Order)?.CoverFileId,
                    FileCount = approvedFiles.Count,
                    CommentCount = approvedFiles.Sum(f => f.Comments?.Count(c => !c.IsDeleted) ?? 0),
                    UsefulCount = doc.Reviews.Count(rv => rv.DocumentReviewType == DocumentReviewType.Useful),
                    NotUsefulCount = doc.Reviews.Count(rv => rv.DocumentReviewType == DocumentReviewType.NotUseful),
                    TotalViewCount = approvedFiles.Sum(f => f.ViewCount),
                    CreatedById = doc.CreatedById,
                    CreatedAt = doc.CreatedAt
                };
            })
            .Where(r => r != null)
            .Cast<DocumentDto>()
            .ToList();

        return new GetDocumentRecommendationsResponse
        {
            Recommendations = recs,
            TotalProcessed = recResponse.TotalProcessed,
            ProcessingTimeMs = recResponse.ProcessingTimeMs
        };
    }

    private async Task<float[]> GetOrCalcUserVectorAsync(Guid userId, CancellationToken ct)
    {
        var existing = await profileVectorStore.Query()
            .Where(v => v.UserId == userId && v.IsActive)
            .OrderByDescending(v => v.CalculatedAt)
            .FirstOrDefaultAsync(ct);

        if (existing != null && !string.IsNullOrEmpty(existing.EmbeddingJson))
        {
            try
            {
                var vec = System.Text.Json.JsonSerializer.Deserialize<float[]>(existing.EmbeddingJson);
                if (vec != null && vec.Length == embeddingService.Dim) return vec;
            }
            catch { /* recalculate */ }
        }

        var data = await userDataRepository.GetUserBehaviorTextDataAsync(userId, ct)
            ?? throw new NotFoundException($"User {userId} not found");

        return await embeddingService.UserVectorAsync(new UserVectorRequest
        {
            Subjects = data.SubjectScores.Select(x => x.Name).ToList(),
            SubjectWeights = data.SubjectScores.Select(x => (float)x.Score).ToList(),
            Tags = data.TagScores.Select(x => x.Name).ToList(),
            TagWeights = data.TagScores.Select(x => (float)x.Score).ToList()
        }, ct);
    }

    private async Task<float[]> CalcDocVectorAsync(Domain.Entities.Document doc, CancellationToken ct)
    {
        var req = new ConvVectorRequest
        {
            Name = doc.DocumentName,
            Subject = doc.Subject?.SubjectName,
            Tags = doc.DocumentTags.Where(t => t.Tag != null).Select(t => t.Tag!.TagName).ToList()
        };
        return await embeddingService.ConvVectorAsync(req, ct);
    }
}
