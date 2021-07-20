using System;
using Codout.Framework.Domain.Interfaces;

namespace Codout.Framework.Domain
{
    [Serializable]
    public abstract class AuditEntity<TId> : Entity<TId>, IAudit
    {
        public virtual DateTime? CreatedAt { get; set; }
        public virtual DateTime? UpdatedAt { get; set; }
        public virtual string CreatedBy { get; set; }
        public virtual string UpdatedBy { get; set; }
    }
}
