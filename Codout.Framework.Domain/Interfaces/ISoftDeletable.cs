using System;

namespace Codout.Framework.Domain.Interfaces;

public interface ISoftDeletable
{
    DateTime? DeletedAt { get; set; }
}