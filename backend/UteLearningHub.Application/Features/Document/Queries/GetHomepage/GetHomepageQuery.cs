using MediatR;
using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Features.Document.Queries.GetHomepage;

public record GetHomepageQuery : IRequest<HomepageDto>;
