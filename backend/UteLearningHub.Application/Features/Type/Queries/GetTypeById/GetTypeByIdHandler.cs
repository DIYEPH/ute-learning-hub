using MediatR;
using Microsoft.EntityFrameworkCore;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;
using UteLearningHub.Application.Services.Identity;

namespace UteLearningHub.Application.Features.Type.Queries.GetTypeById;

public class GetTypeByIdHandler : IRequestHandler<GetTypeByIdQuery, TypeDetailDto>
{   
    private readonly ITypeRepository _typeRepository;
    private readonly ICurrentUserService _currentUserService;
    public GetTypeByIdHandler(ITypeRepository typeRepository, ICurrentUserService currentUserService)
    {
        _typeRepository = typeRepository;
        _currentUserService = currentUserService;
    }

    public async Task<TypeDetailDto> Handle(GetTypeByIdQuery request, CancellationToken cancellationToken)
    {
        // Use GetByIdAsync from repository base (no Include needed)
        var type = await _typeRepository.GetByIdAsync(request.Id, disableTracking: true, cancellationToken);

        if (type == null)
            throw new NotFoundException($"Type with id {request.Id} not found");

        // Query document count separately to avoid loading all documents
        var documentCount = await _typeRepository.GetQueryableSet()
            .Where(t => t.Id == request.Id)
            .Select(t => t.Documents.Count)
            .FirstOrDefaultAsync(cancellationToken);

        return new TypeDetailDto
        {
            Id = type.Id,
            TypeName = type.TypeName,
            DocumentCount = documentCount
        };
    }
}