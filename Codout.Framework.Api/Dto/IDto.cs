using System;
using System.Collections.Generic;
using System.Text;

namespace Codout.Framework.Api.Dto
{
    public interface IDto<TId>
    {
        TId Id { get; set; }
    }
}
