using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Codout.Framework.Data.Auditing;

namespace Codout.Framework.EF.Interceptors;

/// <summary>
/// Interceptor para auditoria automática de entidades que implementam IAuditable
/// </summary>
public class AuditableInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserProvider? _currentUserProvider;

    public AuditableInterceptor(ICurrentUserProvider? currentUserProvider = null)
    {
        _currentUserProvider = currentUserProvider;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        UpdateAuditableEntities(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        UpdateAuditableEntities(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void UpdateAuditableEntities(DbContext? context)
    {
        if (context == null) return;

        var currentUser = _currentUserProvider?.GetCurrentUserId();
        var now = DateTime.UtcNow;

        var entries = context.ChangeTracker
            .Entries()
            .Where(e => e.Entity is IAuditable && 
                       (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entry in entries)
        {
            var auditable = (IAuditable)entry.Entity;

            if (entry.State == EntityState.Added)
            {
                auditable.CreatedAt = now;
                auditable.CreatedBy = currentUser;
            }

            if (entry.State == EntityState.Modified)
            {
                auditable.UpdatedAt = now;
                auditable.UpdatedBy = currentUser;
            }
        }
    }
}

/// <summary>
/// Interface para entidades auditáveis
/// </summary>
public interface IAuditable
{
    DateTime CreatedAt { get; set; }
    string? CreatedBy { get; set; }
    DateTime? UpdatedAt { get; set; }
    string? UpdatedBy { get; set; }
}

/// <summary>
/// Provider para obter o usuário atual (implementar conforme seu sistema de autenticação)
/// </summary>
public interface ICurrentUserProvider
{
    string? GetCurrentUserId();
}
