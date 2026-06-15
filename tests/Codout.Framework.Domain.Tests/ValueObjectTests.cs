using Codout.Framework.Domain.Base;
using FluentAssertions;
using Xunit;

namespace Codout.Framework.Domain.Tests;

/// <summary>
/// Value objects: igualdade estrutural por TODAS as propriedades públicas, operadores
/// ==/!= null-safe e a proibição de [DomainSignature] em value objects.
/// </summary>
public class ValueObjectTests
{
    private class Money : ValueObject
    {
        public Money(string currency, decimal amount)
        {
            Currency = currency;
            Amount = amount;
        }

        public string Currency { get; }
        public decimal Amount { get; }
    }

    private class Weight : ValueObject
    {
        public Weight(decimal amount)
        {
            Amount = amount;
        }

        public decimal Amount { get; }
    }

    private class BadValueObject : ValueObject
    {
        [DomainSignature]
        public string? Code { get; set; }
    }

    [Fact]
    public void Value_objects_with_same_property_values_are_equal()
    {
        var a = new Money("BRL", 10.50m);
        var b = new Money("BRL", 10.50m);

        a.Equals(b).Should().BeTrue();
        (a == b).Should().BeTrue();
        (a != b).Should().BeFalse();
    }

    [Fact]
    public void Value_objects_with_different_property_values_are_not_equal()
    {
        var a = new Money("BRL", 10.50m);
        var b = new Money("USD", 10.50m);
        var c = new Money("BRL", 9.99m);

        a.Equals(b).Should().BeFalse();
        a.Equals(c).Should().BeFalse();
        (a != b).Should().BeTrue();
    }

    [Fact]
    public void Equal_value_objects_have_same_hashcode()
    {
        var a = new Money("BRL", 10.50m);
        var b = new Money("BRL", 10.50m);

        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void Different_value_objects_have_different_hashcodes()
    {
        var a = new Money("BRL", 10.50m);
        var b = new Money("USD", 99.99m);

        a.GetHashCode().Should().NotBe(b.GetHashCode());
    }

    [Fact]
    public void Value_objects_of_different_types_are_not_equal_even_with_matching_values()
    {
        var money = new Money("1", 1m);
        var weight = new Weight(1m);

        money.Equals(weight).Should().BeFalse();
    }

    [Fact]
    public void Equality_operator_handles_nulls()
    {
        Money? nullA = null;
        Money? nullB = null;
        var value = new Money("BRL", 1m);

        (nullA == nullB).Should().BeTrue("null == null é consistente com C#");
        (nullA == value).Should().BeFalse();
        (value == nullA).Should().BeFalse();
        (value != nullA).Should().BeTrue();
        value.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void Value_object_works_in_linq_set_operations()
    {
        var list = new[]
        {
            new Money("BRL", 1m),
            new Money("BRL", 1m),
            new Money("USD", 2m)
        };

        list.Distinct().Should().HaveCount(2, "igualdade estrutural deduplica");
    }

    [Fact]
    public void DomainSignature_on_value_object_property_throws()
    {
        var a = new BadValueObject { Code = "x" };
        var b = new BadValueObject { Code = "x" };

        var act = () => a.Equals(b);

        act.Should().Throw<InvalidOperationException>(
            "[DomainSignature] é proibido em value objects: a assinatura já é o conjunto de todas as propriedades");
    }
}
