namespace UteLearningHub.Domain.Exceptions.Base;

public interface IAppException
{
    int StatusCode { get; }
    string? Title { get; }
    string? Type { get; }

}
public class BaseException : Exception, IAppException
{
    public int StatusCode {  get; set; }
    public string? Title { get; set; }
    public string? Type { get; set; }
    public BaseException() { }
    public BaseException(string? message) : base(message) { }
    public BaseException(string? message, int statusCode, string? title) : base(message)
    {
        StatusCode = statusCode;
        Title = title;
    }
}
