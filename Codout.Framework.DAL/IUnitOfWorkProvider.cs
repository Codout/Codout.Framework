namespace Codout.Framework.DAL;

public interface IUnitOfWorkProvider<out T> where T : IUnitOfWork
{
    T Create();
}