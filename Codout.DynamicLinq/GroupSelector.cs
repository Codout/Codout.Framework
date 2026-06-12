using System;
using System.Collections.Generic;

namespace Codout.DynamicLinq;

public class GroupSelector<TElement>
{
    public Func<TElement, object> Selector { get; set; } = null!;
    public string Field { get; set; } = null!;
    public IEnumerable<Aggregator>? Aggregates { get; set; }
}
