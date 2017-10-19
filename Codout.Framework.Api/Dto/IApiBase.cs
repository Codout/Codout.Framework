using System;
using System.Collections.Generic;
using System.Text;

namespace Codout.Framework.Api.Dto
{
    public interface IApiBase<TDto, in TId>
    {
        IPagedResult<TDto> Get(int page, int size);

        TDto Get(TId id);

        TDto Post(TDto value);

        void Put(TId id, TDto value);

        void Delete(TId id);

        void Sync(IEnumerable<TDto> value);
    }
}
