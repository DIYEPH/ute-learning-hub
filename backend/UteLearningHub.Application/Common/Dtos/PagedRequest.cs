namespace UteLearningHub.Application.Common.Dtos;

public record PagedRequest
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    
    public int Skip => (Page - 1) * PageSize;
    public int Take => PageSize;
}
