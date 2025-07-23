using System;
using System.Data;
using Codout.Framework.DAL;
using Codout.Framework.DAL.Entity;
using NHibernate;

namespace Codout.Framework.NH;

/// <summary>
/// Implementação de Unit of Work para NHibernate,
/// gerencia ciclo de vida de transações e sessões de forma segura.
/// </summary>
public class NHUnitOfWork : IUnitOfWork
{
    private readonly ISession _session;
    private ITransaction _transaction;
    private bool _disposed;

    /// <summary>
    /// Cria uma nova instância de NHUnitOfWork com a sessão injetada.
    /// </summary>
    /// <param name="session">Sessão NHibernate</param>
    public NHUnitOfWork(ISession session)
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));
    }

    /// <summary>
    /// Exposição da sessão ativa.
    /// </summary>
    public ISession Session => _session;

    public void BeginTransaction()
    {
        BeginTransaction(IsolationLevel.ReadCommitted);
    }

    /// <summary>
    /// Inicia uma transação se não houver nenhuma ativa.
    /// </summary>
    public void BeginTransaction(IsolationLevel isolationLevel)
    {
        if (_transaction is not { IsActive: true })
        {
            _transaction = _session.BeginTransaction(isolationLevel);
        }
    }

    /// <summary>
    /// Executa uma operação dentro de transação, fazendo commit ou rollback automaticamente.
    /// </summary>
    public T InTransaction<T>(Func<T> work) where T : class, IEntity
    {
        if (work == null) throw new ArgumentNullException(nameof(work));

        BeginTransaction();
        try
        {
            var result = work();
            Commit();
            return result;
        }
        catch
        {
            Rollback();
            throw;
        }
    }

    public void Commit()
    {
        Commit(IsolationLevel.ReadCommitted);
    }

    /// <summary>
    /// Persiste a transação ativa.
    /// </summary>
    public void Commit(IsolationLevel isolationLevel)
    {
        if (_transaction is not { IsActive: true })
            BeginTransaction(isolationLevel);

        try
        {
            _transaction.Commit();
        }
        catch
        {
            try
            {
                _transaction.Rollback();
            }
            catch
            {
                // Ignorar falha no rollback
            }
            throw;
        }
        finally
        {
            _transaction.Dispose();
            _transaction = null;
        }
    }

    /// <summary>
    /// Desfaz a transação ativa.
    /// </summary>
    public void Rollback()
    {
        if (_transaction is { IsActive: true })
        {
            try
            {
                _transaction.Rollback();
            }
            catch
            {
                // Ignorar falha no rollback
            }
            finally
            {
                _transaction.Dispose();
                _transaction = null;
            }
        }
    }

    /// <summary>
    /// Libera a sessão e faz rollback caso ainda haja transação aberta.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;

        try
        {
            Rollback();
        }
        catch
        {
            // Ignorar exceções no dispose
        }

        _session.Dispose();
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
