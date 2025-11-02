using System.Net;
using UteLearningHub.Domain.Exceptions.Base;

namespace UteLearningHub.Domain.Exceptions;
public class UnauthorizedException : BaseException
{
    private const int _statusCode = (int) HttpStatusCode.Unauthorized;
    private const string _title = "Unauthorized";
    private const string _type = "https://datatracker.ietf.org/doc/html/rfc7235#section-3.1";
    public UnauthorizedException() {
        StatusCode = _statusCode;
        Title = _title;
        Type = _type;
    }
    public UnauthorizedException(string message) : base(message) {
        StatusCode = _statusCode; 
        Title = _title;
        Type = _type;
    }
}
