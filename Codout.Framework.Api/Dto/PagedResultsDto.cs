using System;
using System.Collections.Generic;
using System.Text;

namespace Codout.Framework.Api.Dto
{
    public class PagedResultsDto<TDto> : IPagedResult<TDto>
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public IEnumerable<TDto> Results { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }
    }
}
