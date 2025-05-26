namespace Codout.Framework.Api.Client;

public class ApiErrorMessage
{
    public ApiErrorMessage(int errorCode, string errorMessage)
    {
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
    }

    public int ErrorCode { get; }

    public string ErrorMessage { get; }
}