using MediatR;

namespace UteLearningHub.Application.Features.Report.Commands.ReviewReport;

public record ReviewReportCommand : ReviewReportRequest, IRequest<Unit>;
