using Codout.Framework.NetStandard.Domain.Entity;
using System;

namespace Codout.Framework.NetStandard.Repository
{
    /// <summary>
    /// Unit of Work para repositório genérico
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// Efetua o SaveChanges do contexto (sessão) em questão
        /// </summary>
        void SaveChanges();

        /// <summary>
        /// Repositório Genérico que será controlado
        /// </summary>
        /// <typeparam name="T">Tipo do objeto</typeparam>
        /// <returns>Repositório concreto</returns>
        IRepository<T> Repository<T>() where T : class, IEntity;
    }
}
