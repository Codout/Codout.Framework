using System.Data;

namespace Codout.Framework.DP
{
    public interface IDbStrategy
    {
        IDbConnection GetConnection(string connectionString);
    }
}