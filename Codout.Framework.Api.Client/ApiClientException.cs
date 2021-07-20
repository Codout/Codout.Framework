using System;

namespace Codout.Framework.Api.Client
{
    public class ApiClientException : Exception
    {
        public ApiException ApiException { get; }

        public ApiClientException(ApiException apiException)
            : base(apiException.Message)
        {
            ApiException = apiException;
        }
    }
}
