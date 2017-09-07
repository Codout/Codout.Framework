using System;

namespace Codout.Framework.NetStandard.Commom.Exceptions
{
    public class BusinessRuleException : Exception
    {
        public BusinessRuleException(string message): base(message)
        {

        }
    }
}
