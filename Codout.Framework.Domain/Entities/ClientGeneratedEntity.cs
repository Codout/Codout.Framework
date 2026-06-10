using System;
using Codout.Framework.Data.Entity;

namespace Codout.Framework.Domain.Entities;

/// <summary>
/// Base para entidades/agregados com identidade <see cref="Guid"/> client-generated:
/// a aplicação atribui a PK no momento da criação. Centraliza o
/// <c>SetId(Guid.NewGuid())</c> (guardado por <see cref="Entity{TId}.IsTransient"/>
/// para preservar a re-hidratação do EF, que usa o ctor sem-args e materializa o Id
/// logo depois) e marca o opt-in da convenção via <see cref="IClientGeneratedId"/>.
/// <para>
/// Toda entidade que hoje faz <c>SetId(Guid.NewGuid())</c> espalhado em ctor/factory
/// deve herdar daqui e remover o SetId manual. Entidades store-generated (que dependem
/// de <c>ValueGeneratedOnAdd</c>) NÃO herdam — continuam em <see cref="EntityBase"/>.
/// </para>
/// </summary>
[Serializable]
public abstract class ClientGeneratedEntity : Entity<Guid?>, IClientGeneratedId
{
    protected ClientGeneratedEntity()
    {
        // Só atribui na criação real. Na materialização do EF a instância ainda é
        // transient aqui (Id == null); o EF sobrescreve o Id ao popular as
        // propriedades do reader logo em seguida — comportamento idêntico ao padrão
        // atual em que o ctor já setava um Guid. O guard mantém idempotência.
        if (IsTransient())
            SetId(Guid.NewGuid());
    }
}
