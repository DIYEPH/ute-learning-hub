using System.Net;
using UteLearningHub.Domain.Exceptions.Base;

namespace UteLearningHub.Domain.Exceptions;

public class ForbidenException: BaseException
{
    private const int _statusCode = (int) HttpStatusCode.Forbidden;
    private const string _title = "Forbidden";
    private const string _type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.3";
    public ForbidenException() {
        StatusCode = _statusCode;
        Title = _title;
        Type = _type;
    }
    public ForbidenException(string message) : base(message) {
        StatusCode = _statusCode;
        Title = _title;
        Type = _type;
    }
}
