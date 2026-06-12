using Codout.Framework.Data.Auditing;
using Codout.Framework.Data.Entity;
using FluentAssertions;
using Xunit;

namespace Codout.Framework.Data.Tests;

/// <summary>
/// Abstrações de entidade e auditoria: variância, marker interfaces e visibilidade.
/// </summary>
public class EntityAbstractionsTests
{
    [Fact]
    public void IEntity_of_TId_is_covariant_so_specialized_ids_upcast()
    {
        var entity = new StringIdEntity("abc");

        // Compila apenas porque IEntity<out TId> é covariante.
        IEntity<object> upcast = entity;

        upcast.Id.Should().Be("abc");
    }

    [Fact]
    public void IEntity_of_TId_extends_IEntity()
    {
        typeof(IEntity<object>).Should().Implement<IEntity>();
    }

    [Fact]
    public void IClientGeneratedId_is_a_pure_marker_interface()
    {
        typeof(IClientGeneratedId).GetMembers().Should().BeEmpty(
            "o marker existe apenas para opt-in da ClientGeneratedIdConvention");
        typeof(IClientGeneratedId).IsInterface.Should().BeTrue();
    }

    [Fact]
    public void IHasAssignedId_is_internal_to_the_assembly()
    {
        var type = typeof(IEntity).Assembly.GetType("Codout.Framework.Data.Entity.IHasAssignedId`1");

        type.Should().NotBeNull();
        type!.IsPublic.Should().BeFalse("é detalhe interno, não faz parte da API pública");
    }

    [Fact]
    public void IAuditable_contract_round_trips_audit_data()
    {
        IAuditable auditable = new AuditableStub();
        var now = DateTime.UtcNow;

        auditable.CreatedAt = now;
        auditable.CreatedBy = "ana";
        auditable.UpdatedAt = now.AddMinutes(1);
        auditable.UpdatedBy = "bia";

        auditable.CreatedAt.Should().Be(now);
        auditable.CreatedBy.Should().Be("ana");
        auditable.UpdatedAt.Should().Be(now.AddMinutes(1));
        auditable.UpdatedBy.Should().Be("bia");
    }

    [Fact]
    public void ISoftDeletable_contract_round_trips_deletion_data()
    {
        ISoftDeletable deletable = new SoftDeletableStub();
        var now = DateTime.UtcNow;

        deletable.IsDeleted = true;
        deletable.DeletedAt = now;
        deletable.DeletedBy = "ana";

        deletable.IsDeleted.Should().BeTrue();
        deletable.DeletedAt.Should().Be(now);
        deletable.DeletedBy.Should().Be("ana");
    }

    private class AuditableStub : IAuditable
    {
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }

    private class SoftDeletableStub : ISoftDeletable
    {
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }
    }
}
