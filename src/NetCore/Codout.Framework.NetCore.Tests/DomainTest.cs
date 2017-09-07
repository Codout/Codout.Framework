using Codout.Framework.NetStandard.Domain.Entity;
using System;

namespace Codout.Framework.NetCore.Tests
{
    public abstract class Entity : EntityWithTypedId<Guid?>
    {
    }

    public class Customer : Entity
    {
        public string Nome { get; set; }
    }

    
}
