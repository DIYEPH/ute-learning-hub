using MediatR;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;

namespace UteLearningHub.Application.Features.Type.Queries.GetTypeById;

public class GetTypeByIdHandler : IRequestHandler<GetTypeByIdQuery, TypeDetailDto>
{
    private readonly ITypeRepository _typeRepository;

    public GetTypeByIdHandler(ITypeRepository typeRepository)
    {
        _typeRepository = typeRepository;
    }

    public async Task<TypeDetailDto> Handle(GetTypeByIdQuery request, CancellationToken cancellationToken)
    {
        var type = await _typeRepository.GetByIdAsync(request.Id, disableTracking: true, cancellationToken);

        if (type == null || type.IsDeleted)
            throw new NotFoundException($"Type with id {request.Id} not found");

        var documentCount = await _typeRepository.GetDocumentCountAsync(request.Id, cancellationToken);

        return new TypeDetailDto
        {
            Id = type.Id,
            TypeName = type.TypeName,
            DocumentCount = documentCount
        };
    }
}