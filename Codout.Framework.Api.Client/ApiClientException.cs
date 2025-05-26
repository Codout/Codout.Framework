using System;

namespace Codout.Framework.Api.Client;

public class ApiClientException(ApiException apiException) : Exception(apiException.Message)
{
    public ApiException ApiException { get; } = apiException;
}