using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Codout.Framework.Data.Auditing;

namespace Codout.Framework.EF.Interceptors;

/// <summary>
/// Interceptor para soft delete automático de entidades que implementam ISoftDeletable
/// </summary>
public class SoftDeleteInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserProvider? _currentUserProvider;
    private readonly ILogger<SoftDeleteInterceptor>? _logger;

    public SoftDeleteInterceptor(
        ICurrentUserProvider? currentUserProvider = null,
        ILogger<SoftDeleteInterceptor>? logger = null)
    {
        _currentUserProvider = currentUserProvider;
        _logger = logger;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        HandleSoftDelete(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        HandleSoftDelete(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void HandleSoftDelete(DbContext? context)
    {
        if (context == null) return;

        var currentUser = _currentUserProvider?.GetCurrentUserId();
        var now = DateTime.UtcNow;

        var entries = context.ChangeTracker
            .Entries()
            .Where(e => e.Entity is ISoftDeletable && e.State == EntityState.Deleted);

        foreach (var entry in entries)
        {
            var softDeletable = (ISoftDeletable)entry.Entity;

            entry.State = EntityState.Modified;
            softDeletable.IsDeleted = true;
            softDeletable.DeletedAt = now;
            softDeletable.DeletedBy = currentUser;

            _logger?.LogInformation(
                "Soft delete applied to {EntityType} with ID {EntityId}",
                entry.Entity.GetType().Name,
                entry.Property("Id").CurrentValue);
        }
    }
}
