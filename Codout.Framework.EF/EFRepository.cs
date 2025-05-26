using System;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Codout.Framework.DAL.Entity;
using Codout.Framework.DAL.Repository;

namespace Codout.Framework.EF;

/// <summary>
///     Repositório genérico de dados para EntityFrameworkCore
/// </summary>
/// <typeparam name="T">Classe que define o tipo do repositório</typeparam>
public class EFRepository<T>(DbContext context) : IRepository<T>
    where T : class, IEntity
{
    private bool _disposed;
    protected DbContext Context = context;

    protected DbSet<T> DbSet => Context.Set<T>();

    /// <summary>
    ///     Retorna todos os objetos do repositório (pode ser lento)
    /// </summary>
    /// <returns>Lista de objetos</returns>
    public IQueryable<T> All()
    {
        return DbSet.AsQueryable();
    }

    /// <summary>
    ///     Retorna uma lista de objetos do repositório de acordo com o filtro apresentado
    /// </summary>
    /// <param name="predicate">Lista de objetos</param>
    /// <returns></returns>
    public IQueryable<T> Find(Expression<Func<T, bool>> predicate)
    {
        return DbSet.Where(predicate).AsQueryable();
    }

    /// <summary>
    ///     Retorna uma lista de objetos do repositório de acordo com o filtro e com opção de paginação
    /// </summary>
    /// <param name="filter">Filtro de bojetos</param>
    /// <param name="total">Retorna o todal de objetos</param>
    /// <param name="index">Indica o índice da paginação</param>
    /// <param name="size">Tamanho da página</param>
    /// <returns>Lista de objetos</returns>
    public IQueryable<T> Find(Expression<Func<T, bool>> filter, out int total, int index = 0, int size = 50)
    {
        var skipCount = index * size;

        var resetSet = DbSet.Where(filter);

        resetSet = skipCount == 0
            ? resetSet.Take(size)
            : resetSet.Skip(skipCount).Take(size);

        total = resetSet.Count();

        return resetSet;
    }

    /// <summary>
    ///     Retorna um objeto do repositório de acordo com o filtro
    /// </summary>
    /// <param name="predicate">Filtro</param>
    /// <returns>objeto</returns>
    public T Get(Expression<Func<T, bool>> predicate)
    {
        return DbSet.SingleOrDefault(predicate);
    }

    /// <summary>
    ///     Retorna um objeto de acordo com a Key
    /// </summary>
    /// <param name="key">Key do objeto</param>
    /// <returns>objeto</returns>
    public T Get(object key)
    {
        return DbSet.Find(key);
    }

    /// <summary>
    ///     Efetua a carga do objeto conforme a key
    /// </summary>
    /// <param name="key">Key do objeto</param>
    /// <returns>Objeto</returns>
    public T Load(object key)
    {
        return Get(key);
    }

    /// <summary>
    ///     Delete o objeto indicado do repositório de dados
    /// </summary>
    /// <param name="entity">Objeto a ser deletado</param>
    public void Delete(T entity)
    {
        DbSet.Remove(entity);
    }

    /// <summary>
    ///     Deletra uma lista de objetos confrome o filtro
    /// </summary>
    /// <param name="predicate">Filtro de objetos a serem deletados</param>
    public void Delete(Expression<Func<T, bool>> predicate)
    {
        DbSet.RemoveRange(DbSet.Where(predicate));
    }

    /// <summary>
    ///     Salva o objeto no repositório
    /// </summary>
    /// <param name="entity">Objeto a ser salvo</param>
    /// <returns>Retorna o mesmo objeto, para o caso de retornar algum Id gerado</returns>
    public T Save(T entity)
    {
        if (entity == null) throw new ArgumentNullException("entity");
        return DbSet.Add(entity);
    }

    /// <summary>
    ///     Salva ou atualiza o objeto em questão (USAR SOMENTE SE O ID NÃO FOI SETADO)
    /// </summary>
    /// <param name="entity">Objeto a ser salvo/atualizado</param>
    /// <returns>Retorna o mesmo objeto, para o caso de retornar algum Id gerado</returns>
    public T SaveOrUpdate(T entity)
    {
        if (entity.IsTransient())
            DbSet.Add(entity);

        return entity;
    }

    /// <summary>
    ///     Atualiza o objeto no repositório
    /// </summary>
    /// <param name="entity">Objeto a ser atulizado</param>
    public void Update(T entity)
    {
        Context.Entry(entity).State = EntityState.Modified;
    }

    /// <summary>
    ///     Efetua o Merge do objeto no repositório
    /// </summary>
    /// <param name="entity">Objeto a ser mesclado</param>
    /// <returns>Retorna o mesmo objeto, para o caso de retornar algum Id gerado</returns>
    public T Merge(T entity)
    {
        return DbSet.Attach(entity);
    }

    public T Refresh(T entity)
    {
        Context.Entry(entity).Reload();
        return entity;
    }

    public async Task<T> RefreshAsync(T entity)
    {
        await Context.Entry(entity).ReloadAsync();
        return entity;
    }

    public async Task<T> GetAsync(Expression<Func<T, bool>> predicate)
    {
        return await All().SingleOrDefaultAsync(predicate);
    }

    public async Task<T> GetAsync(object key)
    {
        return await DbSet.FindAsync(key);
    }

    public async Task<T> LoadAsync(object key)
    {
        return await DbSet.FindAsync(key);
    }

    public Task DeleteAsync(T entity)
    {
        DbSet.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Expression<Func<T, bool>> predicate)
    {
        var entities = Find(predicate);
        foreach (var entity in entities)
            await DeleteAsync(entity);
    }

    public Task<T> SaveAsync(T entity)
    {
        return Task.FromResult(DbSet.Add(entity));
    }

    public Task<T> SaveOrUpdateAsync(T entity)
    {
        if (entity.IsTransient())
            DbSet.Add(entity);

        return Task.FromResult(entity);
    }

    public Task UpdateAsync(T entity)
    {
        Context.Entry(entity).State = EntityState.Modified;
        return Task.CompletedTask;
    }

    public Task<T> MergeAsync(T entity)
    {
        return Task.FromResult(DbSet.Attach(entity));
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
            if (disposing)
            {
                // Libera os componentes
            }

        _disposed = true;
    }
}