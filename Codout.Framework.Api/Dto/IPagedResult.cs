using System;
using System.Collections.Generic;
using System.Text;

namespace Codout.Framework.Api.Dto
{
    public interface IPagedResult<out TDto>
    {
        int PageIndex { get; }
        int PageSize { get; }
        int TotalCount { get; }
        int TotalPages { get; }
        IEnumerable<TDto> Results { get; }
        bool HasPreviousPage { get; }
        bool HasNextPage { get; }
    }
}
