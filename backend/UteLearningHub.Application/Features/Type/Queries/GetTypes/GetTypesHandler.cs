using MediatR;
using Microsoft.EntityFrameworkCore;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Repositories;

namespace UteLearningHub.Application.Features.Type.Queries.GetTypes;

public class GetTypesHandler : IRequestHandler<GetTypesQuery, PagedResponse<TypeDto>>
{
    private readonly ITypeRepository _typeRepository;

    public GetTypesHandler(ITypeRepository typeRepository)
    {
        _typeRepository = typeRepository;
    }

    public async Task<PagedResponse<TypeDto>> Handle(GetTypesQuery request, CancellationToken cancellationToken)
    {
        var query = _typeRepository.GetQueryableSet()
            .AsNoTracking();

        // Search by name
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(t => t.TypeName.ToLower().Contains(searchTerm));
        }

        query = query.Where(t => t.ReviewStatus == ReviewStatus.Approved);

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
