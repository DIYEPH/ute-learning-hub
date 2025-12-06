using System.Net;
using UteLearningHub.Domain.Exceptions.Base;

namespace UteLearningHub.Domain.Exceptions;

public class ConcurrencyException : BaseException
{
    private const int _statusCode = (int)HttpStatusCode.Conflict;
    private const string _title = "Conflict";
    private const string _type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.8";

    public ConcurrencyException()
        : base("The resource has been modified by another user. Please reload and try again.")
    {
        StatusCode = _statusCode;
        Title = _title;
        Type = _type;
    }

    public ConcurrencyException(string message) : base(message)
    {
        StatusCode = _statusCode;
        Title = _title;
        Type = _type;
    }
}
