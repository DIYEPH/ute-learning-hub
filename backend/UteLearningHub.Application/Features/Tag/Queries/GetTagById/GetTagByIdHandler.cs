using MediatR;
using Microsoft.EntityFrameworkCore;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;
using UteLearningHub.Application.Services.Identity;

namespace UteLearningHub.Application.Features.Tag.Queries.GetTagById;

public class GetTagByIdHandler : IRequestHandler<GetTagByIdQuery, TagDetailDto>
{
    private readonly ITagRepository _tagRepository;
    private readonly ICurrentUserService _currentUserService;
    public GetTagByIdHandler(ITagRepository tagRepository, ICurrentUserService currentUserService)
    {
        _tagRepository = tagRepository;
        _currentUserService = currentUserService;
    }

    public async Task<TagDetailDto> Handle(GetTagByIdQuery request, CancellationToken cancellationToken)
    {
        var tag = await _tagRepository.GetByIdAsync(request.Id, disableTracking: true, cancellationToken);

        if (tag == null)
            throw new NotFoundException($"Tag with id {request.Id} not found");

        var isAdmin = _currentUserService.IsAuthenticated && _currentUserService.IsInRole("Admin");
        if (!isAdmin && tag.ReviewStatus != ReviewStatus.Approved)
            throw new NotFoundException($"Tag with id {request.Id} not found");

        // Query document count separately to avoid loading all documents
        var documentCount = await _tagRepository.GetQueryableSet()
            .Where(t => t.Id == request.Id)
            .Select(t => t.DocumentTags.Count)
            .FirstOrDefaultAsync(cancellationToken);

        return new TagDetailDto
        {
            Id = tag.Id,
            TagName = tag.TagName,
            DocumentCount = documentCount
        };
    }
}

