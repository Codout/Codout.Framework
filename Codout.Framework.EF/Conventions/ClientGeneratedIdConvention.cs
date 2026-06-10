using Codout.Framework.Data.Entity;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;

namespace Codout.Framework.EF.Conventions;

/// <summary>
/// Para toda entidade que implementa <see cref="IClientGeneratedId"/>, força a PK a
/// <see cref="ValueGenerated.Never"/> — declarando ao EF que a identidade é atribuída
/// pela aplicação. Sem isso, o EF infere <c>EntityState.Modified</c> para uma instância
/// nova já com Id dentro de um grafo tracked (heurística "PK preenchida ⇒ existe"),
/// causando UPDATE de linha inexistente (<c>DbUpdateConcurrencyException</c>).
/// <para>
/// Roda como <see cref="IModelFinalizingConvention"/> (por último, imune ao reset
/// descrito em dotnet/efcore#18507 que afeta convenções interativas) e só toca a PK do
/// tipo marcado — owned types (Money/Address) e suas shadow keys permanecem geridos
/// pelo EF.
/// </para>
/// </summary>
public sealed class ClientGeneratedIdConvention : IModelFinalizingConvention
{
    public void ProcessModelFinalizing(
        IConventionModelBuilder modelBuilder,
        IConventionContext<IConventionModelBuilder> context)
    {
        foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
        {
            if (!typeof(IClientGeneratedId).IsAssignableFrom(entityType.ClrType))
                continue;

            var primaryKey = entityType.FindPrimaryKey();
            if (primaryKey is null)
                continue;

            foreach (var property in primaryKey.Properties)
            {
                // fromDataAnnotation:true → ConfigurationSource forte: blinda contra
                // qualquer convenção de menor prioridade. O retorno (nullable) é
                // ignorado de propósito — o efeito já foi aplicado; encadear sem
                // null-check seria o único erro.
                property.Builder.ValueGenerated(ValueGenerated.Never, fromDataAnnotation: true);
            }
        }
    }
}
