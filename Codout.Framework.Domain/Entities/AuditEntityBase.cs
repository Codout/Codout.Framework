using System;
using Codout.Framework.Domain.Interfaces;

namespace Codout.Framework.Domain.Entities;

[Serializable]
public abstract class AuditEntityBase : EntityBase, IAudit
{
    public virtual DateTime? CreatedAt { get; set; }

    public virtual DateTime? UpdatedAt { get; set; }

    public virtual string CreatedBy { get; set; }

    public virtual string UpdatedBy { get; set; }
}