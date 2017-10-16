using System;

namespace Codout.Framework.Common.Exceptions
{
    public class BusinessRuleException : Exception
    {
        public BusinessRuleException(string message): base(message)
        {

        }
    }
}
