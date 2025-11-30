using Microsoft.AspNetCore.Http;
using UteLearningHub.Application.Features.Document.Commands.CreateDocument;

namespace UteLearningHub.Api.Services;

public interface IFormDataParser
{
    CreateDocumentCommand ParseCreateDocumentCommand(IFormCollection form, CreateDocumentCommand originalCommand);
}
