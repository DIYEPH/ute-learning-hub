using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UteLearningHub.Application.Common.Dtos.Statistics;
using UteLearningHub.Application.Services.Statistics;

namespace UteLearningHub.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")]
public class StatisticsController : ControllerBase
{
    private readonly IStatisticsService _statisticsService;

    public StatisticsController(IStatisticsService statisticsService)
    {
        _statisticsService = statisticsService;
    }

    /// <summary>
    /// Get overview dashboard statistics
    /// </summary>
    [HttpGet("overview")]
    public async Task<ActionResult<OverviewStatsDto>> GetOverview([FromQuery] int days = 30, CancellationToken ct = default)
    {
        var result = await _statisticsService.GetOverviewStatsAsync(days, ct);
        return Ok(result);
    }

    /// <summary>
    /// Get document statistics
    /// </summary>
    [HttpGet("documents")]
    public async Task<ActionResult<DocumentStatsDto>> GetDocuments([FromQuery] int days = 30, CancellationToken ct = default)
    {
        var result = await _statisticsService.GetDocumentStatsAsync(days, ct);
        return Ok(result);
    }

    /// <summary>
    /// Get user statistics
    /// </summary>
    [HttpGet("users")]
    public async Task<ActionResult<UserStatsDto>> GetUsers([FromQuery] int days = 30, CancellationToken ct = default)
    {
        var result = await _statisticsService.GetUserStatsAsync(days, ct);
        return Ok(result);
    }

    /// <summary>
    /// Get moderation/reports statistics
    /// </summary>
    [HttpGet("moderation")]
    public async Task<ActionResult<ModerationStatsDto>> GetModeration([FromQuery] int days = 30, CancellationToken ct = default)
    {
        var result = await _statisticsService.GetModerationStatsAsync(days, ct);
        return Ok(result);
    }

    /// <summary>
    /// Get conversation statistics
    /// </summary>
    [HttpGet("conversations")]
    public async Task<ActionResult<ConversationStatsDto>> GetConversations([FromQuery] int days = 30, CancellationToken ct = default)
    {
        var result = await _statisticsService.GetConversationStatsAsync(days, ct);
        return Ok(result);
    }
}

