using System;

namespace Codout.Framework.Domain.Interfaces;

public interface IAudit
{
    DateTime? CreatedAt { get; set; }
    DateTime? UpdatedAt { get; set; }
    string CreatedBy { get; set; }
    string UpdatedBy { get; set; }
}