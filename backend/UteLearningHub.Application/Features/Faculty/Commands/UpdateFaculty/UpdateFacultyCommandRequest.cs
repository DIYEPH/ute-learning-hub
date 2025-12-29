using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UteLearningHub.Application.Features.Faculty.Commands.UpdateFaculty;

public record UpdateFacultyCommandRequest
{
    public string FacultyName { get; init; } = default!;
    public string FacultyCode { get; init; } = default!;
    public string? Logo { get; init; }
}
