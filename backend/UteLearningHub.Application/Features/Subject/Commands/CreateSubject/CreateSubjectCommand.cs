using MediatR;
using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Features.Subject.Commands.CreateSubject;

public record CreateSubjectCommand : CreateSubjectRequest, IRequest<SubjectDetailDto>;
