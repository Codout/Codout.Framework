namespace Codout.Framework.Data.Entity;

/// <summary>
/// Marca uma entidade cuja identidade (PK) é atribuída pela APLICAÇÃO no momento
/// da criação (client-generated, ex.: <c>SetId(Guid.NewGuid())</c> no construtor),
/// e NÃO pelo store.
/// <para>
/// O mapeamento EF correspondente deve declarar <c>ValueGeneratedNever()</c> na PK.
/// Caso contrário, o EF aplica a heurística "PK preenchida ⇒ entidade já existe" e
/// infere <c>EntityState.Modified</c> para uma instância NOVA já com Id dentro de um
/// grafo tracked — gerando UPDATE de linha inexistente
/// (<c>DbUpdateConcurrencyException</c>) em vez de INSERT.
/// </para>
/// <para>
/// A <c>ClientGeneratedIdConvention</c> (Codout.Framework.EF) aplica
/// <c>ValueGeneratedNever()</c> automaticamente a toda entidade que implementa esta
/// interface. Type-agnostic: a convenção só age sobre a PK, qualquer que seja o TId.
/// </para>
/// </summary>
public interface IClientGeneratedId
{
}
