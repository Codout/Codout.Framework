using System;
using Codout.Framework.Domain.DAL;

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
