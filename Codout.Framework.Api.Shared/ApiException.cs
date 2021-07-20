using Newtonsoft.Json;

namespace Codout.Framework.Api
{
    public class ApiException
    {
        public ApiException(int statusCode, string message, params ApiErrorMessage[] errors)
        {
            StatusCode = statusCode;
            Message = message;
            Errors = errors;
        }

        public int StatusCode { get; set; }
        
        public string Message { get; set; }

        public ApiErrorMessage[] Errors { get; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
