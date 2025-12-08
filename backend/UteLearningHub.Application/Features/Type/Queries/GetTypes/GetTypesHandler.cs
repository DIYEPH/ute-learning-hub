using MediatR;
using Microsoft.EntityFrameworkCore;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Domain.Repositories;

namespace UteLearningHub.Application.Features.Type.Queries.GetTypes;

public class GetTypesHandler : IRequestHandler<GetTypesQuery, PagedResponse<TypeDto>>
{
    private readonly ITypeRepository _typeRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetTypesHandler(ITypeRepository typeRepository, ICurrentUserService currentUserService)
    {
        _typeRepository = typeRepository;
        _currentUserService = currentUserService;
    }

    public async Task<PagedResponse<TypeDto>> Handle(GetTypesQuery request, CancellationToken cancellationToken)
    {
        var query = _typeRepository.GetQueryableSet()
            .AsNoTracking();

        // Filter by IsDeleted status (default: only active items)
        if (request.IsDeleted.HasValue)
            query = query.Where(t => t.IsDeleted == request.IsDeleted.Value);
        else
            query = query.Where(t => !t.IsDeleted);

        // Search by name
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(t => t.TypeName.ToLower().Contains(searchTerm));
        }

        query = query.OrderBy(t => t.TypeName);

        var totalCount = await query.CountAsync(cancellationToken);

        var types = await query
            .Skip(request.Skip)
            .Take(request.Take)
            .Select(t => new TypeDto
            {
                Id = t.Id,
                TypeName = t.TypeName
            })
            .ToListAsync(cancellationToken);

        return new PagedResponse<TypeDto>
        {
            Items = types,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}