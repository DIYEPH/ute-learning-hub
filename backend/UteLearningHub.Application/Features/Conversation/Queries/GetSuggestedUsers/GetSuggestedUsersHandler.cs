using MediatR;
using Microsoft.EntityFrameworkCore;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Application.Services.Recommendation;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;

namespace UteLearningHub.Application.Features.Conversation.Queries.GetSuggestedUsers;

public class GetSuggestedUsersHandler : IRequestHandler<GetSuggestedUsersQuery, GetSuggestedUsersResponse>
{
    private readonly IConversationRepository _convRepo;
    private readonly ICurrentUserService _currentUser;
    private readonly IIdentityService _identity;
    private readonly IEmbeddingService _embed;
    private readonly IProfileVectorStore _profileStore;
    private readonly IConversationVectorStore _convStore;
    private readonly IRecommendationService _recommend;

    public GetSuggestedUsersHandler(
        IConversationRepository convRepo,
        ICurrentUserService currentUser,
        IIdentityService identity,
        IEmbeddingService embed,
        IProfileVectorStore profileStore,
        IConversationVectorStore convStore,
        IRecommendationService recommend)
    {
        _convRepo = convRepo;
        _currentUser = currentUser;
        _identity = identity;
        _embed = embed;
        _profileStore = profileStore;
        _convStore = convStore;
        _recommend = recommend;
    }

    public async Task<GetSuggestedUsersResponse> Handle(GetSuggestedUsersQuery req, CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated)
            throw new UnauthorizedException("Must be authenticated");

        // Lấy conversation
        var conv = await _convRepo.GetByIdWithDetailsAsync(req.ConversationId, true, ct);
        if (conv == null || conv.IsDeleted)
            throw new NotFoundException("Conversation not found");

        // Lấy vector của conversation
        var convVectorEntity = await _convStore.Query()
            .Where(v => v.ConversationId == req.ConversationId && v.IsActive)
            .OrderByDescending(v => v.CalculatedAt)
            .FirstOrDefaultAsync(ct);

        float[] convVector;
        if (convVectorEntity != null && !string.IsNullOrEmpty(convVectorEntity.EmbeddingJson))
        {
            convVector = System.Text.Json.JsonSerializer.Deserialize<float[]>(convVectorEntity.EmbeddingJson) 
                ?? new float[_embed.Dim];
        }
        else
        {
            // Tính vector nếu chưa có
            convVector = await _embed.ConvVectorAsync(new ConvVectorRequest
            {
                Name = conv.ConversationName,
                Subject = conv.Subject?.SubjectName,
                Tags = conv.ConversationTags.Where(t => t.Tag != null).Select(t => t.Tag!.TagName).ToList()
            }, ct);
        }

        // Lấy danh sách member hiện tại (để loại trừ)
        var memberIds = conv.Members.Where(m => !m.IsDeleted).Select(m => m.UserId).ToHashSet();

        // Lấy tất cả user vectors (trừ members)
        var userVectorEntities = await _profileStore.Query()
            .Where(v => v.IsActive && !memberIds.Contains(v.UserId))
            .ToListAsync(ct);

        // Parse vectors
        var userVectors = new List<UserVectorData>();
        foreach (var uv in userVectorEntities)
        {
            if (string.IsNullOrEmpty(uv.EmbeddingJson)) continue;
            try
            {
                var vec = System.Text.Json.JsonSerializer.Deserialize<float[]>(uv.EmbeddingJson);
                if (vec != null && vec.Length == convVector.Length)
                    userVectors.Add(new UserVectorData(uv.UserId, vec));
            }
            catch { continue; }
        }

        // Gọi AI service để tìm similar users
        var aiResult = await _recommend.GetSimilarUsersAsync(
            convVector, userVectors, req.TopK, req.MinScore, ct);

        // Lấy thông tin user
        var suggestions = new List<SuggestedUserDto>();
        foreach (var item in aiResult.Users)
        {
            var user = await _identity.FindByIdAsync(item.UserId);
            if (user == null) continue;

            suggestions.Add(new SuggestedUserDto
            {
                UserId = item.UserId,
                FullName = user.FullName,
                AvatarUrl = user.AvatarUrl,
                Similarity = item.Similarity,
                Rank = item.Rank
            });
        }

        return new GetSuggestedUsersResponse
        {
            Users = suggestions,
            TotalProcessed = aiResult.TotalProcessed
        };
    }
}
