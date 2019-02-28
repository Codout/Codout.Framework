using System;

namespace Codout.Framework.Api.Client
{
    public class ApiException : Exception
    {
        public ApiException(string message)
        : base(message)
        {
        }
    }
}
