namespace Codout.Framework.DAL
{
    public interface IUnitOfWorkFactory<out T> where T : IUnitOfWork
    {
        T Create();
    }
}