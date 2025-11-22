using MediatR;
using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Features.Report.Queries.GetReports;

public record GetReportsQuery : GetReportsRequest, IRequest<PagedResponse<ReportDto>>;
