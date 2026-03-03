using System;

namespace Codout.Framework.Data;

/// <summary>
/// Factory pattern for creating Unit of Work instances
/// </summary>
/// <typeparam name="T">The Unit of Work type</typeparam>
/// <remarks>
/// This interface is obsolete. Use dependency injection to inject IUnitOfWork directly.
/// </remarks>
[Obsolete("Use dependency injection to inject IUnitOfWork directly instead of using a factory pattern. This will be removed in a future version.")]
public interface IUnitOfWorkProvider<out T> where T : IUnitOfWork
{
    /// <summary>
    /// Creates a new Unit of Work instance
    /// </summary>
    /// <returns>A new Unit of Work instance</returns>
    T Create();
}