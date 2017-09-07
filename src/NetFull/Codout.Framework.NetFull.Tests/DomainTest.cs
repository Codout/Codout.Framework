using Codout.Framework.NetStandard.Domain.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
