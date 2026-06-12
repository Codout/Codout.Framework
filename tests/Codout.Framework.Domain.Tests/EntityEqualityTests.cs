using Codout.Framework.Domain.Base;
using Codout.Framework.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Codout.Framework.Domain.Tests;

/// <summary>
/// Igualdade de Entity&lt;TId&gt;: identidade por Id quando persistido, assinatura de
/// domínio ([DomainSignature]) quando transient, e nunca igualdade entre tipos distintos.
/// </summary>
public class EntityEqualityTests
{
    private class Person : Entity<int>
    {
        [DomainSignature]
        public string? Name { get; set; }

        // Propriedade fora da assinatura: não deve influenciar a igualdade.
        public int Age { get; set; }
    }

    private class Company : Entity<int>
    {
        [DomainSignature]
        public string? Name { get; set; }
    }

    // Entidade sem nenhuma propriedade [DomainSignature].
    private class Anonymous : Entity<int>
    {
        public string? Tag { get; set; }
    }

    [Fact]
    public void Transient_entities_with_same_domain_signature_are_equal()
    {
        var a = new Person { Name = "Ana", Age = 30 };
        var b = new Person { Name = "Ana", Age = 99 };

        a.Equals(b).Should().BeTrue("ambas transient com mesma [DomainSignature]; Age não faz parte da assinatura");
    }

    [Fact]
    public void Transient_entities_with_different_domain_signature_are_not_equal()
    {
        var a = new Person { Name = "Ana" };
        var b = new Person { Name = "Bia" };

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void Persisted_entities_with_same_id_are_equal_even_with_different_signature()
    {
        var a = new Person { Name = "Ana" };
        var b = new Person { Name = "Bia" };
        a.SetId(7);
        b.SetId(7);

        a.Equals(b).Should().BeTrue("identidade persistida (Id) prevalece sobre a assinatura");
    }

    [Fact]
    public void Persisted_entities_with_different_ids_are_not_equal_even_with_same_signature()
    {
        var a = new Person { Name = "Ana" };
        var b = new Person { Name = "Ana" };
        a.SetId(1);
        b.SetId(2);

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void Persisted_entity_is_not_equal_to_transient_with_same_signature()
    {
        var persisted = new Person { Name = "Ana" };
        persisted.SetId(1);
        var transient = new Person { Name = "Ana" };

        persisted.Equals(transient).Should().BeFalse("um persistido e um transient nunca são o mesmo objeto de domínio");
        transient.Equals(persisted).Should().BeFalse();
    }

    [Fact]
    public void Entities_of_different_types_with_same_id_are_not_equal()
    {
        var person = new Person { Name = "Acme" };
        var company = new Company { Name = "Acme" };
        person.SetId(5);
        company.SetId(5);

        person.Equals(company).Should().BeFalse("tipos CLR diferentes nunca são iguais");
    }

    [Fact]
    public void Same_reference_is_always_equal()
    {
        var a = new Person { Name = "Ana" };
        a.Equals(a).Should().BeTrue();
    }

    [Fact]
    public void Entity_is_never_equal_to_null_or_other_kind_of_object()
    {
        var a = new Person { Name = "Ana" };

        a.Equals(null).Should().BeFalse();
        a.Equals("Ana").Should().BeFalse();
    }

    [Fact]
    public void Transient_entities_without_signature_properties_fall_back_to_reference_equality()
    {
        var a = new Anonymous { Tag = "x" };
        var b = new Anonymous { Tag = "x" };

        a.Equals(b).Should().BeFalse("sem [DomainSignature] a comparação transient cai em igualdade de referência");
        a.Equals(a).Should().BeTrue();
    }

    [Fact]
    public void Persisted_entities_with_same_id_have_same_hashcode()
    {
        var a = new Person { Name = "Ana" };
        var b = new Person { Name = "Bia" };
        a.SetId(7);
        b.SetId(7);

        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void Transient_entities_with_same_signature_have_same_hashcode()
    {
        var a = new Person { Name = "Ana", Age = 1 };
        var b = new Person { Name = "Ana", Age = 2 };

        a.GetHashCode().Should().Be(b.GetHashCode(), "hash transient deriva da assinatura de domínio");
    }

    [Fact]
    public void Hashcode_is_cached_and_stable_across_persistence()
    {
        // Design (padrão S#arp Architecture): o hash é congelado no primeiro uso para que
        // a entidade não "se perca" dentro de um HashSet/Dictionary quando ganhar Id.
        var person = new Person { Name = "Ana" };
        var transientHash = person.GetHashCode();

        person.SetId(42);

        person.GetHashCode().Should().Be(transientHash, "o hash é cacheado no primeiro GetHashCode()");

        var freshWithSameId = new Person { Name = "Ana" };
        freshWithSameId.SetId(42);
        freshWithSameId.GetHashCode().Should().NotBe(transientHash,
            "uma instância nova com o mesmo Id calcula o hash por Id, diferente do hash transient congelado");
    }

    [Fact]
    public void Entity_survives_hashset_membership_after_gaining_id()
    {
        var person = new Person { Name = "Ana" };
        var set = new HashSet<object> { person };

        person.SetId(99);

        set.Contains(person).Should().BeTrue("o cache de hash garante que a entidade continua localizável");
    }

    [Fact]
    public void GetSignatureProperties_returns_only_domain_signature_properties()
    {
        var person = new Person { Name = "Ana", Age = 30 };

        var properties = person.GetSignatureProperties().Select(p => p.Name);

        properties.Should().BeEquivalentTo(["Name"]);
    }
}
