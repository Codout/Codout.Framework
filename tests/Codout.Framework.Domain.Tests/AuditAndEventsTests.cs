using Codout.Framework.Domain.Entities;
using Codout.Framework.Domain.Entities.Events;
using Codout.Framework.Domain.Interfaces;
using FluentAssertions;
using Xunit;

namespace Codout.Framework.Domain.Tests;

/// <summary>
/// Bases de auditoria (AuditEntity / AuditEntityBase) e eventos de entidade
/// (EntityCreated / EntityChanged / EntityDeleted).
/// </summary>
public class AuditAndEventsTests
{
    private class AuditedOrder : AuditEntity<int>
    {
        public string? Number { get; set; }
    }

    private class AuditedCustomer : AuditEntityBase
    {
        public string? Name { get; set; }
    }

    [Fact]
    public void AuditEntity_exposes_audit_properties_and_implements_IAudit()
    {
        var createdAt = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        var updatedAt = createdAt.AddDays(1);

        var order = new AuditedOrder
        {
            Number = "PED-1",
            CreatedAt = createdAt,
            CreatedBy = "ana",
            UpdatedAt = updatedAt,
            UpdatedBy = "bia"
        };

        order.Should().BeAssignableTo<IAudit>();
        order.Should().BeAssignableTo<Entity<int>>();
        order.CreatedAt.Should().Be(createdAt);
        order.CreatedBy.Should().Be("ana");
        order.UpdatedAt.Should().Be(updatedAt);
        order.UpdatedBy.Should().Be("bia");
    }

    [Fact]
    public void AuditEntity_audit_fields_start_unset()
    {
        var order = new AuditedOrder();

        order.CreatedAt.Should().BeNull();
        order.UpdatedAt.Should().BeNull();
        order.CreatedBy.Should().BeNull();
        order.UpdatedBy.Should().BeNull();
    }

    [Fact]
    public void AuditEntityBase_is_a_nullable_guid_entity_with_audit()
    {
        var customer = new AuditedCustomer { Name = "Ana" };

        customer.Should().BeAssignableTo<EntityBase>();
        customer.Should().BeAssignableTo<IAudit>();
        customer.Id.Should().BeNull("AuditEntityBase é store-generated");
    }

    [Fact]
    public void Entity_events_carry_the_entity()
    {
        var order = new AuditedOrder { Number = "PED-1" };

        new EntityCreated<AuditedOrder>(order).Entity.Should().BeSameAs(order);
        new EntityChanged<AuditedOrder>(order).Entity.Should().BeSameAs(order);
        new EntityDeleted<AuditedOrder>(order).Entity.Should().BeSameAs(order);
    }
}
