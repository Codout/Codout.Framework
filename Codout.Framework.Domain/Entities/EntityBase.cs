using System;

namespace Codout.Framework.Domain.Entities;

/// <summary>
///     Entidade base padrão do framework, com identificador <see cref="Guid" />
///     anulável (<c>null</c> enquanto transient). Use quando o Id é gerado pelo
///     store (ex.: <c>ValueGeneratedOnAdd</c> no EF); para Id atribuído pela
///     aplicação na criação, prefira <see cref="ClientGeneratedEntity" />.
/// </summary>
[Serializable]
public abstract class EntityBase : Entity<Guid?>;
