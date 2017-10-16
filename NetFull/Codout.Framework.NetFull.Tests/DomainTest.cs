using Codout.Framework.Domain.DAL;
using System;

namespace Codout.Framework.NetFull.Tests
{
    public abstract class Entity : EntityWithTypedId<Guid?>
    {
    }

    public class Customer : Entity
    {
        public string Nome { get; set; }
    }
}
