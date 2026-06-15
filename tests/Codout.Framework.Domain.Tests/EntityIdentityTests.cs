using Codout.Framework.Data.Entity;
using Codout.Framework.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Codout.Framework.Domain.Tests;

/// <summary>
/// Identidade das entidades: semântica de IsTransient/SetId para diferentes tipos de Id
/// e a invariante de kernel da ClientGeneratedEntity (Id atribuído pela aplicação no ctor).
/// </summary>
public class EntityIdentityTests
{
    private class IntEntity : Entity<int>;

    private class GuidEntity : Entity<Guid>;

    private class NullableGuidEntity : Entity<Guid?>;

    private class StringEntity : Entity<string>;

    private class Document : ClientGeneratedEntity
    {
        public string? Title { get; set; }
    }

    private class ConcreteBase : EntityBase;

    [Fact]
    public void Int_entity_is_transient_when_id_is_zero()
    {
        var entity = new IntEntity();
        entity.IsTransient().Should().BeTrue();

        entity.SetId(1);
        entity.IsTransient().Should().BeFalse();
    }

    [Fact]
    public void Guid_entity_is_transient_when_id_is_empty()
    {
        var entity = new GuidEntity();
        entity.IsTransient().Should().BeTrue("Guid.Empty é o default");

        entity.SetId(Guid.NewGuid());
        entity.IsTransient().Should().BeFalse();
    }

    [Fact]
    public void Nullable_guid_entity_is_transient_when_id_is_null()
    {
        var entity = new NullableGuidEntity();
        entity.IsTransient().Should().BeTrue();

        entity.SetId(Guid.NewGuid());
        entity.IsTransient().Should().BeFalse();
    }

    [Fact]
    public void String_entity_is_transient_when_id_is_null()
    {
        var entity = new StringEntity();
        entity.IsTransient().Should().BeTrue();

        entity.SetId("abc");
        entity.IsTransient().Should().BeFalse();
    }

    [Fact]
    public void SetId_assigns_the_id()
    {
        var entity = new IntEntity();
        entity.SetId(123);
        entity.Id.Should().Be(123);
    }

    [Fact]
    public void EntityBase_uses_nullable_guid_id_and_starts_transient()
    {
        var entity = new ConcreteBase();

        entity.Should().BeAssignableTo<Entity<Guid?>>();
        entity.Id.Should().BeNull();
        entity.IsTransient().Should().BeTrue("EntityBase é store-generated: nasce sem Id");
    }

    // ---- ClientGeneratedEntity: invariante de kernel (identity client-generated) ----

    [Fact]
    public void ClientGeneratedEntity_assigns_id_in_constructor()
    {
        var doc = new Document();

        doc.Id.Should().NotBeNull("a aplicação atribui a PK na criação");
        doc.Id.Should().NotBe(Guid.Empty);
        doc.IsTransient().Should().BeFalse("já nasce com identidade");
    }

    [Fact]
    public void ClientGeneratedEntity_generates_unique_ids_per_instance()
    {
        var a = new Document();
        var b = new Document();

        a.Id.Should().NotBe(b.Id!.Value);
    }

    [Fact]
    public void ClientGeneratedEntity_implements_the_IClientGeneratedId_marker()
    {
        new Document().Should().BeAssignableTo<IClientGeneratedId>(
            "o marker é o opt-in da ClientGeneratedIdConvention no EF");
    }

    [Fact]
    public void ClientGeneratedEntity_allows_rehydration_to_override_the_ctor_id()
    {
        // O EF materializa via ctor sem-args e sobrescreve o Id em seguida (SetId/setter).
        var doc = new Document();
        var persistedId = Guid.NewGuid();

        doc.SetId(persistedId);

        doc.Id.Should().Be(persistedId);
    }

    [Fact]
    public void Two_client_generated_entities_are_not_equal_because_ids_differ()
    {
        var a = new Document { Title = "x" };
        var b = new Document { Title = "x" };

        a.Equals(b).Should().BeFalse("ambas já nascem persist-ready com Ids distintos");
    }

    [Fact]
    public void Client_generated_entities_with_same_id_are_equal()
    {
        var id = Guid.NewGuid();
        var a = new Document { Title = "x" };
        var b = new Document { Title = "y" };
        a.SetId(id);
        b.SetId(id);

        a.Equals(b).Should().BeTrue();
    }
}
