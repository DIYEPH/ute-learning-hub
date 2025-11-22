using MediatR;
using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Features.Report.Commands.CreateReport;

public record CreateReportCommand : CreateReportRequest, IRequest<ReportDto>;