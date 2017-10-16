using System;
using System.Collections.Generic;

namespace Codout.Framework.Common.Exceptions
{
    public class DataContractException : Exception
    {
        public IList<string> Errors { get; private set; }

        public override string Message => string.Join(Environment.NewLine, Errors);

        public DataContractException(IList<string> errors)
        {
            Errors = errors;
        }
    }
}

