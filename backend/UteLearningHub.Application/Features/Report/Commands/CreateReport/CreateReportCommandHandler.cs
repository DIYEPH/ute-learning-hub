using MediatR;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Report;

namespace UteLearningHub.Application.Features.Report.Commands.CreateReport;

public class CreateReportCommandHandler(IReportService reportService) : IRequestHandler<CreateReportCommand, ReportDto>
{
    public async Task<ReportDto> Handle(CreateReportCommand request, CancellationToken ct)
    {
        return await reportService.CreateAsync(request, ct);
    }
}
