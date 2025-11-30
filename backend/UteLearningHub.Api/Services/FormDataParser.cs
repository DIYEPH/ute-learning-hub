using Microsoft.AspNetCore.Http;
using UteLearningHub.Application.Features.Document.Commands.CreateDocument;

namespace UteLearningHub.Api.Services;

public class FormDataParser : IFormDataParser
{
    public CreateDocumentCommand ParseCreateDocumentCommand(IFormCollection form, CreateDocumentCommand originalCommand)
    {
        var tagIds = ParseTagIds(form);
        var tagNames = ParseTagNames(form);

        return originalCommand with
        {
            TagIds = tagIds ?? originalCommand.TagIds,
            TagNames = tagNames ?? originalCommand.TagNames
        };
    }

    private IList<Guid>? ParseTagIds(IFormCollection form)
    {
        if (!form.ContainsKey("TagIds"))
            return null;

        var tagIdsValues = form["TagIds"];
        var validTagIds = new List<Guid>();

        foreach (var value in tagIdsValues)
        {
            if (string.IsNullOrWhiteSpace(value))
                continue;

            if (Guid.TryParse(value, out var guid) && guid != Guid.Empty)
            {
                validTagIds.Add(guid);
            }
        }

        return validTagIds.Count > 0 ? validTagIds : null;
    }

    private IList<string>? ParseTagNames(IFormCollection form)
    {
        if (!form.ContainsKey("TagNames"))
            return null;

        var tagNamesValues = form["TagNames"];
        var validTagNames = new List<string>();

        foreach (var value in tagNamesValues)
        {
            if (string.IsNullOrWhiteSpace(value))
                continue;

            validTagNames.Add(value.Trim());
        }

        return validTagNames.Count > 0 ? validTagNames : null;
    }
}
